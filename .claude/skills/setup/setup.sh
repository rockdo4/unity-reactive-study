#!/usr/bin/env bash
# 클론 직후 1회 실행하는 로컬 개발 환경 초기화.
# 안전 범위(프로젝트 로컬)만 자동 실행한다. 글로벌 git 설정을 바꾸는
# UnityYAMLMerge 머지 드라이버 등록은 명령을 출력만 하므로 직접 실행해야 한다.
set -u

cd "$(dirname "${BASH_SOURCE[0]}")/../../.." || exit 1

echo "==> CSharpier (dotnet 로컬 도구) 복원"
dotnet tool restore || echo "  ! 실패 — .NET SDK 설치를 확인하세요."

echo "==> Prettier 등 npm 의존성 설치"
npm ci || echo "  ! 실패 — Node.js 설치를 확인하세요."

ver="$(sed -n 's/^m_EditorVersion: //p' ProjectSettings/ProjectVersion.txt 2>/dev/null || true)"
ver="${ver:-<Unity버전>}"
cat <<EOF

==> [수동] UnityYAMLMerge 머지 드라이버 (글로벌 git 설정, 최초 1회)
    씬/프리팹 3-way 자동 머지에 필요합니다. 아래 명령을 직접 실행하세요
    (경로는 OS·설치 위치에 맞게 조정):

    # Windows (Unity Hub 기본 설치)
    git config --global merge.unityyamlmerge.name "Unity SmartMerge (UnityYAMLMerge)"
    git config --global merge.unityyamlmerge.driver '"C:/Program Files/Unity/Hub/Editor/${ver}/Editor/Data/Tools/UnityYAMLMerge.exe" merge -p --force --fallback none %O %B %A %A'
    git config --global merge.unityyamlmerge.recursive binary

    # macOS 경로 예시:
    #   "/Applications/Unity/Hub/Editor/${ver}/Unity.app/Contents/Tools/UnityYAMLMerge"

==> 완료.
EOF
