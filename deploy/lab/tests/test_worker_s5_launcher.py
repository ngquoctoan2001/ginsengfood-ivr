import importlib.util
import json
from pathlib import Path
import tempfile
import unittest

spec = importlib.util.spec_from_file_location('worker_s5', Path(__file__).resolve().parents[1] / 'run-vieneu-worker-s5.py')
module = importlib.util.module_from_spec(spec)
spec.loader.exec_module(module)


class WorkerS5LauncherTests(unittest.TestCase):
    def test_tts_keeps_original_quota_and_client_only_shares_isolated_loopback(self):
        tts, client = module.commands(['docker'], Path('/base'), Path('/kit'), Path('/out'),
                                     'sha256:tts', 'sha256:runtime', 'probe-only', 'S5_TARGET', 450)
        self.assertEqual(tts[tts.index('--network') + 1], 'none')
        self.assertEqual(tts[tts.index('--cpus') + 1], '2')
        self.assertEqual(tts[tts.index('--memory') + 1], '4g')
        self.assertEqual(tts[tts.index('--memory-swap') + 1], '4g')
        self.assertEqual(client[client.index('--network') + 1], 'container:probe-only')
        self.assertEqual(client[client.index('--cpus') + 1], '0.5')
        for command in (tts, client):
            self.assertIn('REAL_CUSTOMER_CALL_ALLOWED=NO', command)
            self.assertIn('--read-only', command)
            self.assertIn('--user', command)
            self.assertNotIn('--privileged', command)
            self.assertNotIn('-p', command)
            self.assertNotIn('docker.sock', ' '.join(command))

    def test_manifest_must_cover_required_files(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / 'manifest.json').write_text(json.dumps({'REAL_CUSTOMER_CALL_ALLOWED': 'NO', 'files': {}}))
            with self.assertRaisesRegex(ValueError, 'Incomplete'):
                module.verify_kit(root)

    def test_manifest_cannot_allow_customer_calls(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            (root / 'manifest.json').write_text(json.dumps({'REAL_CUSTOMER_CALL_ALLOWED': 'YES', 'files': {}}))
            with self.assertRaisesRegex(ValueError, 'disabled'):
                module.verify_kit(root)


if __name__ == '__main__':
    unittest.main()
