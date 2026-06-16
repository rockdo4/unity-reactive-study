using R3;
using UnityEngine;

namespace ReactiveStudy.Basics
{
    /// <summary>
    /// AddTo(this) 시연용 프로브 — 파괴되면 구독이 자동으로 함께 해제된다.
    /// </summary>
    public class LifetimeProbe : MonoBehaviour
    {
        private void Start()
        {
            Observable
                .Interval(System.TimeSpan.FromSeconds(3))
                .Subscribe(_ => Debug.Log("[LifetimeProbe] 살아 있음 (AddTo로 수명 묶임)"))
                .AddTo(this); // 이 GameObject가 파괴되면 자동 Dispose
        }

        private void OnDestroy()
        {
            Debug.Log("[LifetimeProbe] 파괴됨 — 구독도 자동 해제");
        }
    }
}
