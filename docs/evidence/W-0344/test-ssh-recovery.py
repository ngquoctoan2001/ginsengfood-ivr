"""Local TEST_ONLY checks; Linux subprocess fixture simulates losing an SSH process group."""
import argparse
import importlib.util
import json
import os
from pathlib import Path
import shutil
import signal
import subprocess
import sys
import tarfile
import tempfile
import time
import unittest
from unittest.mock import patch
import uuid

HELPER = Path(__file__).with_name("recover-s5-full-flow.py")
spec = importlib.util.spec_from_file_location("recovery", HELPER)
recovery = importlib.util.module_from_spec(spec)
spec.loader.exec_module(recovery)


class RecoveryTests(unittest.TestCase):
    def test_running_completed_partial_and_other_jobs(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            self.assertEqual("NOT_STARTED", recovery.classify(root, [], []))
            self.assertEqual("OTHER_PROBE_RUNNING", recovery.classify(root, [], [20]))
            self.assertEqual("RUNNING", recovery.classify(root, [10], []))
            for name in ("full-flow-s5", "result", "launcher.log", "recovery-start.json"):
                p = root / name
                p.touch()
                self.assertEqual("PARTIAL_REVIEW_REQUIRED", recovery.classify(root, [], []))
                p.unlink()
            (root / "recovery-exit.json").write_text('{"exit_code":1}')
            self.assertEqual("FAILED_BEFORE_RECEIPT", recovery.classify(root, [], []))
            (root / "result.tar.gz").write_bytes(b"TEST_ONLY")
            self.assertEqual("RECEIPT_READY", recovery.classify(root, [], []))
            self.assertEqual("RUNNING", recovery.classify(root, [10], []))

    def test_repeated_start_refused_before_process_spawn(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "recovery-start.json").write_text("{}")
            args = argparse.Namespace(archive_sha256="a" * 64, installer_sha256="b" * 64)
            with patch.object(recovery.subprocess, "Popen") as spawn:
                with self.assertRaises(FileExistsError):
                    recovery.start_detached(root, args)
                spawn.assert_not_called()

    def test_diagnostics_exclude_credentials(self):
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            (root / "compose.private.json").write_text("TEST_ONLY_SECRET")
            (root / "seed.sql").write_text("TEST_ONLY_SECRET")
            (root / "launcher.log").write_text("TEST_ONLY_LOG")
            recovery.diagnostics(root, "PARTIAL_REVIEW_REQUIRED")
            with tarfile.open(root / "recovery-diagnostic.tar.gz") as archive:
                self.assertEqual({"recovery-status.json", "launcher.log"}, set(archive.getnames()))

    @unittest.skipUnless(sys.platform == "linux", "Linux target semantics")
    def test_pins_host_and_directory_must_match(self):
        root = Path("/home/ssv/ivr-full-flow-w0344-20990101-000000-" + uuid.uuid4().hex[:8])
        root.mkdir(parents=True)
        archive = root / "ivr-full-flow-w0344.tar.gz"
        installer = root / "install-s5-full-flow.sh"
        archive.write_bytes(b"TEST_ONLY_ARCHIVE")
        installer.write_bytes(b"TEST_ONLY_INSTALLER")
        pins = recovery.digest(archive), recovery.digest(installer)
        (root / "ivr-full-flow-w0344.tar.gz.sha256").write_text(pins[0] + "  ivr-full-flow-w0344.tar.gz\n")
        recovery.validate(root, *pins)
        with patch.object(recovery.socket, "gethostname", return_value="wrong-target"):
            with self.assertRaisesRegex(ValueError, "Expected vps61"):
                recovery.validate(root, *pins)
        with self.assertRaisesRegex(ValueError, "Unexpected run directory"):
            recovery.validate(Path("/tmp"), *pins)
        archive.write_bytes(b"CHANGED_TEST_ONLY_ARCHIVE")
        with self.assertRaisesRegex(ValueError, "Checksum mismatch"):
            recovery.validate(root, *pins)

    @unittest.skipUnless(sys.platform == "linux", "Requires Linux sessions and SIGHUP")
    def test_detached_job_survives_ssh_process_group_hangup(self):
        root = Path("/home/ssv/ivr-full-flow-w0344-20990101-000000-" + uuid.uuid4().hex[:8])
        root.mkdir(parents=True)
        shutil.copyfile(HELPER, root / HELPER.name)
        archive = root / "ivr-full-flow-w0344.tar.gz"
        archive.write_bytes(b"TEST_ONLY_ARCHIVE")
        installer = root / "install-s5-full-flow.sh"
        installer.write_text("import time\nfrom pathlib import Path\ntime.sleep(2)\nPath('result.tar.gz').write_bytes(b'TEST_ONLY_RESULT')\n")
        archive_pin, installer_pin = recovery.digest(archive), recovery.digest(installer)
        (root / "ivr-full-flow-w0344.tar.gz.sha256").write_text(archive_pin + "  ivr-full-flow-w0344.tar.gz\n")
        # Only the fixture's 'bash' runs Python; no application, SSH or Docker is invoked.
        fakebin = root / "test-only-bin"
        fakebin.mkdir()
        fakebash = fakebin / "bash"
        fakebash.write_text("#!" + sys.executable + "\nimport os,sys\nos.execv(sys.executable,[sys.executable,sys.argv[1]])\n")
        fakebash.chmod(0o755)
        parent_code = """
import argparse,importlib.util,sys,time
from pathlib import Path
root=Path(sys.argv[1])
s=importlib.util.spec_from_file_location('helper',root/'recover-s5-full-flow.py')
m=importlib.util.module_from_spec(s);s.loader.exec_module(m)
p=m.start_detached(root,argparse.Namespace(archive_sha256=sys.argv[2],installer_sha256=sys.argv[3]))
print(p,flush=True)
time.sleep(60)
"""
        env = dict(os.environ, PATH=str(fakebin) + ":" + os.environ["PATH"])
        parent = subprocess.Popen([sys.executable, "-u", "-c", parent_code, str(root), archive_pin, installer_pin],
                                  env=env, stdout=subprocess.PIPE, text=True, start_new_session=True)
        try:
            child = int(parent.stdout.readline().strip())
            self.assertNotEqual(os.getpgid(parent.pid), os.getpgid(child))
            os.killpg(parent.pid, signal.SIGHUP)
            parent.wait(timeout=5)
            deadline = time.monotonic() + 15
            while time.monotonic() < deadline and not (root / "recovery-exit.json").is_file():
                time.sleep(0.1)
            self.assertEqual(0, json.loads((root / "recovery-exit.json").read_text())["exit_code"])
            self.assertEqual(b"TEST_ONLY_RESULT", (root / "result.tar.gz").read_bytes())
        finally:
            if parent.poll() is None:
                parent.kill()
                parent.wait(timeout=5)
            parent.stdout.close()


if __name__ == "__main__":
    unittest.main(verbosity=2)
