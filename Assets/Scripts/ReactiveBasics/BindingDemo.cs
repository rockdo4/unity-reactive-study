using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveStudy.Basics
{
    /// <summary>
    /// 교안 05 대응(전반) — ReactiveProperty와 UI 바인딩.
    /// 값 변화 → UI 자동 갱신, Select+DistinctUntilChanged로 파생 상태,
    /// CombineLatest로 여러 값을 합성해 버튼 활성화를 제어한다.
    /// </summary>
    public class BindingDemo : MonoBehaviour
    {
        [SerializeField]
        private Button m_DamageButton;

        [SerializeField]
        private Button m_HealButton;

        [SerializeField]
        private Button m_UseMpButton;

        [SerializeField]
        private Button m_RestoreMpButton;

        [SerializeField]
        private Button m_SkillButton;

        [SerializeField]
        private TextMeshProUGUI m_HpText;

        [SerializeField]
        private TextMeshProUGUI m_MpText;

        [SerializeField]
        private TextMeshProUGUI m_HpStateText;

        [SerializeField]
        private TextMeshProUGUI m_SkillLogText;

        // 인스펙터에서 초기값을 편집할 수 있도록 SerializableReactiveProperty 사용
        // (일반 ReactiveProperty<T>는 [SerializeField] 직렬화 불가)
        [SerializeField]
        private SerializableReactiveProperty<int> m_Hp = new(100);

        [SerializeField]
        private SerializableReactiveProperty<int> m_Mp = new(50);

        private const int SkillMpCost = 20;

        private void Start()
        {
            // 값 변화 → 텍스트 자동 갱신. 구독 즉시 현재값이 한 번 흐른다(폴링 없음)
            m_Hp.Subscribe(hp => m_HpText.text = $"HP : {hp}").AddTo(this);
            m_Mp.Subscribe(mp => m_MpText.text = $"MP : {mp}").AddTo(this);

            // Select로 파생 상태를 만들고, DistinctUntilChanged로 "구간이 바뀔 때만" 갱신
            m_Hp.Select(hp =>
                    hp <= 0 ? "쓰러짐"
                    : hp < 30 ? "위험"
                    : hp < 70 ? "주의"
                    : "안전"
                )
                .DistinctUntilChanged()
                .Subscribe(state =>
                {
                    m_HpStateText.text = $"상태: {state}";
                    m_HpStateText.color = state switch
                    {
                        "안전" => Color.green,
                        "주의" => Color.yellow,
                        _ => Color.red,
                    };
                })
                .AddTo(this);

            // CombineLatest — HP·MP 둘 다 조건을 만족할 때만 스킬 버튼 활성화
            m_Hp.CombineLatest(m_Mp, (hp, mp) => hp > 0 && mp >= SkillMpCost)
                .DistinctUntilChanged()
                .Subscribe(canUse => m_SkillButton.interactable = canUse)
                .AddTo(this);

            m_DamageButton.OnClickAsObservable().Subscribe(_ => m_Hp.Value = Mathf.Max(0, m_Hp.Value - 10)).AddTo(this);
            m_HealButton.OnClickAsObservable().Subscribe(_ => m_Hp.Value = Mathf.Min(100, m_Hp.Value + 10)).AddTo(this);
            m_UseMpButton.OnClickAsObservable().Subscribe(_ => m_Mp.Value = Mathf.Max(0, m_Mp.Value - 5)).AddTo(this);
            m_RestoreMpButton
                .OnClickAsObservable()
                .Subscribe(_ => m_Mp.Value = Mathf.Min(50, m_Mp.Value + 5))
                .AddTo(this);

            m_SkillButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    m_Mp.Value -= SkillMpCost;
                    m_SkillLogText.text = $"스킬 사용! (MP -{SkillMpCost}, 프레임 {Time.frameCount})";
                })
                .AddTo(this);
        }

        private void OnDestroy()
        {
            m_Hp.Dispose();
            m_Mp.Dispose();
        }
    }
}
