"""Offline guard tests; do not contact Docker or label the local host S5."""
import copy
import importlib.util
import json
from pathlib import Path
import tempfile
import unittest
from unittest.mock import patch

spec = importlib.util.spec_from_file_location('s5', Path(__file__).parents[1] / 'run-vieneu-s5.py')
s5 = importlib.util.module_from_spec(spec)
spec.loader.exec_module(s5)


class S5Guards(unittest.TestCase):
    def setUp(self):
        self.snapshot = {'host_name': 'vps61', 'architecture': 'x86_64', 'logical_cpus': 10,
                         'host_memory': {'MemTotal': 14 * 1024**3, 'MemAvailable': 11 * 1024**3},
                         'docker': {'Name': 'vps61', 'OSType': 'linux', 'NCPU': 10, 'MemTotal': 14 * 1024**3}}

    def test_initial_quota_is_valid(self):
        s5.validate_target(self.snapshot, 'vps61', 2, 4)

    def test_wrong_host_and_docker_daemon_stop_before_run(self):
        for field in ('host_name', 'docker'):
            with self.subTest(field=field):
                value = copy.deepcopy(self.snapshot)
                if field == 'docker': value['docker']['Name'] = 'different-server'
                else: value[field] = 'different-server'
                with self.assertRaises(ValueError): s5.validate_target(value, 'vps61', 2, 4)

    def test_reported_memory_mismatch_is_not_silently_accepted(self):
        self.snapshot['docker']['MemTotal'] = 48 * 1024**3
        with self.assertRaisesRegex(ValueError, 'RAM mismatch'):
            s5.validate_target(self.snapshot, 'vps61', 2, 4)

    def test_quota_leaves_host_headroom(self):
        for cpus, ram, available in ((10, 4, 11), (2, 12, 11), (2, 4, 5)):
            with self.subTest(cpus=cpus, ram=ram, available=available):
                self.snapshot['host_memory']['MemAvailable'] = available * 1024**3
                with self.assertRaises(ValueError): s5.validate_target(self.snapshot, 'vps61', cpus, ram)

    def test_remote_context_never_contacts_remote_daemon(self):
        with patch.object(s5.platform, 'system', return_value='Linux'), \
             patch.object(s5, 'capture', side_effect=['remote', 'ssh://remote']), \
             patch.dict(s5.os.environ, {}, clear=True):
            with self.assertRaisesRegex(ValueError, 'local Unix'):
                s5.inventory()

    def test_conflicting_docker_host_is_rejected(self):
        with patch.object(s5.platform, 'system', return_value='Linux'), \
             patch.object(s5, 'capture', side_effect=['default', 'unix:///var/run/docker.sock']), \
             patch.dict(s5.os.environ, {'DOCKER_HOST': 'tcp://different-server:2375'}, clear=True):
            with self.assertRaisesRegex(ValueError, 'DOCKER_HOST'):
                s5.inventory()

    def test_bundle_tamper_is_rejected_before_docker_load(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder).resolve()
            names = ['tts-image.tar', 'probe_load.py', 'worker-texts.json', 'voice-acceptance-manifest.json', 'approved-audio.json']
            for name in names: (root / name).write_text('{}')
            (root / 'worker-texts.json').write_text(json.dumps({'fixtureOnly': True, 'realCustomerCallAllowed': 'NO'}))
            manifest = {'real_customer_call_allowed': 'NO', 'image_index': 'sha256:' + 'a' * 64,
                        'image_config': 'sha256:' + 'b' * 64, 'files': {name: s5.sha256(root / name) for name in names}}
            (root / 'manifest.json').write_text(json.dumps(manifest))
            s5.verify_bundle(root)
            (root / 'probe_load.py').write_text('changed')
            with self.assertRaisesRegex(ValueError, 'changed bundle file'):
                s5.verify_bundle(root)

    def test_container_is_isolated_and_bounded(self):
        with tempfile.TemporaryDirectory() as folder:
            root = Path(folder)
            (root / 'voice-acceptance-manifest.json').write_text(json.dumps({'selections': {r: {'voice_id': r} for r in ('North', 'Central', 'South')}}))
            with patch.object(s5.os, 'getuid', return_value=1000, create=True), patch.object(s5.os, 'getgid', return_value=1000, create=True):
                args = s5.container_command(['docker', '--context', 'default'], root, root / 'result', 'sha256:' + 'a' * 64, 'own-test', 2, 4)
            for option, value in (('--network', 'none'), ('--cpus', '2'), ('--memory', '4g'), ('--memory-swap', '4g'), ('--user', '1000:1000')):
                self.assertEqual(args[args.index(option) + 1], value)
            self.assertIn('REAL_CUSTOMER_CALL_ALLOWED=NO', args)
            self.assertNotIn('-p', args)
            self.assertNotIn('/var/run/docker.sock', ' '.join(args))
            mounts = [args[i + 1] for i, value in enumerate(args) if value == '--mount']
            self.assertEqual(len(mounts), 5)
            self.assertEqual(sum(value.endswith(',readonly') for value in mounts), 4)


if __name__ == '__main__':
    unittest.main()
