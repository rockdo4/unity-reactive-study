# 타이밍 바 — UI 제공 스켈레톤 (R3 채우기)

> 이 버전은 [빈 씬에서 만드는 R3 미니게임](타이밍%20바%20—%20빈%20씬에서%20만드는%20R3%20미니게임.md)의 **스캐폴드(쉬운) 버전**입니다. 화면(UI)과 Inspector 필드 연결은 **이미 되어 있습니다**. 여러분은 **게임 규칙(Model)과 R3 스트림(Presenter)만** 채우면 됩니다. UI를 직접 만드는 대신 **R3 코드에 집중**하고 싶을 때 이쪽을 선택하세요.
>
> 만드는 게임·동작·완료 기준은 빈 씬 버전과 **똑같습니다**. 차이는 "화면을 직접 만드느냐"뿐입니다. 게임 규칙·R3 단계별 설명·연산자 힌트 표는 빈 씬 버전 문서를 함께 보세요.

## 받는 것 (스캐폴드)

배포받은 `R3Lecture-Student-3-TimingBar` 패키지를 임포트하면:

- **씬** `ReactiveTimingBar` — 트랙·Good/Perfect 존·마커·HIT/START 버튼·점수/콤보/진행도/판정/상태 텍스트가 배치되어 있습니다.
- **와이어링** — `MyTimingBarPresenter`의 `[SerializeField]` 필드가 위 UI에 **모두 연결**되어 있습니다(Inspector에서 끌어다 넣는 작업은 끝).
- **스켈레톤 스크립트 2개** — `MyTimingBarModel`(규칙)·`MyTimingBarPresenter`(입력·프레임·표시). 둘 다 **본문이 비어 있습니다**.

> ⚠️ **1부 기초 패키지(`R3Lecture-Student-1-Basics`)를 먼저 임포트**하세요(한글 폰트가 거기 있습니다). R3·UniTask·Input System 설치도 선행되어야 합니다.

임포트 직후 Play하면 화면은 뜨지만 버튼이 무반응인 게 정상입니다 — 아래를 채우면 동작합니다.

## 채울 것 ①: `MyTimingBarModel` (게임 규칙 — 순수 C#)

점수·콤보·판정 규칙입니다. `ReactiveProperty<int> Score/Combo/MaxCombo`, 존 폭 상수(`PerfectHalfWidth`·`GoodHalfWidth`), 점수 상수, 생성자(`AddTo`)·`Dispose`는 **제공**됩니다. 아래 메서드 **본문**을 채우세요.

| 메서드                  | 채울 내용                                                                                                                |
| ----------------------- | ------------------------------------------------------------------------------------------------------------------------ |
| `ZoneOf(float phase)`   | 위상(0~1)이 중앙(0.5)에서 얼마나 떨어졌는지로 `None`/`Good`/`Perfect` 판정. `PerfectHalfWidth`·`GoodHalfWidth` 상수 사용 |
| `Judge(float phase)`    | `ZoneOf` 결과를 `Judgement`(Miss/Good/Perfect)로 변환                                                                    |
| `ApplyHit(float phase)` | 한 번의 치기 반영. Miss면 콤보 0, 아니면 콤보 +1·최고 콤보 갱신·(기본점 × 콤보 배수)만큼 점수 가산. 판정 결과 반환       |
| `ResetForNewGame()`     | Score·Combo·MaxCombo를 0으로                                                                                             |

> 💡 존 폭 상수가 화면의 존 Image와 같은 값이라, `ZoneOf`를 상수로 계산하면 **시각 존과 판정이 정확히 일치**합니다(직접 픽셀로 맞추지 마세요).

## 채울 것 ②: `MyTimingBarPresenter` (입력·프레임·표시 — MonoBehaviour)

`[SerializeField]` 필드(트랙·마커·버튼·텍스트·`m_MarkerSpeed`·`m_AttemptsPerRound`)는 **연결 완료**, `Start()`는 **비어 있습니다**. 여기에 R3 스트림으로 게임을 구동하세요. 빈 칸 메서드를 미리 만들어 두지 않았으니 **구조(필드·메서드 분리)도 직접 설계**합니다.

`Start()`에서 모델 생성·`m_UsableWidth` 계산 후, 빈 씬 버전 문서의 **2~6단계**를 그대로 구현하면 됩니다.

| 단계              | 핵심 R3                                                                                                                                          |
| ----------------- | ------------------------------------------------------------------------------------------------------------------------------------------------ |
| 2. 마커 왕복      | `Observable.EveryUpdate` + `ReactiveProperty<float>`(위상) + `Mathf.PingPong` · `AddTo`                                                          |
| 3. 존 진입 강조   | 위상 `.Select(ZoneOf).DistinctUntilChanged().Subscribe(존→트랙색)`                                                                               |
| 4. 누른 순간 판정 | `HIT.OnClickAsObservable()` + Space(`Keyboard...wasPressedThisFrame`) `Merge` → `WithLatestFrom(위상)` → `ApplyHit`                              |
| 5. 라운드 흐름    | `START.OnClickAsObservable().SubscribeAwait(..., Drop)` + `UniTask.Delay`(카운트다운) + `Take(N)` + `WaitAsync` + 라운드용 `CompositeDisposable` |
| 표시              | `Model.Score/Combo` · 진행도 `.Subscribe(값→텍스트)`                                                                                             |
| 6. 수명           | 영속 구독 `AddTo(this)` · 라운드 구독은 라운드 `CompositeDisposable` · `OnDestroy`에서 모델 `Dispose`                                            |

> 핵심 규칙은 빈 씬 버전과 같습니다: **`Update()`·코루틴·상태 플래그 금지.** 모든 흐름을 스트림으로.

연산자 ↔ 단계 매핑, 단계별 ✅ 확인 포인트, 흔한 함정은 [빈 씬 버전 문서](타이밍%20바%20—%20빈%20씬에서%20만드는%20R3%20미니게임.md)에 자세히 있습니다.

## 완료 기준

- [ ] START → 카운트다운 → 마커 왕복 → 존 판정/콤보/점수 → N회 후 결과 → 다시 시작이 동작한다.
- [ ] `Update()` · 코루틴 · 상태 플래그를 쓰지 않았다(모든 흐름이 스트림).
- [ ] 게임 규칙(판정·점수·콤보)이 `MyTimingBarModel`에, 입력·표시가 `MyTimingBarPresenter`에 있다.
- [ ] `Window > Observable Tracker`에서 씬을 나가면 남은 구독이 0이다.
- [ ] Console에 에러·경고가 없다.

## 제출물

- 작성한 `MyTimingBarModel.cs` · `MyTimingBarPresenter.cs`.
- 플레이 영상(한 판 — 카운트다운부터 결과까지).
- Observable Tracker 스크린샷(씬을 나간 뒤 남은 구독 0).

## 더 해보기

- UI까지 스스로 만들고 싶다면 [빈 씬 버전](타이밍%20바%20—%20빈%20씬에서%20만드는%20R3%20미니게임.md)으로 도전하세요(같은 게임, 화면도 직접 제작).
- 도전 과제(제한시간 모드·난도 상승·가상 시간 테스트)도 빈 씬 버전 문서에 있습니다.
