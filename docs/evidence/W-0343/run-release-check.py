#!/usr/bin/env python3
"""Offline, versioned S5 candidate check; no scheduler, SIP or customer data."""
import argparse
import datetime as dt
import hashlib
import importlib.util
import json
import os
from pathlib import Path
import subprocess
import sys
import time
import uuid

sys.dont_write_bytecode = True
PROFILE = {"tts_cpus": 2, "tts_memory_gib": 4, "capacity": 1, "ort_threads": 1,
           "segment_ms": 30000, "queue_ms": 90000, "preparation_ms": 120000, "queue_limit": 8}


def sha(path):
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for chunk in iter(lambda: stream.read(1048576), b""):
            h.update(chunk)
    return h.hexdigest()


def read(path):
    return json.loads(path.read_text(encoding="utf-8"))


def write(path, data):
    path.write_text(json.dumps(data, ensure_ascii=False, indent=2) + "\n", encoding="utf-8")


def verify_release(root, expected):
    if sha(root / "catalog.json") != expected:
        raise ValueError("Catalog pin mismatch")
    catalog = read(root / "catalog.json")
    if (catalog["REAL_CUSTOMER_CALL_ALLOWED"] != "NO" or catalog["production"] != "BLOCKED"
            or catalog["profile"] != PROFILE or catalog["release"] != "vieneu-w0343"):
        raise ValueError("Release scope/profile mismatch")
    paths = list(root.rglob("*"))
    if any(p.is_symlink() for p in paths):
        raise ValueError("Symlinks are not allowed")
    actual = {p.relative_to(root).as_posix() for p in paths if p.is_file()}
    if actual != set(catalog["files"]) | {"catalog.json", "SHA256SUMS"}:
        raise ValueError("Release file set mismatch")
    for name, item in catalog["files"].items():
        p = (root / name).resolve()
        p.relative_to(root)
        if p.stat().st_size != item["bytes"] or sha(p) != item["sha256"]:
            raise ValueError("Release file mismatch: " + name)
    lines = [sha(p) + "  " + p.relative_to(root).as_posix()
             for p in sorted(paths, key=lambda p: p.relative_to(root).as_posix())
             if p.is_file() and p.name != "SHA256SUMS"]
    if (root / "SHA256SUMS").read_bytes() != ("\n".join(lines) + "\n").encode("ascii"):
        raise ValueError("Checksum list mismatch")
    return catalog


def module(name, path):
    spec = importlib.util.spec_from_file_location(name, path)
    loaded = importlib.util.module_from_spec(spec)
    spec.loader.exec_module(loaded)
    return loaded


def services(docker):
    # No environment variables or customer data from existing containers are captured.
    ids = subprocess.check_output(docker + ["ps", "-q"], text=True).split()
    result = []
    for identifier in ids:
        raw = subprocess.check_output(docker + ["inspect", "--format",
            '{{json .Id}}|{{json .Name}}|{{json .Image}}|{{json .State.StartedAt}}|{{json .RestartCount}}',
            identifier], text=True).strip().split("|")
        result.append(dict(zip(("id", "name", "image", "started_at", "restart_count"),
                               (json.loads(x) for x in raw))))
    return sorted(result, key=lambda x: x["id"])


def inspect_probe(docker, name, image):
    info = json.loads(subprocess.check_output(docker + ["inspect", name], text=True))[0]
    cfg = info["HostConfig"]
    env = set(info["Config"]["Env"])
    if (info["Image"] != image or cfg["NanoCpus"] != 2000000000
            or cfg["Memory"] != 4294967296 or cfg["MemorySwap"] != 4294967296
            or cfg["NetworkMode"] != "none" or not cfg["ReadonlyRootfs"]
            or cfg["Privileged"] or cfg["PortBindings"] or info["Config"]["User"] in ("", "0", "0:0")
            or "REAL_CUSTOMER_CALL_ALLOWED=NO" not in env
            or "VIE_NEU_MAX_CONCURRENCY=1" not in env or "VIE_NEU_ORT_THREADS=1" not in env):
        raise ValueError("Container deployment guard mismatch")
    return info


