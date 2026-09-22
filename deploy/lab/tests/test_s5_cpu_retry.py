"""The retry must not change app/models/tests or reuse an unverified base kit."""
import hashlib
import importlib.util
import io
import json
from pathlib import Path
import tarfile
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[3]
spec = importlib.util.spec_from_file_location('cpu_retry', ROOT / 'docs/evidence/W-0344/install-s5-cpu-retry.py')
retry = importlib.util.module_from_spec(spec)
spec.loader.exec_module(retry)


class CpuRetryGuards(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory(); self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name); self.base = self.root / 'base'; self.base.mkdir()
        files = {'launcher.py': b'guards-stay-intact', 'cases.py': b'old-cases', 'images/asterisk.tar': b'old-native-image'}
        for name, data in files.items():
            path = self.base / name; path.parent.mkdir(exist_ok=True); path.write_bytes(data)
        self.original = {'candidate': 'fixed-app-sha', 'REAL_CUSTOMER_CALL_ALLOWED': 'NO',
                         'images': {'asterisk': {'index': 'old', 'archive': 'images/asterisk.tar'}, 'ivr-api': {'index': 'same'}},
                         'files': {k: hashlib.sha256(v).hexdigest() for k, v in files.items()}}
        (self.base / 'manifest.json').write_text(json.dumps(self.original))
        self.revised = json.loads(json.dumps(self.original))
        self.revised['images']['asterisk']['index'] = 'new-portable'
        self.revised['files']['images/asterisk.tar'] = hashlib.sha256(b'new-portable-image').hexdigest()
        self.revised['files']['cases.py'] = hashlib.sha256(b'wait-for-rtp').hexdigest()

    def patch(self, mutate=None, extra=False):
        if mutate: mutate(self.revised)
        manifest = json.dumps(self.revised).encode()
        change = {'REAL_CUSTOMER_CALL_ALLOWED': 'NO', 'base_manifest_sha256': retry.digest(self.base / 'manifest.json'),
                  'new_manifest_sha256': hashlib.sha256(manifest).hexdigest()}
        data = {'patch.json': json.dumps(change).encode(), 'manifest.json': manifest,
                'images/asterisk.tar': b'new-portable-image', 'cases.py': b'wait-for-rtp'}
        if extra: data['../escape'] = b'bad'
        path = self.root / 'patch.tar.gz'
        with tarfile.open(path, 'w:gz') as tar:
            for name, content in data.items():
                info = tarfile.TarInfo(name); info.size = len(content); tar.addfile(info, io.BytesIO(content))
        return path

    def test_rebuild_preserves_original_and_all_other_files(self):
        retry.prepare_retry(self.base, self.patch(), self.root / 'new')
        self.assertEqual((self.base / 'images/asterisk.tar').read_bytes(), b'old-native-image')
        self.assertEqual((self.root / 'new/images/asterisk.tar').read_bytes(), b'new-portable-image')
        self.assertEqual((self.root / 'new/launcher.py').read_bytes(), b'guards-stay-intact')
        self.assertEqual((self.root / 'new/cases.py').read_bytes(), b'wait-for-rtp')

    def test_app_image_change_refused(self):
        patch = self.patch(lambda d: d['images']['ivr-api'].update(index='unapproved'))
        with self.assertRaisesRegex(ValueError, 'only the Asterisk'): retry.prepare_retry(self.base, patch, self.root / 'new')

    def test_guard_replacement_refused(self):
        patch = self.patch(lambda d: d['files'].update({'launcher.py': 'changed'}))
        with self.assertRaisesRegex(ValueError, 'only the Asterisk'): retry.prepare_retry(self.base, patch, self.root / 'new')

    def test_customer_calls_cannot_be_enabled(self):
        patch = self.patch(lambda d: d.update(REAL_CUSTOMER_CALL_ALLOWED='YES'))
        with self.assertRaises(ValueError): retry.prepare_retry(self.base, patch, self.root / 'new')

    def test_base_corruption_refused_before_copy(self):
        patch = self.patch(); (self.base / 'launcher.py').write_bytes(b'corrupt')
        with self.assertRaisesRegex(ValueError, 'Base kit changed'): retry.prepare_retry(self.base, patch, self.root / 'new')
        self.assertFalse((self.root / 'new').exists())

    def test_wrong_base_manifest_refused(self):
        patch = self.patch(); (self.base / 'manifest.json').write_text('{}')
        with self.assertRaisesRegex(ValueError, 'Wrong base kit'): retry.prepare_retry(self.base, patch, self.root / 'new')

    def test_archive_path_escape_refused(self):
        with self.assertRaisesRegex(ValueError, 'unexpected members'): retry.prepare_retry(self.base, self.patch(extra=True), self.root / 'new')

    def test_existing_destination_refused(self):
        target = self.root / 'new'; target.mkdir()
        with self.assertRaisesRegex(ValueError, 'overwrite'): retry.prepare_retry(self.base, self.patch(), target)

    def test_replacement_hash_mismatch_refused(self):
        patch = self.patch(lambda d: d['files'].update({'images/asterisk.tar': '0'*64}))
        with self.assertRaisesRegex(ValueError, 'Reconstructed file hash'): retry.prepare_retry(self.base, patch, self.root / 'new')


if __name__ == '__main__':
    unittest.main(verbosity=2)
