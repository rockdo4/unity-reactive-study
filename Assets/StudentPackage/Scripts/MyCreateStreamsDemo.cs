using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyCreateStreamsDemo : MonoBehaviour
{
    [SerializeField]
    private Button m_RangeButton;

    [SerializeField]
    private Button m_TimerButton;

    [SerializeField]
    private Button m_IntervalToggleButton;

    [SerializeField]
    private TextMeshProUGUI m_IntervalToggleLabel;

    [SerializeField]
    private Button m_SubjectButton;

    [SerializeField]
    private Button m_EventButton;

    [SerializeField]
    private TextMeshProUGUI m_LogText;

    private readonly Subject<string> m_Subject = new();

    private event Action<int> LegacyScoreEvent;

    private readonly Queue<string> m_LogLines = new();
    private IDisposable m_IntervalSubscription;
    private int m_SubjectCount;
    private int m_LegacyScore;

    private void Start() { }

    private void SetToggleLabel(string text)
    {
        if (m_IntervalToggleLabel != null)
            m_IntervalToggleLabel.text = text;
    }

    private void Log(string message)
    {
        m_LogLines.Enqueue(message);
        while (m_LogLines.Count > 7)
            m_LogLines.Dequeue();
        if (m_LogText != null)
            m_LogText.text = string.Join("\n", m_LogLines);
    }

    private void OnDestroy()
    {
        m_Subject.Dispose();
    }
}
