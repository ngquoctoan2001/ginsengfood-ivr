#!/usr/bin/env node
// Points this clone's git at .githooks/, which is what actually enforces the
// "everything lands on main, nobody creates branches" rule.
//
// Run once per clone (and again if the repo folder is moved):
//   pnpm hooks:install
//
// core.hooksPath is stored in .git/config, which every linked worktree shares,
// so one run covers the main checkout and every `git worktree` beside it. The
// path is written absolute on purpose: a relative ".githooks" would resolve
// against each worktree's own root, and a worktree sitting on a branch from
// before these files existed would silently run no hooks at all.

import { execFileSync } from 'node:child_process';
import { existsSync } from 'node:fs';
import path from 'node:path';

const git = (...args) =>
  execFileSync('git', args, { encoding: 'utf8', stdio: ['ignore', 'pipe', 'pipe'] }).trim();

const HOOKS = ['reference-transaction', 'pre-push'];

// Runs from `prepare`, so a tarball install or a CI checkout with no .git must
// be a no-op rather than a failed install.
let commonDir;
try {
  // --git-common-dir resolves to the main checkout's .git even from a worktree.
  commonDir = git('rev-parse', '--path-format=absolute', '--git-common-dir');
} catch {
  console.log('Not a git repository - skipping hook install.');
  process.exit(0);
}
const repoRoot = path.dirname(commonDir);
const hooksDir = path.join(repoRoot, '.githooks');

const missing = HOOKS.filter((h) => !existsSync(path.join(hooksDir, h)));
if (missing.length > 0) {
  console.error(`Cannot install: ${hooksDir} is missing ${missing.join(', ')}.`);
  console.error('Check out main in the primary worktree first, then re-run.');
  process.exit(1);
}

const configured = path.resolve(hooksDir).split(path.sep).join('/');
git('config', 'core.hooksPath', configured);

let readBack = '';
try {
  readBack = git('config', '--get', 'core.hooksPath');
} catch {
  /* unset */
}
if (readBack !== configured) {
  console.error(`Failed to set core.hooksPath (got ${readBack || '<unset>'}).`);
  process.exit(1);
}

console.log(`core.hooksPath -> ${configured}`);
console.log(`Installed: ${HOOKS.join(', ')}`);
console.log('New branches are now refused by git itself, in every worktree of this clone.');
