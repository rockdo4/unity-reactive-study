using UnityEngine;

public class MyLifetimeProbe : MonoBehaviour
{
    private void Start() { }

    private void OnDestroy()
    {
        Debug.Log("[LifetimeProbe] 파괴됨 — 구독도 자동 해제");
    }
}
