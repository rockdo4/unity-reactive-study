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

    private void SetupDoubleClick() { }

    private void SetupSearchDebounce() { }

    private void SetupTapAndCooldown() { }

    private static void SetText(TextMeshProUGUI label, string text)
    {
        if (label != null)
            label.text = text;
    }
}
