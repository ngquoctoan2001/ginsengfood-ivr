# Branch policy — `main` only, no exceptions

> Luật bắt buộc: repo này chỉ làm việc trên `main`. Không agent nào được tạo
> nhánh mới — kể cả ChatGPT, Codex hay Claude Code.

**Never create a git branch in this repository.** Every change is committed
straight to `main`. This binds every agent without exception — Claude Code,
Codex, ChatGPT, Cursor, an IDE button, a shell script — and every spelling of
the command:

- `git checkout -b` / `-B` / `--orphan`
- `git switch -c` / `-C` / `--create` / `--orphan`
- `git branch <name>`
- `git worktree add` — invents a branch named after the path unless `--detach`
- `git push <remote> HEAD:refs/heads/<anything but main>`
- `git update-ref refs/heads/<name>`

If you believe a branch is genuinely required, **stop and ask the repo owner**.
Do not decide that for yourself, and do not reach for the escape hatch below on
your own initiative — it exists for the owner, not for you.

Both remotes track `main` only: `origin` (GitLab) and `github` (GitHub). A
`git push origin main` reaches both, because `remote.origin.pushurl` holds two
values. Where a stray branch already exists, merge it into `main` and delete it.

> **`github/main` always looks stale, and it almost never is.** That push travels
> through `origin`'s second pushurl, so it never updates the `github` remote.
> `git rev-parse github/main` therefore reports whatever the last `git fetch
> github` saw, which can be weeks old. On `2026-09-08` this made a review report
> the mirror 43 commits behind while both remotes were byte-identical at
> `03a1922`. Ask the remote, not the tracking ref:
>
> ```sh
> git ls-remote --heads github    # and: git ls-remote --heads origin
> ```

**One named exception, and only this one.** `codex/w0128-w0129-candidate` is **not** stray and must
**not** be deleted. `W-0130` created it deliberately as a provenance anchor, because `main@2a4f45d`
was a mixed `save` of 98 files and did not carry enough provenance for the `W-0128`/`W-0129`
evidence to bind to. [`docs/evidence/W-0130/README.md`](docs/evidence/W-0130/README.md) names the
branch, its worktree and its tree hash; deleting the ref would make commit `1fa0150` unreachable and
break that chain. Being one commit ahead of `main` is its design, not unfinished work.

Owner confirmed this on `2026-09-07` (`W-0222`) while deleting the two branches that genuinely were
stray. Written down here because every audit so far has re-flagged it — the 07/09 review asserted
"repo only has main" in sixteen separate rows, which was wrong in the other direction.

This exception permits nothing: no new branch, no hook change, no relaxed deny rule. It marks one
existing ref as load-bearing.

## This is enforced, not advisory

`core.hooksPath` points at [`.githooks/`](.githooks), so git itself refuses the
operation — there is no shell phrasing that gets around it:

| Hook | Refuses |
|------|---------|
| `.githooks/reference-transaction` | creating any local ref under `refs/heads/` other than `main` |
| `.githooks/pre-push` | publishing any branch other than `main` to a remote |

Deleting branches, committing on `main`, fetching, tags and stashes are all
untouched. `core.hooksPath` lives in `.git/config`, which every linked worktree
shares, so the rule covers `git worktree` checkouts too.

Claude Code additionally denies the branch-creating commands up front, via
`.claude/settings.json` and `.claude/hooks/no-new-branch.sh`, so the block
arrives as a readable message instead of a hook failure.

**Do not disable, weaken, or reroute any of this.** Changing `core.hooksPath`,
editing `.githooks/`, or removing the deny rules is out of scope for every task
unless the repo owner asks for it in so many words.

## For the repo owner only

One command, one deliberate exception:

```bash
IVR_ALLOW_NEW_BRANCH=1 git switch -c <name>
```

After cloning, or if the repo folder is moved, reinstall the hooks:

```bash
pnpm hooks:install
```

`pnpm install` runs that automatically via the `prepare` script. To confirm the
rule is live: `git config --get core.hooksPath` must print an existing
`.githooks` path.

<!-- gitnexus:start -->
# GitNexus — Code Intelligence

This project is indexed by GitNexus as **ginsengfood-ivr** (54112 symbols, 77039 relationships, 300 execution flows). Use the GitNexus MCP tools to understand code, assess impact, and navigate safely.

> If any GitNexus tool warns the index is stale, run `npx gitnexus analyze` in terminal first.

## Always Do

- **MUST run impact analysis before editing any symbol.** Before modifying a function, class, or method, run `gitnexus_impact({target: "symbolName", direction: "upstream"})` and report the blast radius (direct callers, affected processes, risk level) to the user.
- **MUST run `gitnexus_detect_changes()` before committing** to verify your changes only affect expected symbols and execution flows.
- **MUST warn the user** if impact analysis returns HIGH or CRITICAL risk before proceeding with edits.
- When exploring unfamiliar code, use `gitnexus_query({query: "concept"})` to find execution flows instead of grepping. It returns process-grouped results ranked by relevance.
- When you need full context on a specific symbol — callers, callees, which execution flows it participates in — use `gitnexus_context({name: "symbolName"})`.

## Never Do

- NEVER edit a function, class, or method without first running `gitnexus_impact` on it.
- NEVER ignore HIGH or CRITICAL risk warnings from impact analysis.
- NEVER rename symbols with find-and-replace — use `gitnexus_rename` which understands the call graph.
- NEVER commit changes without running `gitnexus_detect_changes()` to check affected scope.

## Resources

| Resource | Use for |
|----------|---------|
| `gitnexus://repo/ginsengfood-ivr/context` | Codebase overview, check index freshness |
| `gitnexus://repo/ginsengfood-ivr/clusters` | All functional areas |
| `gitnexus://repo/ginsengfood-ivr/processes` | All execution flows |
| `gitnexus://repo/ginsengfood-ivr/process/{name}` | Step-by-step execution trace |

## CLI

| Task | Read this skill file |
|------|---------------------|
| Understand architecture / "How does X work?" | `.claude/skills/gitnexus/gitnexus-exploring/SKILL.md` |
| Blast radius / "What breaks if I change X?" | `.claude/skills/gitnexus/gitnexus-impact-analysis/SKILL.md` |
| Trace bugs / "Why is X failing?" | `.claude/skills/gitnexus/gitnexus-debugging/SKILL.md` |
| Rename / extract / split / refactor | `.claude/skills/gitnexus/gitnexus-refactoring/SKILL.md` |
| Tools, resources, schema reference | `.claude/skills/gitnexus/gitnexus-guide/SKILL.md` |
| Index, status, clean, wiki CLI commands | `.claude/skills/gitnexus/gitnexus-cli/SKILL.md` |

<!-- gitnexus:end -->
