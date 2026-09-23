"""W-0345: the case driver runs either programme, only as a pair the wire matrix allows."""
import ast
from pathlib import Path
import unittest

ROOT = Path(__file__).resolve().parents[3]
SOURCE = (ROOT / 'deploy/lab/full-flow-s5/cases.py').read_text(encoding='utf-8')
TREE = ast.parse(SOURCE)


def assignment(name):
    return next(n.value for n in TREE.body if isinstance(n, ast.Assign) and any(
        isinstance(t, ast.Name) and t.id == name for t in n.targets))


def function(name):
    return next(n for n in TREE.body if isinstance(n, ast.FunctionDef) and n.name == name)


class ProgrammeSwitch(unittest.TestCase):
    def test_only_the_two_contract_pairs_exist(self):
        programmes = ast.literal_eval(assignment('PROGRAMS'))
        self.assertEqual({k: v[0] for k, v in programmes.items()}, {'GOLDEN_HOUR': 'ONLINE', 'TWENTY_FOUR_SEVEN': 'COD'})

    def test_golden_hour_stays_the_default(self):
        self.assertIn("os.environ.get('IVR_FLOW_PROGRAM','GOLDEN_HOUR')", SOURCE)
        self.assertIn('assert PROGRAM in PROGRAMS', SOURCE)

    def test_admission_payload_takes_the_selected_pair(self):
        body = ast.unparse(function('admit'))
        for literal in ("'GOLDEN_HOUR'", "'ONLINE'", "'Giờ Vàng'", "'TWENTY_FOUR_SEVEN'", "'COD'"):
            self.assertNotIn(literal, body)
        for name in ("'program_code': PROGRAM", "'payment_method_snapshot': PAYMENT", "'program_display_name': PROGRAM_NAME"):
            self.assertIn(name, body)

    def test_every_case_checks_the_stored_pair(self):
        body = ast.unparse(function('run_case'))
        self.assertIn("program_type||'|'||payment_method_snapshot FROM ivr_confirmation_tasks", body)
        self.assertIn("assert item['program'] == PROGRAM + '|' + PAYMENT", body)


if __name__ == '__main__':
    unittest.main(verbosity=2)
