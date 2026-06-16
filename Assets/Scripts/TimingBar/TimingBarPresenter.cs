using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ReactiveStudy.TimingBar
{
    /// <summary>
    /// 타이밍 바의 입력·프레임 루프·UI 바인딩을 담당한다(MVP의 Presenter, MonoBehaviour).
    /// Update()·코루틴·상태 플래그 없이 R3 스트림만으로 게임을 구동한다.
    /// 게임 규칙은 <see cref="TimingBarModel"/> 가, 프레임/입력/표시는 이 클래스가 맡는다.
    /// </summary>
    public class TimingBarPresenter : MonoBehaviour
    {
        [Header("트랙 / 마커")]
        [SerializeField]
        private Image m_TrackImage;

        [SerializeField]
        private RectTransform m_Marker;

        [Header("입력")]
        [SerializeField]
        private Button m_HitButton;

        [SerializeField]
        private Button m_StartButton;

        [Header("표시")]
        [SerializeField]
        private TextMeshProUGUI m_ScoreText;

        [SerializeField]
        private TextMeshProUGUI m_ComboText;

        [SerializeField]
        private TextMeshProUGUI m_AttemptText;

        [SerializeField]
        private TextMeshProUGUI m_JudgementText;

        [SerializeField]
        private TextMeshProUGUI m_StateText;

        [Header("설정")]
        [SerializeField]
        private float m_MarkerSpeed = 0.6f; // 왕복 속도(초당 위상)

        [SerializeField]
        private int m_AttemptsPerRound = 12;

        private static readonly Color NoneColor = new(0.2f, 0.2f, 0.25f, 1f);
        private static readonly Color GoodColor = new(0.2f, 0.5f, 0.9f, 1f);
        private static readonly Color PerfectColor = new(0.95f, 0.75f, 0.15f, 1f);
        private static readonly Color MissColor = new(0.9f, 0.3f, 0.3f, 1f);

        private TimingBarModel m_Model;
        private ReactiveProperty<float> m_Phase; // 마커의 현재 위상(0~1) — 입력이 샘플링할 Hot 값
        private ReactiveProperty<int> m_Attempt; // 이번 라운드의 시도 횟수
        private float m_UsableWidth;

        private void Start()
        {
            m_Model = new TimingBarModel();
            m_Phase = new ReactiveProperty<float>(0f).AddTo(this);
            m_Attempt = new ReactiveProperty<int>(0).AddTo(this);
            m_UsableWidth = m_TrackImage.rectTransform.rect.width - m_Marker.rect.width;

            BindMarker();
            BindView();
            BindGameLoop();
        }

        // 존에 "들어오는 순간"만 트랙 색을 바꾼다(Select + DistinctUntilChanged 상태머신).
        // 마커 이동 자체는 한 판 동안만(PlayOneGameAsync) 돈다 — 대기·결과 화면에선 멈춰 있다.
        private void BindMarker()
        {
            m_Phase
                .Select(TimingBarModel.ZoneOf)
                .DistinctUntilChanged()
                .Subscribe(zone => m_TrackImage.color = ColorFor(zone))
                .AddTo(this);

            ParkMarker(); // 시작 화면: 마커는 왼쪽 끝(존 밖)에 정지
        }

        // 마커를 왼쪽 끝(위상 0, 존 밖)에 세워 둔다 — 대기·결과 화면의 정지 상태.
        private void ParkMarker()
        {
            m_Phase.Value = 0f;
            m_Marker.anchoredPosition = new Vector2((0f - 0.5f) * m_UsableWidth, 0f);
        }

        // Model 의 상태(ReactiveProperty) → UI. "값이 바뀌면 이 텍스트와 같게"를 한 번 선언한다.
        private void BindView()
        {
            m_Model.Score.Subscribe(score => m_ScoreText.text = $"점수 {score:N0}").AddTo(this);
            m_Model
                .Combo.Subscribe(combo => m_ComboText.text = combo >= 2 ? $"{combo} 콤보!" : string.Empty)
                .AddTo(this);
            m_Attempt.Subscribe(n => m_AttemptText.text = $"{n} / {m_AttemptsPerRound}").AddTo(this);
        }

        // START → 카운트다운 → 라운드 → 결과. 진행 중 START 재클릭은 Drop 으로 무시한다.
        private void BindGameLoop()
        {
            m_StartButton
                .OnClickAsObservable()
                .SubscribeAwait((_, ct) => PlayOneGameAsync(ct), AwaitOperation.Drop)
                .AddTo(this);
        }

        private async ValueTask PlayOneGameAsync(CancellationToken ct)
        {
            m_Model.ResetForNewGame();
            m_Attempt.Value = 0;
            m_StartButton.interactable = false;
            m_JudgementText.text = string.Empty;

            // 한 판(카운트다운+라운드) 동안만 살아 있는 구독을 play 그릇에 담아, 판이 끝나면 정리한다.
            using var play = new CompositeDisposable();

            // 마커 이동은 이 판 동안만 — 위상은 판 시작 기준 경과 시간으로 계산해 매번 왼쪽(0)부터 출발.
            float startTime = Time.time;
            Observable
                .EveryUpdate()
                .Subscribe(_ =>
                {
                    float phase = Mathf.PingPong((Time.time - startTime) * m_MarkerSpeed, 1f);
                    m_Phase.Value = phase; // 입력 판정이 WithLatestFrom 으로 샘플링한다
                    m_Marker.anchoredPosition = new Vector2((phase - 0.5f) * m_UsableWidth, 0f);
                })
                .AddTo(play);

            await CountdownAsync(ct); // 카운트다운 동안 마커가 움직여 리듬 워밍업

            m_StateText.text = "존에 올 때 Space 또는 HIT!";

            Observable<float> roundHits = HitStream()
                .WithLatestFrom(m_Phase, (_, phase) => phase) // 누른 '순간'의 마커 위치를 샘플
                .Take(m_AttemptsPerRound) // 정해진 횟수만 받고 라운드 종료(OnCompleted)
                .Share(); // 처리 구독과 완료 대기가 같은 스트림을 공유

            roundHits
                .Subscribe(phase =>
                {
                    m_Attempt.Value += 1;
                    FlashJudgement(m_Model.ApplyHit(phase));
                })
                .AddTo(play);

            await roundHits.WaitAsync(ct); // N회 시도가 끝나면 여기로

            m_StateText.text = $"결과 — 점수 {m_Model.Score.Value:N0} · 최고 콤보 {m_Model.MaxCombo.Value}";
            m_StartButton.interactable = true;
            ParkMarker(); // 결과 화면: 마커 정지(play 구독은 메서드 반환 시 자동 해제)
        }

        private async ValueTask CountdownAsync(CancellationToken ct)
        {
            for (int i = 3; i >= 1; i--)
            {
                m_StateText.text = i.ToString();
                await UniTask.Delay(TimeSpan.FromSeconds(1), cancellationToken: ct);
            }
            m_StateText.text = "시작!";
        }

        // HIT 버튼과 Space 키(Input System)를 하나의 입력 스트림으로 합친다(Merge).
        private Observable<Unit> HitStream() =>
            m_HitButton
                .OnClickAsObservable()
                .Merge(
                    Observable
                        .EveryUpdate()
                        .Where(_ => Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                );

        private void FlashJudgement(Judgement judgement)
        {
            (string label, Color color) = judgement switch
            {
                Judgement.Perfect => ("Perfect!", PerfectColor),
                Judgement.Good => ("Good", GoodColor),
                _ => ("Miss", MissColor),
            };
            m_JudgementText.text = label;
            m_JudgementText.color = color;
        }

        private static Color ColorFor(Zone zone) =>
            zone switch
            {
                Zone.Perfect => PerfectColor,
                Zone.Good => GoodColor,
                _ => NoneColor,
            };

        private void OnDestroy() => m_Model?.Dispose();
    }
}
