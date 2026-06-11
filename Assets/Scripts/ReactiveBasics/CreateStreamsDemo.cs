using System;
using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveStudy.Basics
{
    /// <summary>
    /// 교안 02 대응 — Observable을 만드는 다섯 가지 방법.
    /// 팩토리(Range/Timer/Interval), Subject, ReactiveProperty, FromEvent를
    /// 버튼으로 하나씩 실행해 로그로 확인한다.
    /// </summary>
    public class CreateStreamsDemo : MonoBehaviour
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

        // Subject: 코드 어디서든 OnNext로 값을 직접 밀어 넣는 발행기
        private readonly Subject<string> m_Subject = new();

        // 기존 C# event. FromEvent로 Observable로 감싸는 시연용
        private event Action<int> LegacyScoreEvent;

        private readonly Queue<string> m_LogLines = new();
        private IDisposable m_IntervalSubscription;
        private int m_SubjectCount;
        private int m_LegacyScore;

        private void Start()
        {
            // 1) Range — 콜드(cold) Observable: 구독하는 순간 값이 흐르고 즉시 완료된다
            m_RangeButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Log("--- Range(1, 4) 구독 ---");
                    Observable
                        .Range(1, 4)
                        .Subscribe(x => Log($"Range OnNext: {x}"), result => Log($"Range OnCompleted: {result}"))
                        .AddTo(this);
                })
                .AddTo(this);

            // 2) Timer — 지정 시간 뒤 한 번 발행하고 완료되는 일회성 스트림
            m_TimerButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    Log("Timer(2초) 시작...");
                    Observable.Timer(TimeSpan.FromSeconds(2)).Subscribe(_ => Log("Timer 발행! (2초 경과)")).AddTo(this);
                })
                .AddTo(this);

            // 3) Interval — 일정 주기로 무한 발행. 멈추려면 구독을 직접 Dispose해야 한다
            m_IntervalToggleButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (m_IntervalSubscription == null)
                    {
                        int tick = 0;
                        m_IntervalSubscription = Observable
                            .Interval(TimeSpan.FromSeconds(1))
                            .Subscribe(_ => Log($"Interval tick {++tick}"))
                            .AddTo(this); // 토글을 깜빡해도 오브젝트 파괴 시 함께 해제되도록 이중 안전망
                        Log("Interval 시작 (1초 간격)");
                        SetToggleLabel("Interval 중지");
                    }
                    else
                    {
                        m_IntervalSubscription.Dispose(); // Dispose가 곧 구독 해제
                        m_IntervalSubscription = null;
                        Log("Interval 중지 (Dispose 호출)");
                        SetToggleLabel("Interval 시작");
                    }
                })
                .AddTo(this);

            // 4) Subject — OnNext를 호출해 직접 발행
            m_Subject.Subscribe(msg => Log($"Subject 수신: {msg}")).AddTo(this);
            m_SubjectButton
                .OnClickAsObservable()
                .Subscribe(_ => m_Subject.OnNext($"메시지 #{++m_SubjectCount}"))
                .AddTo(this);

            // 5) FromEvent — 기존 C# event를 Observable로 감싼다 (구독 해제 시 -=도 자동)
            Observable
                .FromEvent<int>(h => LegacyScoreEvent += h, h => LegacyScoreEvent -= h)
                .Subscribe(score => Log($"FromEvent 수신: 점수 {score}"))
                .AddTo(this);
            m_EventButton
                .OnClickAsObservable()
                .Subscribe(_ => LegacyScoreEvent?.Invoke(m_LegacyScore += 10))
                .AddTo(this);
        }

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
            m_Subject.Dispose(); // 발행기(Subject)도 IDisposable
        }
    }
}
