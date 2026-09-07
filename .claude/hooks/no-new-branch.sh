#!/bin/sh
# ginsengfood-ivr: PreToolUse guard that stops Claude Code before it runs a
# branch-creating git command, so the agent gets a readable explanation instead
# of a raw hook failure.
#
# This is the friendly layer only. The enforcement that binds every tool —
# Codex, ChatGPT, an IDE button, a plain terminal — is .githooks/, installed via
# `git config core.hooksPath .githooks`. Keep both; neither replaces the other.
#
# Reads the PreToolUse payload on stdin and matches the raw JSON, which is
# enough for these patterns and avoids a jq or node dependency on the hot path.

payload=''
while IFS= read -r line || [ -n "$line" ]; do
	payload="$payload $line"
done

# A human overriding on purpose passes the escape hatch inline.
case "$payload" in
*IVR_ALLOW_NEW_BRANCH=1*) exit 0 ;;
esac

hit=''
case "$payload" in
*"checkout -b"* | *"checkout -B"*) hit='git checkout -b' ;;
*"switch -c"* | *"switch -C"* | *"switch --create"*) hit='git switch -c' ;;
*"git branch "[!-]*) hit='git branch <name>' ;;
*--orphan*) hit='git checkout/switch --orphan' ;;
esac

# `git worktree add` invents a branch named after the path unless --detach.
if [ -z "$hit" ]; then
	case "$payload" in
	*"worktree add"*)
		case "$payload" in
		*--detach*) ;;
		*) hit='git worktree add' ;;
		esac
		;;
	esac
fi

[ -n "$hit" ] || exit 0

printf '%s' '{"hookSpecificOutput":{"hookEventName":"PreToolUse","permissionDecision":"deny","permissionDecisionReason":"'
printf '%s' "Blocked: $hit creates a branch, and this repository allows work on main only. "
printf '%s' 'Commit directly on main instead. This rule binds every agent (Claude Code, Codex, ChatGPT) and is also enforced by git itself in .githooks/, so there is no way around it in the shell. '
printf '%s' 'If the repo owner has decided a branch is genuinely needed, they can run the command themselves prefixed with IVR_ALLOW_NEW_BRANCH=1.'
printf '%s' '"}}'
exit 0
