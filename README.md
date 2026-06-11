# unity-reactive-study

유니티 **리액티브 프로그래밍(R3)** 수업 자료 저장소. 리액티브 일반 이론(옵저버 패턴의 확장, 스트림, 마블 다이어그램)부터 R3 사용법(생성·연산자·구독 수명·Unity 통합·비동기)까지를 다루고, 배운 것만으로 미니 게임(리액티브 클리커)을 완성하는 따라하기 수업으로 마무리합니다.

> 기준 버전: **R3 1.3.1** · Unity 6.3 / URP 17 · 입력은 Input System

## 시작하기

1. 저장소를 클론하고 Unity 6.3으로 엽니다. R3는 이미 설치되어 있습니다(NuGet DLL `Assets/Packages/` + `R3.Unity` 패키지).
2. 데모 씬을 생성합니다.
    - `Tools > Reactive Study > 1부 기초 데모 씬 생성` — 개념 데모 5패널 (`Assets/Scenes/ReactiveBasics.unity`)
    - `Tools > Reactive Study > 2부 클리커 씬 생성` — 미니 게임 완성본 (`Assets/Scenes/ReactiveClicker.unity`)
3. [Docs/00. 수업 개요](Docs/00.%20수업%20개요.md)에서 커리큘럼을 확인합니다.

## 문서

| 문서                                       | 내용                                           |
| ------------------------------------------ | ---------------------------------------------- |
| [00. 수업 개요](Docs/00.%20수업%20개요.md) | 커리큘럼·운영 방식·준비물                      |
| `Docs/교안/` 01~05                         | 1부 — 기본 개념 및 사용법 (이론 + 따라하기)    |
| `Docs/교안/` 06                            | 2부 — 리액티브 클리커 만들기 (따라하기)        |
| `Docs/강사용/`                             | 강사 사전 학습 로드맵, 차시별 진행 가이드, Q&A |
| `Docs/실습/`                               | 1부·2부 마무리 실습 과제                       |

## 코드

| 위치                             | 내용                                                     |
| -------------------------------- | -------------------------------------------------------- |
| `Assets/Scripts/ReactiveBasics/` | 1부 데모 5종(생성/연산자/수명/바인딩/비동기) + 씬 생성기 |
| `Assets/Scripts/ClickerGame/`    | 2부 클리커 완성본 (Model/Presenter) + 씬 생성기          |

## 참고

- R3 공식 저장소: <https://github.com/Cysharp/R3>
- 로컬 개발 환경 초기화(포맷터 등): `bash .claude/skills/setup/setup.sh` (Claude Code: `/setup`)
