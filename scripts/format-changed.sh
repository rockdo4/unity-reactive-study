#!/usr/bin/env bash
# 커밋 전에 직접 실행하는 변경 파일 포매터: `npm run format:changed`
#
# 왜 자동 훅이 아닌가:
#   에이전트 턴 중·턴 종료 시 포매터가 파일을 바꾸면 디스크가 에이전트의 파일 상태와 어긋나
#   다음 편집 때 재읽기가 강제된다(토큰 낭비). 그래서 커밋 직전에 사람이/에이전트가 한 번 돌린다.
#
# 왜 전체 포맷(`npm run format`)이 아닌가:
#   전체 포맷은 보존 대상(벤더 에셋 등)까지 재포맷한다. 변경·신규 파일만 대상으로 한다.
set -u

repo_root="$(git rev-parse --show-toplevel 2>/dev/null)" || {
  echo "git 저장소가 아닙니다." >&2
  exit 1
}
cd "$repo_root" || exit 1

# 변경(삭제 제외) + staged + 신규(untracked) 파일을 NUL 구분으로 수집(공백 포함 경로 안전).
declare -A seen
cs_files=()
prettier_files=()
while IFS= read -r -d '' f; do
  [[ -z "$f" || -n "${seen[$f]:-}" ]] && continue
  seen[$f]=1
  [[ -f "$f" ]] || continue

  # 벤더 에셋 제외
  [[ "$f" =~ (^|/)Assets/TextMesh\ Pro/ ]] && continue

  if [[ "$f" == *.cs ]]; then
    # C#은 Assets/ 하위만
    [[ "$f" =~ (^|/)Assets/ ]] && cs_files+=("$f")
  elif [[ "$f" =~ \.(jsonc?|md|markdown|ya?ml|m?jsx?|cjs|mjs|tsx?|html|css|scss|less)$ ]]; then
    prettier_files+=("$f")
  fi
done < <(
  git diff -z --name-only --diff-filter=ACMR
  git diff -z --name-only --cached --diff-filter=ACMR
  git ls-files -z --others --exclude-standard
)

status=0

if ((${#cs_files[@]} > 0)); then
  echo "==> CSharpier: ${#cs_files[@]}개"
  dotnet csharpier format -- "${cs_files[@]}" || status=1
fi

if ((${#prettier_files[@]} > 0)); then
  echo "==> Prettier: ${#prettier_files[@]}개"
  [[ -d node_modules ]] || npm ci --ignore-scripts || status=1
  npm exec -- prettier --write --ignore-path .prettierignore -- "${prettier_files[@]}" || status=1
fi

((${#cs_files[@]} + ${#prettier_files[@]} == 0)) && echo "포맷할 변경 파일이 없습니다."
exit "$status"
