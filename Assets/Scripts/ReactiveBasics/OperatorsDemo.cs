using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace ReactiveStudy.Basics
{
    /// <summary>
    /// 교안 03 대응 — 연산자로 스트림을 다듬고 합성한다.
    /// 더블클릭(Chunk+Debounce), 검색어 입력(Debounce+DistinctUntilChanged),
    /// 연타 감지(Chunk)와 쿨다운(ThrottleFirst)을 시연한다.
    /// </summary>
    public class OperatorsDemo : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI m_DoubleClickText;

        [SerializeField]
        private TMP_InputField m_SearchInput;

        [SerializeField]
        private TextMeshProUGUI m_SearchResultText;

        [SerializeField]
        private Button m_TapButton;

        [SerializeField]
        private TextMeshProUGUI m_TapText;

        [SerializeField]
        private TextMeshProUGUI m_CooldownText;

        private void Start()
        {
            SetupDoubleClick();
            SetupSearchDebounce();
            SetupTapAndCooldown();
        }

        private void SetupDoubleClick()
        {
            // 매 프레임 스트림에서 "왼쪽 버튼이 눌린 프레임"만 거른다 (Input System 사용)
            // Share(): 아래에서 본체와 Debounce 경계 두 곳이 같은 스트림을 쓰므로 구독을 하나로 공유한다
            var clickStream = Observable
                .EveryUpdate()
                .Where(_ => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
                .Share();

            // 클릭이 250ms 동안 잠잠해지면(Debounce) 그때까지 모인(Chunk) 클릭을 한 묶음으로.
            // 묶음에 2개 이상 들어 있으면 더블클릭이다.
            clickStream
                .Chunk(clickStream.Debounce(TimeSpan.FromMilliseconds(250)))
                .Where(clicks => clicks.Length >= 2)
                .Subscribe(clicks =>
                    SetText(m_DoubleClickText, $"더블클릭! ({clicks.Length}회 묶음, 프레임 {Time.frameCount})")
                )
                .AddTo(this);
        }

        private void SetupSearchDebounce()
        {
            // 타이핑이 500ms 멈췄을 때만, 그리고 직전 검색어와 다를 때만 "검색"을 실행한다.
            // 자동완성·설정 자동 저장에서 흔히 쓰는 조합 (Debounce + DistinctUntilChanged)
            m_SearchInput
                .onValueChanged.AsObservable()
                .Debounce(TimeSpan.FromMilliseconds(500))
                .DistinctUntilChanged()
                .Subscribe(query =>
                    SetText(
                        m_SearchResultText,
                        string.IsNullOrWhiteSpace(query) ? "검색 대기 중..." : $"검색 실행: \"{query}\""
                    )
                )
                .AddTo(this);
        }

        private void SetupTapAndCooldown()
        {
            var taps = m_TapButton.OnClickAsObservable().Share();

            // 500ms 안에 3번 누르면 연타 (Chunk: 시간 또는 개수 경계로 묶기)
            taps.Chunk(TimeSpan.FromMilliseconds(500), 3)
                .Where(xs => xs.Length >= 3)
                .Subscribe(_ => SetText(m_TapText, $"3연타! (프레임 {Time.frameCount})"))
                .AddTo(this);

            // 첫 클릭만 통과시키고 2초간 무시 (ThrottleFirst: 스킬 쿨다운)
            taps.ThrottleFirst(TimeSpan.FromSeconds(2))
                .Subscribe(_ => SetText(m_CooldownText, $"발동! 2초 쿨다운 (프레임 {Time.frameCount})"))
                .AddTo(this);
        }

        private static void SetText(TextMeshProUGUI label, string text)
        {
            if (label != null)
                label.text = text;
        }
    }
}
