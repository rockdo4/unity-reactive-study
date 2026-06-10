#!/usr/bin/env bash
# 턴 종료(Claude Code: Stop / Cursor: stop) 시 1회 일괄 포매팅.
#
# 왜 PostToolUse/afterFileEdit 가 아니라 종료 훅인가:
#   편집 직후 포매터가 파일을 바꾸면 디스크 내용이 에이전트의 파일 상태와 어긋나,
#   같은 파일을 다시 편집할 때 "외부에서 변경됨"으로 재읽기가 강제된다(토큰 낭비).
#   턴이 끝난 뒤 한 번만 포매팅하면 세션 중 디스크가 바뀌지 않아 재읽기가 사라진다.
#
# 종료 훅은 편집된 파일 경로를 주지 않으므로 stdin 은 무시하고,
# git 워킹트리에서 변경/신규 파일을 직접 수집해 포매팅한다.
#
# 출력 규약(무한 루프 방지): stdout 으로 아무것도 내지 않고 exit 0.
#   - Claude Code Stop: decision 미출력 → 재진행 없음.
#   - Cursor stop: followup_message 미출력 → 자동 재submit 없음.
set +e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
# shellcheck source=common.sh
source "$SCRIPT_DIR/common.sh"

# 훅이 넘기는 stdin 을 비워 파이프가 막히지 않게 한다(내용은 쓰지 않음).
cat >/dev/null 2>&1

repo_root="$(get_repo_root)" || exit 0
cd "$repo_root" || exit 0

command -v git >/dev/null 2>&1 || exit 0
git rev-parse --is-inside-work-tree >/dev/null 2>&1 || exit 0

# 변경(삭제 제외) + staged + 신규(untracked) 파일을 NUL 구분으로 수집한다.
# NUL 구분이라 공백 포함 경로("Assets/TextMesh Pro/...")도 안전하다.
declare -A seen
cs_files=()
prettier_files=()
while IFS= read -r -d '' f; do
  [[ -z "$f" ]] && continue
  [[ -n "${seen[$f]}" ]] && continue
  seen[$f]=1

  # 벤더 에셋 제외
  [[ "$f" =~ (^|/)Assets/TextMesh\ Pro/ ]] && continue

  if [[ "$f" == *.cs ]]; then
    # C# 는 Assets/ 하위만 포매팅
    [[ "$f" =~ (^|/)Assets/ ]] || continue
    [[ -f "$f" ]] && cs_files+=("$f")
  elif [[ "$f" =~ \.(jsonc?|md|markdown|ya?ml|m?jsx?|cjs|mjs|tsx?|html|css|scss|less)$ ]]; then
    [[ -f "$f" ]] && prettier_files+=("$f")
  fi
done < <(
  git diff -z --name-only --diff-filter=ACMR 2>/dev/null
  git diff -z --name-only --cached --diff-filter=ACMR 2>/dev/null
  git ls-files -z --others --exclude-standard 2>/dev/null
)

# C# (csharpier) — 변경된 .cs 만 일괄 처리
if ((${#cs_files[@]} > 0)); then
  dotnet csharpier format -- "${cs_files[@]}" >/dev/null 2>&1
fi

# Prettier — 변경된 대상 파일만 일괄 처리
if ((${#prettier_files[@]} > 0)); then
  node_modules="$repo_root/node_modules"
  if [[ ! -d "$node_modules" ]]; then
    (cd "$repo_root" && npm ci --ignore-scripts >/dev/null 2>&1)
  fi
  if [[ -d "$node_modules" ]]; then
    ignore_file="$repo_root/.prettierignore"
    if [[ -f "$ignore_file" ]]; then
      npm exec -- prettier --write --ignore-path "$ignore_file" -- "${prettier_files[@]}" >/dev/null 2>&1
    else
      npm exec -- prettier --write -- "${prettier_files[@]}" >/dev/null 2>&1
    fi
  fi
fi

exit 0
