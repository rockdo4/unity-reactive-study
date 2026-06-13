# 리액티브 프로그래밍 (R3) 학습 프로젝트

Unity 6.3 / URP 17 기반 **R3 리액티브 프로그래밍 강의용 저장소**입니다. 리액티브 일반 이론(옵저버 패턴의 확장, 스트림, 마블 다이어그램)부터 R3 사용법(Observable 생성·연산자·구독 수명·Unity 통합·비동기)까지를 다루고, 배운 것만으로 미니 게임(리액티브 클리커)을 완성하는 따라하기 수업으로 마무리합니다.

> 기준 버전: **R3 1.3.1** (`com.cysharp.r3`) · Unity 6.3 / URP 17 · 입력은 Input System
> 공식 저장소: <https://github.com/Cysharp/R3>

이 저장소는 **강사 전용**입니다. 학생은 저장소를 클론하지 않습니다 — 교안은 노션으로, 데모는 차시별 `.unitypackage`로 배포합니다(아래 「배포 모델」 참조).

## 학습 구성

각 차시는 독립적인 데모 씬과 핵심 스크립트를 가집니다(1부 데모는 단일 씬이 아니라 차시별 독립 씬). 차시 번호는 강의 진도 순서를 따릅니다.

| 차시 | 주제                        | 데모 씬 (`Assets/Scenes/`) | 핵심 스크립트                                           |
| ---- | --------------------------- | -------------------------- | ------------------------------------------------------- |
| 01   | 리액티브 프로그래밍이란     | (없음 · 이론)              | –                                                       |
| 02   | R3 시작 · Observable 만들기 | `02_CreateStreams`         | `CreateStreamsDemo.cs`                                  |
| 03   | 연산자                      | `03_Operators`             | `OperatorsDemo.cs`                                      |
| 04   | 구독 수명 관리              | `04_Lifetime`              | `LifetimeDemo.cs` (`LifetimeProbe` 포함)                |
| 05   | Unity 통합과 비동기         | `05_BindingAsync`          | `BindingDemo.cs` · `AsyncDemo.cs` (바인딩+비동기 2패널) |
| 06   | 따라하기 · 리액티브 클리커  | `06_ReactiveClicker`       | `ClickerModel.cs` · `ClickerPresenter.cs`               |

- 1부(02~05) 데모는 **완성본**입니다 — 실행하며 R3 동작을 관찰합니다.
- 2부(06)는 학생이 교안 단계를 따라 **직접 코드를 작성**하는 따라하기 수업입니다. 저장소의 완성본 씬은 막혔을 때 비교·확인용입니다.
- 데모 코드 위치: `Assets/Scripts/ReactiveBasics/`(1부 데모 5종 + 차시별 씬 생성기), `Assets/Scripts/ClickerGame/`(2부 클리커 완성본 Model/Presenter + 씬 생성기 + EditMode 테스트).

## 강의 문서

강의의 모든 텍스트 자료가 `Docs/`에 정리돼 있습니다. 문서는 **역할별 폴더**로 나뉩니다.

| 폴더/파일               | 내용                                                                                                                               |
| ----------------------- | ---------------------------------------------------------------------------------------------------------------------------------- |
| `Docs/교안/`            | 차시별 수업 자료(이론) — 수업 중 학생과 함께 보고 **노션 공유 페이지**로 배포. 01~05(1부) · 06(2부 따라하기).                      |
| `Docs/강사용/`          | 강사 전용 — `00. 강사 사전 학습 가이드`(멘탈 모델·가드레일·FAQ·Unity 관용구→R3), 차시별 진행 가이드(이론/따라하기 분리), 예상 Q&A. |
| `Docs/데모/`            | 데모 운영 — `씬 생성·패키지 도구 가이드`(씬 생성 절차 · 문제 해결).                                                                |
| `Docs/00. 수업 개요.md` | 전체 커리큘럼 · 운영 방식 · 준비물.                                                                                                |

- **파일명 규칙** — 강사용은 `NN. 제목 이론 - 강사용.md` / `NN. 제목 따라하기 - 강사용.md`로 이론·따라하기를 분리합니다. 한 종류뿐인 차시(01 이론 전용, 06 따라하기 전용)는 `NN. 제목 - 강사용.md` 한 파일을 씁니다.
- **강사가 R3를 처음 접한다면** — [Docs/강사용/00. 강사 사전 학습 가이드](Docs/강사용/00.%20강사%20사전%20학습%20가이드.md)부터 읽으세요.

## 도구로 씬·패키지 생성

씬 파일(`*.unity`)은 git에 커밋하지 않습니다. **에디터 스크립트가 씬을 코드로 생성**하므로 생성기 코드가 정본입니다.

