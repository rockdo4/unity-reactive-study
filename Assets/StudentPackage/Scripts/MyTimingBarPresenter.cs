using System;
using System.Threading;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MyTimingBarPresenter : MonoBehaviour
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
    private float m_MarkerSpeed = 0.6f;

    [SerializeField]
    private int m_AttemptsPerRound = 12;

    private void Start() { }
}
