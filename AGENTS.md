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

## 강의 자료·배포

- **강사용 데모 씬**은 **통합 기초 씬(1부, 5섹션 2열)과 클리커 씬(2부)** SceneCreator가 정본이며 **커밋하지 않는다**(`Assets/Scenes/*.unity`는 `.gitignore`). `Tools > Reactive Study > Create All Scenes`로 두 씬을 생성·갱신한다. 1부 통합 씬은 `ReactiveBasicsSceneCreator`가 챕터별 `Build*Panel`을 재사용해 조립한다.
- **학생 배포**: 교안은 노션, 데모는 `Assets/StudentPackage/`(클래스명 `My*`, **R3 본문을 비운 스켈레톤** — 수업 중 강사와 한 줄씩 라이브 코딩)에서 익스포트한 **필수 패키지 2종**(① 1부 기초 통합 + 공통 폰트/TMP, ② 2부 클리커) + **선택 1종**(③ 타이밍 바 — capstone 스캐폴드, 아래 capstone 항목)으로 배포한다(학생은 빈 URP 프로젝트, 1부 패키지를 가장 먼저 임포트). 배포되는 `My*` 코드는 **주석을 전부 제거**해 배포한다(안내는 따라하기 문서·강의로). StudentPackage의 씬·스크립트는 커밋한다. 절차·운영 규칙은 `Docs/강사용/00. 학생 배포 가이드.md`.
- **동기화 규칙**: 강사용 런타임 코드(`Assets/Scripts/`)나 씬 구성을 바꾸면 → 대응 `My*` 스크립트(`Assets/StudentPackage/Scripts/`)를 같이 고치고 → `Create All Student Scenes`로 학생 씬을 재생성하고 → 해당 패키지를 다시 익스포트한다. `My*`의 직렬화 필드명은 원본과 **동일**해야 한다(`StudentPackageCreator`의 SwapComponent가 필드명으로 값을 복사하며, 어긋나면 명시적 예외로 실패).
- **capstone 과제(타이밍 바)**: 두 난이도로 낸다. **A. 자력 완성(기본)** — 빈 씬에서 UI·코드 전부 학생이(두더지 모델). **스켈레톤·학생 패키지 없이** 완성본 윈도우 빌드 + 과제 명세만 배포. **B. UI 제공 스캐폴드(선택)** — UI·필드 와이어링까지 된 학생 패키지(③ `R3Lecture-Student-3-TimingBar`: `ReactiveTimingBar` 씬 + `MyTimingBarModel`/`MyTimingBarPresenter` 스켈레톤)를 배포, 학생은 Model 규칙 + Presenter R3만 채운다. B 스켈레톤은 1·2부와 달리 **빈 메서드 골격도 안 주고**(`Start()`만 비움) 스트림 구조까지 학생이 설계한다 — `My*` 필드명은 원본 `TimingBarPresenter`와 동일해야 SwapComponent가 와이어링을 복사한다. 모범답안(강사 전용)은 `Assets/Scripts/TimingBar/`(MVP: `TimingBarModel`/`TimingBarPresenter`), 정본 씬은 `Tools > Reactive Study > 03. 타이밍 바(과제 정본) 씬 생성`(빌드·시연용, 비커밋). 학생 명세 `Docs/과제/`(A·B 각 1종), 운영 `Docs/강사용/07...`.
- **문서**: `Docs/교안/`(학생용·노션 배포, 01~05) — **순수 개념·이론·일반 코드 예제만** 담고 데모 따라하기·씬 이름·저장소 경로(`Assets/...`)·강사용 링크·타 차시 인용은 넣지 않는다(각 장은 독립적으로 읽힌다). 데모 따라하기·시연 대본은 `Docs/강사용/`(차시별 이론/따라하기 + 사전 학습·학생 배포 가이드)에 둔다. 연습문제(실습)는 강의에서 빠졌고 `Archive/실습/`에 보관한다.

## 패키지 소스

- `Library/PackageCache/`에 URP·InputSystem 등 실제 소스가 있으나 `.gitignore`라 Glob/Grep에 안 잡힌다. 참조 시: `rg --no-ignore -t cs "<패턴>" Library/PackageCache/<패키지>@*/`