def default_entrypoint_check(docker, worker, root, output, image, runtime, scope):
    name = "ivr-w0343-" + uuid.uuid4().hex[:12]
    command, _ = worker.commands(docker, root / "base", root / "kit", output,
                                 image, runtime, name, scope, 30, True)
    # Keep the approved isolation/mounts/quota; start the image's actual ENTRYPOINT.
    command = command[:command.index("--entrypoint")] + [image]
    created = False
    try:
        subprocess.run(command, check=True, stdout=subprocess.DEVNULL, timeout=60)
        created = True
        write(output / "deployment-inspect.json", inspect_probe(docker, name, image))
        for iteration in ("start", "restart"):
            if iteration == "restart":
                subprocess.run(docker + ["restart", "-t", "10", name], check=True,
                               stdout=subprocess.DEVNULL, timeout=60)
            started = time.monotonic()
            while time.monotonic() - started < 120:
                result = subprocess.run(docker + ["exec", name, "python", "-c",
                    "import urllib.request; urllib.request.urlopen('http://127.0.0.1:8090/health/ready', timeout=2)"],
                    stdout=subprocess.DEVNULL, stderr=subprocess.DEVNULL, timeout=15)
                if result.returncode == 0:
                    break
                if not inspect_probe(docker, name, image)["State"]["Running"]:
                    raise ValueError("Default entrypoint exited")
                time.sleep(1)
            else:
                raise ValueError("Default entrypoint readiness timeout")
            write(output / (iteration + "-ready.json"), {"http_status": 200,
                  "elapsed_ms": round((time.monotonic() - started) * 1000)})
        inspect_probe(docker, name, image)
    finally:
        if created:
            with (output / "entrypoint.log").open("w", encoding="utf-8") as log:
                subprocess.run(docker + ["logs", name], stdout=log, stderr=subprocess.STDOUT)
            subprocess.run(docker + ["stop", "-t", "10", name], stdout=subprocess.DEVNULL, timeout=60)
            write(output / "deployment-final-inspect.json", inspect_probe(docker, name, image))
            subprocess.run(docker + ["rm", name], check=True, stdout=subprocess.DEVNULL, timeout=60)