- **강사용 데모 씬 생성** — 차시별 메뉴(`Tools > Reactive Study > 02. Observable 만들기 씬 생성` … `06. 클리커 씬 생성`)로 개별 생성하거나, `Tools > Reactive Study > Create All Scenes`(`Assets/Editor/AllScenesCreator.cs`)로 전 차시 씬을 한 번에 생성·갱신합니다. 생성기 코드는 `Assets/Scripts/*/Editor/*SceneCreator.cs`.
- **학생 배포 패키지 생성** — `Tools > Reactive Study > Student Package > Create All Student Scenes`로 학생용 씬을 생성하고, `Export All Student Packages`로 차시별 `.unitypackage`를 만듭니다(차시별 `Export NN. …` 메뉴로 개별 익스포트도 가능). 생성기는 `Assets/Editor/StudentPackageCreator.cs`, 학생 스크립트는 `Assets/StudentPackage/Scripts/`.
- **학생 스크립트** — 클래스에 `My` 접두사를 붙입니다(`MyCreateStreamsDemo` · `MyOperatorsDemo` · `MyLifetimeDemo`(+`MyLifetimeProbe`) · `MyBindingDemo` · `MyAsyncDemo` · `MyClickerModel` · `MyClickerPresenter`). 1부는 완성본(실행·관찰), 2부는 핵심 R3 메서드 본문을 비운 **스켈레톤**으로 라이브 코딩용입니다.

## 배포 모델

학생에게는 두 가지로 배포합니다 — **교안은 노션 공유 페이지**, **데모는 차시별 `.unitypackage`**.

학생 측 준비 절차:

1. **빈 URP 프로젝트**(Unity 6.3 / URP 17)를 만든다.
2. R3를 설치한다 — R3 코어는 **NuGetForUnity**로, 유니티 통합(`R3.Unity`)은 **git URL**(`com.cysharp.r3`)로. 설치 절차는 교안 02에서 단계별로 다룬다.
3. Input System을 설치한다(레거시 Input Manager 미사용).
4. **`00-Common` 패키지(`R3Lecture-Student-00-Common`)를 가장 먼저** 임포트한다 — Pretendard 한글 폰트 + TMP 설정이 들어 있어 한글 깨짐을 막는다.
5. 차시별 패키지(`R3Lecture-Student-02-CreateStreams` … `R3Lecture-Student-06-Clicker`)를 **진도에 맞춰** 임포트한다.

| 차시 | 학생 패키지(`.unitypackage`)         | 학생 코드              |
| ---- | ------------------------------------ | ---------------------- |
| 공통 | `R3Lecture-Student-00-Common`        | (한글 폰트 · TMP 설정) |
| 02   | `R3Lecture-Student-02-CreateStreams` | 완성본                 |
| 03   | `R3Lecture-Student-03-Operators`     | 완성본                 |
| 04   | `R3Lecture-Student-04-Lifetime`      | 완성본                 |
| 05   | `R3Lecture-Student-05-BindingAsync`  | 완성본                 |
| 06   | `R3Lecture-Student-06-Clicker`       | 스켈레톤(라이브 코딩)  |

## 시작하기 (강사)

1. 저장소를 클론하고 Unity 6.3으로 엽니다. R3는 **이미 설치**되어 있습니다(NuGet DLL `Assets/Packages/` + Package Manager `R3.Unity`).
2. 최초 1회 로컬 개발 환경을 초기화합니다(CSharpier·Prettier 포맷 훅 등):

    ```bash
    bash .claude/skills/setup/setup.sh   # Claude Code: /setup 스킬로도 실행 가능
    ```

3. `Tools > Reactive Study > Create All Scenes`로 데모 씬을 생성한 뒤, `Assets/Scenes/`에서 학습할 씬을 열고 Play를 누릅니다.
4. 커리큘럼은 [Docs/00. 수업 개요](Docs/00.%20수업%20개요.md)에서 확인합니다.

## 의존성

- R3 (`com.cysharp.r3`) — 코어 NuGet `R3 1.3.1` + 유니티 통합 `R3.Unity`(git URL `#1.3.1`)
- Input System (`com.unity.inputsystem` 1.19) · TextMeshPro(uGUI 2.0) · Unity Test Framework · URP 17

## 참고

- R3 공식 저장소: <https://github.com/Cysharp/R3>
- R3 발표 글(저자 neuecc): <https://neuecc.medium.com/r3-a-new-modern-reimplementation-of-reactive-extensions-for-c-cf29abcc5826>
- 로컬 개발 환경 초기화(포맷터 등): `bash .claude/skills/setup/setup.sh` (Claude Code: `/setup`)
