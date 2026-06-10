# Shared helpers for Claude Code / Cursor format hooks (bash).

_hook_dir() {
  cd "$(dirname "${BASH_SOURCE[1]}")" && pwd
}

_is_repo_root() {
  local dir="$1"
  [[ -f "$dir/package.json" ]] ||
    [[ -f "$dir/dotnet-tools.json" ]] ||
    [[ -f "$dir/.config/dotnet-tools.json" ]]
}

get_repo_root() {
  local dir
  dir="$(_hook_dir)"
  while [[ -n "$dir" ]]; do
    if _is_repo_root "$dir"; then
      printf '%s\n' "$dir"
      return 0
    fi
    local parent
    parent="$(dirname "$dir")"
    [[ "$parent" == "$dir" ]] && break
    dir="$parent"
  done
  echo "Could not find repo root from $(_hook_dir)" >&2
  return 1
}