def execute_release_check():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--catalog-sha256", required=True)
    parser.add_argument("--output", type=Path, required=True)
    parser.add_argument("--expected-host", default="vps61")
    parser.add_argument("--local-lab", action="store_true")
    parser.add_argument("--verify-only", action="store_true")
    args = parser.parse_args()
    root = Path(__file__).resolve().parent
    catalog = verify_release(root, args.catalog_sha256)
    if args.verify_only:
        print("RELEASE_FILES_VERIFIED", len(catalog["files"]), flush=True)
        return
    output = args.output.resolve()
    if output == root or root in output.parents:
        raise ValueError("Results must be outside the immutable release")
    if any("," in str(p) for p in (root, output)):
        raise ValueError("Comma in bind path")
    output.mkdir(parents=True, exist_ok=False)
    common = module("s5_common", root / "kit/s5_common.py")
    worker = module("worker_s5", root / "kit/run-worker-s5.py")
    if args.local_lab:
        docker, inventory, scope = ["docker"], {"scope": "LOCAL_LAB"}, "LOCAL_LAB"
    else:
        docker, inventory = common.inventory()
        common.validate_target(inventory, args.expected_host, 2, 4)
        if inventory["host_memory"]["MemAvailable"] < 6.5 * 1024**3:
            raise ValueError("Need 6.5 GiB available RAM including host headroom")
        scope = "S5_TARGET"
    write(output / "inventory.json", inventory)
    receipt = {"work_id": "W-0343", "scope": scope, "status": "FAILED", "profile": PROFILE,
               "production": "BLOCKED", "REAL_CUSTOMER_CALL_ALLOWED": "NO",
               "catalog_sha256": sha(root / "catalog.json"), "tts_image_index": catalog["tts_image_index"],
               "tts_image_config": catalog["tts_image_config"], "release_path": str(root),
               "started_utc": dt.datetime.now(dt.timezone.utc).isoformat()}
    before = services(docker)
    write(output / "services-before.json", before)
    try:
        base_manifest = common.verify_bundle(root / "base")
        kit_manifest = worker.verify_kit(root / "kit")
        image = worker.pinned_image(docker, catalog["tts_image_index"], catalog["tts_image_config"],
                                    root / "base/tts-image.tar", output / "tts-load.log")
        if (base_manifest["image_index"] != catalog["tts_image_index"]
                or base_manifest["image_config"] != catalog["tts_image_config"]
                or kit_manifest["base_manifest_sha256"] != sha(root / "base/manifest.json")):
            raise ValueError("Image/base/kit binding mismatch")
        runtime = worker.pinned_image(docker, kit_manifest["runtime_index"], kit_manifest["runtime_config"],
                                      root / "kit/runtime-image.tar", output / "runtime-load.log")
        receipt["image_used"] = image
        # The real model files and evidence inside the exact image must pass before serving.
        uid = "{}:{}".format(os.getuid(), os.getgid()) if hasattr(os, "getuid") else "1654:1654"
        verify = docker + ["run", "--rm", "--pull", "never", "--network", "none", "--read-only",
            "--user", uid, "--cpus", "2", "--memory", "4g", "--memory-swap", "4g", "--cap-drop", "ALL",
            "--security-opt", "no-new-privileges", "-e", "REAL_CUSTOMER_CALL_ALLOWED=NO", "--mount",
            "type=bind,src={},dst=/models,readonly".format(root / "base/models"), "--entrypoint", "python", image,
            "/opt/ivr-tts/scripts/verify-model.py", "--lock", "/opt/ivr-tts/models/MODELS.lock", "--bundle", "/models"]
        with (output / "license-model-check.log").open("w", encoding="utf-8") as log:
            subprocess.run(verify + ["--mode", "nonprod"], check=True, stdout=log, stderr=subprocess.STDOUT, timeout=180)
        with (output / "release-approval-check.log").open("w", encoding="utf-8") as log:
            # This is the offline file/approval verifier, not the production server mode.
            subprocess.run(verify + ["--mode", "production"], check=True, stdout=log, stderr=subprocess.STDOUT, timeout=180)
        print("MODEL_AND_LICENSE_VERIFIED; checking default entrypoint and restart", flush=True)
        default_entrypoint_check(docker, worker, root, output, image, runtime, scope)
        print("ENTRYPOINT_RESTART_PASS; starting worker speech probe with unchanged deadlines", flush=True)
        command = [sys.executable, "-B", str(root / "kit/run-worker-s5.py"), "--run",
                   "--base-bundle", str(root / "base"), "--output", str(output / "measurement"),
                   "--runs", "1" if args.local_lab else "2", "--soak-seconds", "30" if args.local_lab else "450",
                   "--final-profile"]
        command += ["--local-lab"] if args.local_lab else ["--expected-host", args.expected_host]
        subprocess.run(command, check=True, timeout=3000)
        host = read(output / "measurement/host.json")
        if host["tts_image"] != image or any(r["errors"] or r["pcm_equal"] != r["jobs"] for r in host["runs"]):
            raise ValueError("Incomplete audio or worker errors")
        if not host["runs"] or len(host["runs"]) != (1 if args.local_lab else 2):
            raise ValueError("Measurement run count mismatch")
        for run in host["runs"]:
            model = read(output / ("measurement/run-" + str(run["run"])) / "model.json")
            for sample in model["samples"]:
                counters = dict(line.split() for line in sample["memory.events"].splitlines())
                if int(counters.get("oom", 0)) or int(counters.get("oom_kill", 0)):
                    raise ValueError("OOM observed")
        receipt["runs"] = host["runs"]
        receipt["status"] = "PASS"
    except Exception as error:
        receipt["error"] = str(error)
        raise
    finally:
        after = services(docker)
        write(output / "services-after.json", after)
        receipt["existing_services_unchanged"] = before == after
        if before != after:
            receipt["status"] = "FAILED"
            receipt["service_change_requires_review"] = True
        receipt["ended_utc"] = dt.datetime.now(dt.timezone.utc).isoformat()
        write(output / "receipt.json", receipt)
    if receipt["status"] != "PASS":
        raise ValueError("Service state changed during check; inspect returned evidence")
    print("S5_RELEASE_CHECK_PASS" if scope == "S5_TARGET" else "LOCAL_RELEASE_CHECK_PASS", flush=True)
    print("PRODUCTION_BLOCKED; REAL_CUSTOMER_CALL_ALLOWED=NO", flush=True)


if __name__ == "__main__":
    execute_release_check()
