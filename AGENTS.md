# AGENTS.md

Unity 6.3 / URP 17 프로젝트. Claude Code·Cursor 등 모든 에이전트의 공통 지침.

## 편집 금지 (Unity가 관리)

- `*.meta` — Unity가 GUID와 함께 자동 생성.
- `*.unity`·`*.prefab`·`*.asset`·`*.mat` 등 YAML 자산 — 에디터로만 편집, 텍스트 직접 편집 금지.
- `Packages/packages-lock.json`, `ProjectSettings/` — Unity·Package Manager가 관리.

## 코드

- 위치 `Assets/Scripts/`. 기능 단위 폴더마다 `.asmdef`(이름=폴더 경로, 에디터 전용은 Editor asmdef로 분리).
- 스타일은 `.editorconfig`(C# 4-space, max 120). 작업을 마치면(Stop 훅) CSharpier·Prettier가 변경 파일을 일괄 자동 포맷하므로 수동 포맷 불필요(편집 도중에는 포맷되지 않는다).
- 입력은 Input System(레거시 Input Manager 금지), 렌더는 URP.

## 컴파일·테스트

- Unity 에디터가 컴파일한다. `dotnet build`/`dotnet test`로 검증하지 않는다(.csproj는 IDE용이며 Unity가 재생성).
- 코드 변경 후 Unity MCP `Unity_GetConsoleLogs`로 에러를 확인한다. 테스트는 Unity Test Framework.

## 패키지 소스

- `Library/PackageCache/`에 URP·InputSystem 등 실제 소스가 있으나 `.gitignore`라 Glob/Grep에 안 잡힌다. 참조 시: `rg --no-ignore -t cs "<패턴>" Library/PackageCache/<패키지>@*/`
