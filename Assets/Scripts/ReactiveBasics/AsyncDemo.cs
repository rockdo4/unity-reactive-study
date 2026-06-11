using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveStudy.Basics
{
    /// <summary>
    /// 교안 05 대응(후반) — async/await 통합.
    /// SubscribeAwait의 AwaitOperation(Sequential vs Drop) 차이와
    /// CancellationToken에 의한 취소를 시연한다. 버튼을 연타해 보면 차이가 보인다.
    /// </summary>
    public class AsyncDemo : MonoBehaviour
    {
        [SerializeField]
        private Button m_SequentialButton;

        [SerializeField]
        private Button m_DropButton;

        [SerializeField]
        private TextMeshProUGUI m_LogText;

        private readonly Queue<string> m_LogLines = new();
        private int m_SequentialCount;
        private int m_DropCount;

        private void Start()
        {
            // Sequential: 비동기 작업이 끝날 때까지 다음 이벤트를 "줄 세워" 순서대로 처리
            m_SequentialButton
                .OnClickAsObservable()
                .SubscribeAwait(
                    async (_, ct) =>
                    {
                        int id = ++m_SequentialCount;
                        Log($"[순차] 로드 #{id} 시작...");
                        await FakeLoadAsync(ct); // ct: 구독 해제(오브젝트 파괴) 시 자동 취소
                        Log($"[순차] 로드 #{id} 완료");
                    },
                    AwaitOperation.Sequential
                )
                .AddTo(this);

            // Drop: 처리 중에 들어온 이벤트는 버린다 — 저장 버튼 연타 방지에 적합
            m_DropButton
                .OnClickAsObservable()
                .SubscribeAwait(
                    async (_, ct) =>
                    {
                        int id = ++m_DropCount;
                        Log($"[드롭] 로드 #{id} 시작... (처리 중 클릭은 무시)");
                        await FakeLoadAsync(ct);
                        Log($"[드롭] 로드 #{id} 완료");
                    },
                    AwaitOperation.Drop
                )
                .AddTo(this);
        }

        // 가짜 비동기 로드 — 실제로는 Addressables 로드, 네트워크 요청 등이 들어갈 자리
        private static async ValueTask FakeLoadAsync(CancellationToken ct)
        {
            await Task.Delay(TimeSpan.FromSeconds(1.5), ct);
        }

        private void Log(string message)
        {
            m_LogLines.Enqueue(message);
            while (m_LogLines.Count > 6)
                m_LogLines.Dequeue();
            if (m_LogText != null)
                m_LogText.text = string.Join("\n", m_LogLines);
        }
    }
}
