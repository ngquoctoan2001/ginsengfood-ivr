"""W-0344: every config file bind-mounted into the SIP peer must be readable before the peer starts."""
import ast
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[3]
CASES = ROOT / 'deploy/lab/full-flow-s5/cases.py'


def peer_mount_audit(source):
    """Return (mounted names, names chmod'ed 0644 before `docker run`) for setup_peer."""
    tree = ast.parse(source)
    setup = next(n for n in tree.body if isinstance(n, ast.FunctionDef) and n.name == 'setup_peer')
    mounted, readable, run_line = set(), set(), None
    for node in ast.walk(setup):
        if isinstance(node, ast.Call) and any(isinstance(a, ast.Constant) and a.value == 'run' for a in node.args[:2]):
            run_line = node.lineno if run_line is None else min(run_line, node.lineno)
    for loop in (n for n in ast.walk(setup) if isinstance(n, ast.For) and isinstance(n.iter, ast.Tuple)):
        names = {e.value for e in loop.iter.elts if isinstance(e, ast.Constant)}
        body = ast.unparse(loop)
        if 'type=bind' in body:
            mounted |= names
        if '.chmod(420)' in body.replace('0o644', '420') and (run_line is None or loop.lineno < run_line):
            readable |= names
    return mounted, readable


class PeerConfigIsReadable(unittest.TestCase):
    def test_every_mounted_peer_file_is_made_0644_before_docker_run(self):
        mounted, readable = peer_mount_audit(CASES.read_text(encoding='utf-8'))
        self.assertEqual(mounted, {'pjsip.conf', 'extensions.conf', 'http.conf', 'logger.conf'})
        self.assertEqual(readable, mounted)

    def test_driver_without_chmod_is_detected(self):
        source = '''
def setup_peer():
    mounts=[]
    for name in ('pjsip.conf','logger.conf'): mounts+=['--mount',f'type=bind,src={name},dst=/etc/asterisk/{name},readonly']
    cmd('docker','run','-d',*mounts)
'''
        mounted, readable = peer_mount_audit(source)
        self.assertEqual(mounted, {'pjsip.conf', 'logger.conf'})
        self.assertEqual(readable, set())


if __name__ == '__main__':
    unittest.main(verbosity=2)
