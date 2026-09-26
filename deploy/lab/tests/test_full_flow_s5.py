"""Refusal tests for the portable S5 kit; no Docker or target host required."""
import copy
import importlib.util
import json
from pathlib import Path
import sys
import tarfile
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[3]
spec = importlib.util.spec_from_file_location('flow_s5', ROOT / 'deploy/lab/full-flow-s5/launcher.py')
flow = importlib.util.module_from_spec(spec)
spec.loader.exec_module(flow)
common_spec = importlib.util.spec_from_file_location('flow_common', ROOT / 'deploy/lab/run-vieneu-s5.py')
common = importlib.util.module_from_spec(common_spec)
common_spec.loader.exec_module(common)


class FullFlowS5Guards(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.root = Path(self.temp.name)

    def config(self):
        # These fields exercise boundaries independently from the packaged compose file.
        services = {s: {'pull_policy': 'never'} for s in flow.SERVICES}
        for name in ('ivr-api', 'ivr-worker'):
            services[name]['environment'] = {
                'IVR_EXECUTION_MODE': 'LAB_REAL_SIM', 'REAL_CUSTOMER_CALL_ALLOWED': 'NO',
                'Ivr__Telephony__Asterisk__DestinationAlias': 'LAB-A',
                'ConnectionStrings__IvrDb': 'Host=postgres;Database=ivr;Username=ivr;GSS Encryption Mode=Disable',
                'Ivr__CallbackDelivery__TargetBaseUrl': 'http://fake-sales:8080'}
        services['ivr-worker']['environment'].update({
            'Ivr__Speech__Tts__TimeoutMilliseconds': '30000',
            'Ivr__Speech__Tts__PreparationTimeoutMilliseconds': '120000',
            'Ivr__Speech__Tts__PreparationQueueTimeoutMilliseconds': '90000',
            'Ivr__Speech__Tts__PreparationQueueLimit': '8'})
        services['ivr-tts'].update(cpus=2, mem_limit='4g', memswap_limit='4g',
                                   read_only=True, network_mode='service:ivr-worker')
        return {'services': services, 'networks': {'lab': {'internal': True}, 'internal': {'internal': True}, 'frontend': {'internal': False}}}

    def test_customer_call_flag_cannot_be_relaxed(self):
        config = self.config(); config['services']['ivr-worker']['environment']['REAL_CUSTOMER_CALL_ALLOWED'] = 'YES'
        with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_external_phone_alias_refused(self):
        config = self.config(); config['services']['ivr-api']['environment']['Ivr__Telephony__Asterisk__DestinationAlias'] = '0841234567'
        with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_external_database_and_callback_refused(self):
        for key in ('ConnectionStrings__IvrDb', 'Ivr__CallbackDelivery__TargetBaseUrl'):
            with self.subTest(key=key):
                config = self.config(); config['services']['ivr-worker']['environment'][key] = 'external.example'
                with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_unmeasured_timeout_refused(self):
        config = self.config(); config['services']['ivr-worker']['environment']['Ivr__Speech__Tts__TimeoutMilliseconds'] = '5000'
        with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_unbounded_tts_refused(self):
        config = self.config(); config['services']['ivr-tts']['mem_limit'] = '16g'
        with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_external_routing_and_sip_publication_refused(self):
        config = self.config(); config['networks']['lab']['internal'] = False
        with self.assertRaises(ValueError): flow.validate_profile(config)
        config = self.config(); config['services']['asterisk']['ports'] = ['5060:5060/udp']
        with self.assertRaises(ValueError): flow.validate_profile(config)
        config = self.config(); config['services']['asterisk']['networks'] = ['frontend']
        with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_host_network_and_privileged_refused(self):
        for key, value in [('network_mode', 'host'), ('privileged', True)]:
            config = self.config(); config['services']['postgres'][key] = value
            with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_online_pull_refused(self):
        config = self.config(); config['services']['postgres']['pull_policy'] = 'always'
        with self.assertRaises(ValueError): flow.validate_profile(config)

    def test_manifest_escape_refused(self):
        outside = self.root / 'outside'; outside.write_text('x')
        kit = self.root / 'kit'; kit.mkdir()
        with self.assertRaises(ValueError): flow.safe_path(kit, '../outside')

    def test_wrong_candidate_and_missing_evidence_refused(self):
        for manifest in ({'candidate': 'stale', 'REAL_CUSTOMER_CALL_ALLOWED': 'NO'},
                         {'candidate': flow.SHA, 'REAL_CUSTOMER_CALL_ALLOWED': 'NO', 'files': {}}):
            (self.root / 'manifest.json').write_text(json.dumps(manifest))
            with self.assertRaises(ValueError): flow.verify_kit(self.root)

    def test_cleanup_requires_exact_peer_project_not_just_work_label(self):
        project = 'm8_ivr-flow-test'
        other = {'Name': '/' + project + '-peer', 'Config': {'Labels': {'ivr.work': 'W0344', 'ivr.flow.project': 'm8_ivr-flow-other'}}}
        self.assertFalse(flow.owns_container(other, project))
        other['Config']['Labels']['ivr.flow.project'] = project
        self.assertTrue(flow.owns_container(other, project))
        self.assertFalse(flow.owns_container(other, 'm8_ivr-flow-other'))

    def test_s5_run_stays_inside_the_m8_allocation(self):
        # Q-24 (PA3, 26/09): results below /home/ssv/m8 and the API port in 6800-6899.
        flow.require_s5_allocation(Path('/home/ssv/m8/w0344-run-1'), flow.DEFAULT_PORT)
        for output, port in (('/home/ssv/w0344-run-1', 6843), ('/home/ssv/m8', 6843),
                             ('/home/ssv/m8/run', 58443), ('/home/ssv/m8/run', 6799),
                             ('/home/ssv/m8/run', 6900)):
            with self.subTest(output=output, port=port):
                with self.assertRaisesRegex(ValueError, 'Q-24'):
                    flow.require_s5_allocation(Path(output), port)

    def test_a_refused_s5_run_leaves_no_directory_and_a_rehearsal_is_not_bound(self):
        output = self.root / 'result'
        with self.assertRaisesRegex(ValueError, 'Q-24'):
            flow.prepare_output(output, flow.DEFAULT_PORT, local_lab=False)
        self.assertFalse(output.exists())
        flow.prepare_output(output, 58443, local_lab=True)
        self.assertTrue(output.is_dir())

    def test_project_names_are_m8_and_the_case_runner_expects_them(self):
        name = flow.project_name(flow.dt.datetime(2026, 9, 26, 3, 4, 5, tzinfo=flow.dt.timezone.utc))
        self.assertRegex(name, r'^m8_ivr-flow-20260926030405-[0-9a-f]{6}$')
        cases = (ROOT / 'deploy/lab/full-flow-s5/cases.py').read_text(encoding='utf-8')
        self.assertIn("re.fullmatch(r'm8_ivr-flow-[a-z0-9-]+',PROJECT)", cases)

    def test_launcher_and_s5_common_share_one_allocation(self):
        self.assertEqual(flow.M8_PREFIX, common.M8_PREFIX)
        self.assertEqual(flow.M8_ROOT, common.M8_ROOT)
        self.assertEqual(flow.M8_PORTS, common.M8_PORTS)
        self.assertIn(flow.DEFAULT_PORT, common.M8_PORTS)

    def test_receipt_excludes_credentials_and_database(self):
        output = self.root / 'result'; output.mkdir()
        (output / 'summary.json').write_text('{}')
        (output / 'compose.private.json').write_text('secret')
        (output / 'database.private.sql').write_text('secret')
        private = output / 'run-2' / 'peer'; private.mkdir(parents=True)
        (private / 'pjsip.conf').write_text('secret')
        (private.parent / 'pjsip-original.conf').write_text('secret')
        (private.parent / 'result.json').write_text('{}')
        receipt = flow.package_receipt(output)
        with tarfile.open(receipt) as archive:
            self.assertEqual(set(archive.getnames()), {'result/summary.json', 'result/run-2/result.json', 'result/receipt-manifest.json'})

    def test_wrong_target_and_remote_daemon_refused(self):
        snapshot = {'host_name': 'vps61', 'architecture': 'x86_64', 'logical_cpus': 10,
                    'host_memory': {'MemTotal': 14 * 1024**3, 'MemAvailable': 12 * 1024**3},
                    'docker': {'OSType': 'linux', 'Name': 'vps61', 'NCPU': 10, 'MemTotal': 14 * 1024**3}}
        common.validate_target(snapshot, 'vps61', 2, 4)
        for location in ('host_name', 'daemon'):
            altered = copy.deepcopy(snapshot)
            if location == 'host_name': altered['host_name'] = 'other'
            else: altered['docker']['Name'] = 'other'
            with self.assertRaises(ValueError): common.validate_target(altered, 'vps61', 2, 4)

    def test_host_daemon_ram_mismatch_refused(self):
        snapshot = {'host_name': 'vps61', 'architecture': 'x86_64', 'logical_cpus': 10,
                    'host_memory': {'MemTotal': 14 * 1024**3, 'MemAvailable': 12 * 1024**3},
                    'docker': {'OSType': 'linux', 'Name': 'vps61', 'NCPU': 10, 'MemTotal': 48 * 1024**3}}
        with self.assertRaises(ValueError): common.validate_target(snapshot, 'vps61', 2, 4)


if __name__ == '__main__':
    unittest.main(verbosity=2)
