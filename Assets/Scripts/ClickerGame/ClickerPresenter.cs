using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveStudy.Clicker
{
    /// <summary>
    /// 클리커 게임의 Presenter — 입력(버튼)을 Model 호출로 바꾸고,
    /// Model의 ReactiveProperty를 UI에 바인딩한다(MVP의 Presenter 역할).
    /// 콤보·피버 게이트 같은 "입력 스트림 가공"도 여기서 연산자로 처리한다.
    /// </summary>
    public class ClickerPresenter : MonoBehaviour
    {
        [Header("입력")]
        [SerializeField]
        private Button m_ClickButton;

        [SerializeField]
        private Button m_FeverButton;

        [SerializeField]
        private Button m_ClickUpgradeButton;

        [SerializeField]
        private Button m_AutoUpgradeButton;

        [Header("표시")]
        [SerializeField]
        private TextMeshProUGUI m_GoldText;

        [SerializeField]
        private TextMeshProUGUI m_StatsText;

        [SerializeField]
        private TextMeshProUGUI m_ComboText;

        [SerializeField]
        private TextMeshProUGUI m_FeverStateText;

        [SerializeField]
        private TextMeshProUGUI m_ClickUpgradeLabel;

        [SerializeField]
        private TextMeshProUGUI m_AutoUpgradeLabel;

        [SerializeField]
        private TextMeshProUGUI m_EventLogText;

        [SerializeField]
        private Image m_ClickButtonImage;

        private static readonly TimeSpan ComboResetDelay = TimeSpan.FromMilliseconds(800);
        private static readonly TimeSpan FeverCooldown = TimeSpan.FromSeconds(15);
        private const int ComboBonusUnit = 10; // 콤보 10·20·30...마다 보너스

        private readonly Queue<string> m_LogLines = new();
        private ClickerModel m_Model;

        private void Start()
        {
            m_Model = new ClickerModel();

            BindInput();
            BindView();
        }

        private void BindInput()
        {
            // 클릭 스트림은 본체·콤보·보너스 세 곳에서 쓰므로 Share로 구독을 공유한다
            var clicks = m_ClickButton.OnClickAsObservable().Share();

            // 1) 클릭 → 골드 (입력을 Model 호출로 변환)
            clicks.Subscribe(_ => m_Model.Click()).AddTo(this);

            // 2) 콤보 — 클릭은 +1, 0.8초 잠잠해지면(Debounce) 0으로 리셋. Merge+Scan 누적기
            var combo = clicks
                .Select(_ => 1)
                .Merge(clicks.Debounce(ComboResetDelay).Select(_ => 0))
                .Scan(0, (acc, x) => x == 0 ? 0 : acc + 1)
                .Share();

            combo.Subscribe(c => m_ComboText.text = c >= 2 ? $"{c} 콤보!" : string.Empty).AddTo(this);

            // 3) 콤보 보너스 — 10·20·30...에 도달하는 순간 보너스 골드
            combo
                .Where(c => c > 0 && c % ComboBonusUnit == 0)
                .Subscribe(c =>
                {
                    long bonus = c * m_Model.GoldPerClick.Value;
                    m_Model.AddBonus(bonus);
                    Log($"{c}콤보 달성! 보너스 +{bonus:N0}");
                })
                .AddTo(this);

            // 4) 피버 — ThrottleFirst가 쿨다운 게이트: 첫 클릭만 통과, 15초간 나머지는 무시
            m_FeverButton
                .OnClickAsObservable()
                .ThrottleFirst(FeverCooldown)
                .Subscribe(_ =>
                {
                    if (m_Model.TryStartFever())
                        Log($"피버 시작! {m_Model.FeverDuration.TotalSeconds:0}초간 클릭 2배");
                })
                .AddTo(this);

            // 5) 업그레이드 구매
            m_ClickUpgradeButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (m_Model.TryBuyClickUpgrade())
                        Log($"클릭 강화! 클릭당 +{m_Model.GoldPerClick.Value:N0}");
                })
                .AddTo(this);

            m_AutoUpgradeButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (m_Model.TryBuyAutoUpgrade())
                        Log($"도우미 고용! 초당 +{m_Model.GoldPerSecond.Value:N0}");
                })
                .AddTo(this);
        }

        private void BindView()
        {
            // Model의 값 변화 → UI 자동 갱신 (Update 폴링 없음)
            m_Model.Gold.Subscribe(gold => m_GoldText.text = $"골드 {gold:N0}").AddTo(this);

            m_Model
                .GoldPerClick.CombineLatest(
                    m_Model.GoldPerSecond,
                    (perClick, perSec) => $"클릭당 +{perClick:N0} · 초당 +{perSec:N0}"
                )
                .Subscribe(text => m_StatsText.text = text)
                .AddTo(this);

            // 구매 가능 여부 → 버튼 활성화 (Model의 파생 상태를 그대로 바인딩)
            m_Model.CanBuyClickUpgrade.Subscribe(can => m_ClickUpgradeButton.interactable = can).AddTo(this);
            m_Model.CanBuyAutoUpgrade.Subscribe(can => m_AutoUpgradeButton.interactable = can).AddTo(this);

            m_Model
                .ClickUpgradeCost.Subscribe(cost => m_ClickUpgradeLabel.text = $"클릭 강화\n비용 {cost:N0}")
                .AddTo(this);
            m_Model
                .AutoUpgradeCost.Subscribe(cost => m_AutoUpgradeLabel.text = $"도우미 고용\n비용 {cost:N0}")
                .AddTo(this);

            // 피버 상태 → 안내 문구 + 클릭 버튼 색
            m_Model
                .IsFever.Subscribe(isFever =>
                {
                    m_FeverStateText.text = isFever ? "피버 중! 클릭 2배" : "피버 (15초 쿨다운)";
                    if (m_ClickButtonImage != null)
                        m_ClickButtonImage.color = isFever ? new Color(1f, 0.6f, 0.2f) : Color.white;
                })
                .AddTo(this);
        }

        private void Log(string message)
        {
            m_LogLines.Enqueue(message);
            while (m_LogLines.Count > 5)
                m_LogLines.Dequeue();
            if (m_EventLogText != null)
                m_EventLogText.text = string.Join("\n", m_LogLines);
        }

        private void OnDestroy()
        {
            m_Model?.Dispose(); // Presenter가 파괴되면 Model(자동 수입 Interval 포함)도 정리
        }
    }
}
