using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyClickerPresenter : MonoBehaviour
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
    private const int ComboBonusUnit = 10;

    private readonly Queue<string> m_LogLines = new();
    private MyClickerModel m_Model;

    private void Start()
    {
        m_Model = new MyClickerModel();

        BindInput();
        BindView();
    }

    private void BindInput() { }

    private void BindView() { }

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
        m_Model?.Dispose();
    }
}
