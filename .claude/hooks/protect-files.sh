#!/usr/bin/env bash
# PreToolUse hook: blocks Claude from editing files that should never be hand-edited.
# Exit code 2 = block the tool call; stderr is shown to Claude as the reason.
set -euo pipefail

input=$(cat)
file_path=$(printf '%s' "$input" | jq -r '.tool_input.file_path // empty')
[[ -z "$file_path" ]] && exit 0

case "$file_path" in
  */Migrations/*.Designer.cs|*/Migrations/*ModelSnapshot.cs)
    echo "Blocked: EF Core generated files must be produced by 'dotnet ef', not edited by hand. Use the add-migration skill." >&2
    exit 2
    ;;
  */.env|*/.env.*)
    echo "Blocked: .env files hold secrets and are off-limits." >&2
    exit 2
    ;;
  */.git/*)
    echo "Blocked: do not edit files inside .git/." >&2
    exit 2
    ;;
esac

exit 0
