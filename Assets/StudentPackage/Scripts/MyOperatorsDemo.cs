using System;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MyOperatorsDemo : MonoBehaviour
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
        var clickStream = Observable
            .EveryUpdate()
            .Where(_ => Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            .Share();

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

        taps.Chunk(TimeSpan.FromMilliseconds(500), 3)
            .Where(xs => xs.Length >= 3)
            .Subscribe(_ => SetText(m_TapText, $"3연타! (프레임 {Time.frameCount})"))
            .AddTo(this);

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
