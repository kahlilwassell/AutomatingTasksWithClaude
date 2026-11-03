#!/usr/bin/env bash
# PostToolUse hook: after Claude edits a .cs file, fix whitespace/formatting in that
# file only. Fast because --folder skips loading the whole solution. Never blocks.
set -uo pipefail

input=$(cat)
file_path=$(printf '%s' "$input" | jq -r '.tool_input.file_path // empty')

if [[ "$file_path" == *.cs && -f "$file_path" ]] && command -v dotnet >/dev/null 2>&1; then
  cd "${CLAUDE_PROJECT_DIR:-.}" || exit 0
  dotnet format whitespace . --folder --include "$file_path" >/dev/null 2>&1 || true
fi

exit 0
