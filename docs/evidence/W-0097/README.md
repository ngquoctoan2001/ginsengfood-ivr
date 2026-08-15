# W-0097 — Admin UI visual design pass

| | |
| --- | --- |
| Work ID | `W-0097` · Origin `UNPLANNED` (IVR dev request, 2026-08-15) |
| Status | `TESTS_PASS` (owner/reviewer acceptance pending) |
| Scope | `admin-ui/` presentation only — no API, no behaviour, no governance change |
| Governance | `IVR_EXECUTION_MODE=MOCK` · `REAL_CUSTOMER_CALL_ALLOWED=NO` |

## 1. Tooling

The IVR dev asked for the console to be made better looking using
`https://github.com/nextlevelbuilder/ui-ux-pro-max-skill` (MIT).

Installed at **user level** (`~/.claude/skills/ui-ux-pro-max/`, 73 files, 3.8 MB),
not into this repository. The repo runs PII scanning, gitleaks and a Markdown
doc-map over its whole tree; adding ~600 third-party files would enlarge all
three scan surfaces for no benefit to the product. The skill is self-contained,
runs on the system Python with no external dependencies, and its `search.py` was
smoke-tested before use. Its paths were rewritten from `${CLAUDE_PLUGIN_ROOT}`
to the user-level location.

Only the `ui-ux-pro-max` skill was installed; the repository also ships
`brand`, `banner-design`, `design`, `design-system`, `slides` and `ui-styling`,
none of which apply to an internal operations console.

Queries carried no project data, per the skill's own instruction.

## 2. What the skill recommended, and what was taken

`--design-system` for "internal operations admin console monitoring dense tables
safety critical" at density 8, motion 3, variance 3 returned:

| Recommendation | Taken? |
| --- | --- |
| Style: **Minimalism & Swiss** ("enterprise apps, dashboards, professional tools") | Yes |
| Slate palette (`#0F172A`, `#1E293B`, `#334155`, `#475569`, `#94A3B8`, `#F8FAFC`) | Yes, extended to a full light **and** dark ramp |
| Destructive `#EF4444`, success green | Yes, as **status** colours |
| Accent green `#22C55E` for calls to action | **No** — see below |
| Typography: Fira Sans / Fira Code from Google Fonts | **No** — see below |
| Motion: subtle, 300–400ms, reduced-motion aware | Yes, 180ms transitions; reduced-motion already honoured |
| Pre-delivery checklist (contrast, focus, touch targets, no emoji icons, responsive) | Yes, all items |

**Green is not used for buttons.** In a monitoring console green already means
"healthy". Using it for calls to action would overload the one colour operators
read as a status. Interactive surfaces use the generated *primary* slate instead.

**No web font is loaded.** The generated pairing would add a request to
`fonts.googleapis.com` from an internal console, and P0-1 established that this
app loads no remote fonts. The system stack keeps the same technical character
at zero cost.

Both departures are recorded in the header comment of `globals.css`.

## 3. The substantive change: colour is no longer load-bearing

The skill's highest-severity finding was *"Don't convey information by colour
alone"*. Two signals in the console failed it:

- the environment badge distinguished MOCK from non-MOCK by background colour only;
- toned metrics (paused queue, health failures, near-expiry) coloured the number and nothing else.

Both now render a glyph and a word alongside the colour, via a single new
`StatusBadge` / `StatusIcon` pair. The icons are hand-written inline SVG — no
icon package was added, and no emoji is used as an icon.

This replaced four different ad-hoc badge treatments with one.

## 4. Contrast is now enforced, not asserted

`src/lib/design/contrast.ts` implements WCAG relative luminance and contrast
ratio. `tests/unit/contrast.test.ts` parses the tokens straight out of
`globals.css` and checks 17 text pairs at 4.5:1 and 3 boundary pairs at 3:1, in
**both** themes — 44 assertions.

It caught a real failure on the first run: `--ivr-border-strong` was `#cbd5e1`,
which is 1.48:1 against a white card. That colour is used for input, select and
button boundaries, so WCAG 1.4.11 applies. Changed to `#7c8aa0` (3.50:1 light,
4.48:1 dark).

Lowering any token below the bar now fails the build.

## 5. Consolidation

The visual inconsistency had a structural cause: duplicated CSS.

| Was | Now |
| --- | --- |
| 6 page modules each defining `.table`, `.tableScroll`, `.caption`, `.mono`, `.wrap` | one `components/data/DataTable.module.css` |
| 5 near-identical copies of `.submit`, `.control`, `.field`, `.label` | one `components/forms/Controls.module.css` |
| 4 ad-hoc status badge styles | one `StatusBadge` |

Three now-empty CSS modules were deleted. Tables gained a sticky header, compact
rows, row hover and tabular numerals; the shell header became sticky so the
environment badge stays visible while scrolling a long table.

## 6. Verification

```text
npm --prefix admin-ui run lint       exit 0  (eslint --max-warnings 0)
npm --prefix admin-ui run typecheck  exit 0  (tsc --noEmit, strict)
npm --prefix admin-ui test           13 files / 146 tests / 146 pass
npm --prefix admin-ui run build      exit 0  14 routes + Proxy
```

146 tests, up from 102: the 44 new contrast assertions. **No existing test was
changed or relaxed.** That matters for a visual refactor of this size — the
suite asserts on rendered strings and test ids (`[đã ẩn]`, `fail-closed`,
`state-NOT_WIRED`, `Sắp hết hạn`, `ĐANG BẬT`, `Đã duyệt đủ`, `execution-mode`,
`queue-status`), so all of them passing unchanged is evidence the restyle did not
alter what the console says.

Also verified against the live stack (PostgreSQL 55433, `Ivr.Api` 5005, console
3005) with the P3-2 fixture: the new `StatusBadge`, `MetricGrid` and `DataTable`
classes render on the served HTML, and the SVG status glyphs are present.

## 7. Deliverable

[`design-preview.html`](design-preview.html) — a self-contained page showing the
palette, status badges, navigation, metrics, table, controls and governance
notices. It inlines `globals.css` verbatim, so it cannot drift from the console.
Open it and switch the OS light/dark setting to see both themes.

## 8. Not claimed

- Owner and reviewer acceptance: **pending**.
- **No screenshots.** The preview browser in this environment does not composite
  frames, so no image of the running console could be captured; the static
  preview page and HTTP-level assertions stand in for one.
- Accessibility QA beyond contrast and the colour-alone fix is still `NOT_RUN`
  and owned by `P5-5` (`W-0039`). Keyboard traversal, screen-reader output and
  visual regression were not exercised.
- The component-library decision (P3-1 §5) is **still** `NEED_CONFIRMATION`. This
  pass added no UI dependency; the console remains plain CSS Modules.
- No API, contract, permission, governance flag or test expectation changed.
- The skill's data is third-party and lives outside the repository, so nothing
  here is committed from it.
