"""W-0344: the case driver may press DTMF only after the peer has received the whole prompt."""
import ast
from pathlib import Path
import re
import unittest

ROOT = Path(__file__).resolve().parents[3]
CASES = ROOT / 'deploy/lab/full-flow-s5/cases.py'
RTP_GATE = re.compile(r"int\(received_rtp\[1\]\)\s*>=\s*measured\['segmented_audio_ms'\]\s*//\s*20")
FINAL_ASSERT = "assert rtp and int(rtp[1])>=measured['segmented_audio_ms']//20"
# The shape that pressed DTMF on a wall-clock estimate in the 22/09 rehearsal-r2.
WALL_CLOCK_ONLY = '''
if peers:
    if digit is not None and not sent and time.monotonic()-answered>=measured['segmented_audio_ms']/1000+2:
        item['rtp']=cli(PEER,'pjsip show channelstats'); item['sent_utc']=utc()
        cli(PEER,f'channel redirect {peers[0]} send-{digit},s,1'); sent=True
'''


def redirect_guards(source):
    """Return, for each DTMF redirect, the tests of every `if` whose body encloses it."""
    tree = ast.parse(source)
    parent = {child: node for node in ast.walk(tree) for child in ast.iter_child_nodes(node)}
    guards = []
    for node in ast.walk(tree):
        if (isinstance(node, ast.Call) and isinstance(node.func, ast.Name) and node.func.id == 'cli'
                and any(isinstance(arg, ast.JoinedStr) and 'channel redirect' in ast.unparse(arg) for arg in node.args)):
            tests, child = [], node
            while child in parent:
                owner = parent[child]
                if isinstance(owner, ast.If) and child in owner.body:
                    tests.append(ast.unparse(owner.test))
                child = owner
            guards.append(tests)
    return guards


class DtmfWaitsForReceivedAudio(unittest.TestCase):
    def test_single_redirect_is_gated_on_received_rtp(self):
        guards = redirect_guards(CASES.read_text(encoding='utf-8'))
        self.assertEqual(len(guards), 1, 'exactly one DTMF redirect expected')
        self.assertTrue(any(RTP_GATE.search(test) for test in guards[0]), guards[0])

    def test_final_audio_assertion_is_unchanged(self):
        self.assertEqual(CASES.read_text(encoding='utf-8').count(FINAL_ASSERT), 1)

    def test_wall_clock_only_shape_is_detected(self):
        guards = redirect_guards(WALL_CLOCK_ONLY)
        self.assertEqual(len(guards), 1)
        self.assertFalse(any(RTP_GATE.search(test) for test in guards[0]), guards[0])


if __name__ == '__main__':
    unittest.main(verbosity=2)
