using System.Text;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ReactiveStudy.Basics
{
    /// <summary>
    /// 교안 04 대응 — 구독 수명 관리.
    /// CompositeDisposable로 여러 구독을 한꺼번에 정리하고,
    /// 일부러 누수를 만든 뒤 ObservableTracker로 활성 구독을 추적해 본다.
    /// </summary>
    public class LifetimeDemo : MonoBehaviour
    {
        [SerializeField]
        private Button m_AddSubscriptionButton;

        [SerializeField]
        private Button m_DisposeAllButton;

        [SerializeField]
        private Button m_LeakButton;

        [SerializeField]
        private Button m_TrackerButton;

        [SerializeField]
        private Button m_DestroyProbeButton;

        [SerializeField]
        private GameObject m_ProbeObject;

        [SerializeField]
        private TextMeshProUGUI m_StatusText;

        [SerializeField]
        private TextMeshProUGUI m_TrackerText;

        // 여러 구독을 담아 두었다가 한 번에 Dispose하는 그릇
        private readonly CompositeDisposable m_Disposables = new();
        private int m_AddedCount;
        private int m_LeakedCount;

        private void Awake()
        {
            // 활성 구독 추적 켜기 — 오버헤드가 있으므로 학습·디버그 용도로만 켠다
            ObservableTracker.EnableTracking = true;
            ObservableTracker.EnableStackTrace = true;
        }

        private void Start()
        {
            m_AddSubscriptionButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    int id = ++m_AddedCount;
                    Observable
                        .Interval(System.TimeSpan.FromSeconds(5))
                        .Subscribe(_ => Debug.Log($"[LifetimeDemo] 관리되는 구독 #{id} tick"))
                        .AddTo(m_Disposables); // CompositeDisposable에 등록
                    UpdateStatus($"구독 #{id} 추가");
                })
                .AddTo(this);

            m_DisposeAllButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    m_Disposables.Clear(); // 담아 둔 구독 전부 해제 (그릇은 재사용)
                    UpdateStatus("전체 해제 (Clear)");
                })
                .AddTo(this);

            // 잘못된 예: AddTo도, 변수 보관도 없는 전역 스트림 구독 — 이 구독은 영원히 산다
            m_LeakButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    int id = ++m_LeakedCount;
                    Observable
                        .Interval(System.TimeSpan.FromSeconds(5))
                        .Subscribe(_ => Debug.Log($"[LifetimeDemo] 누수 구독 #{id} tick (해제 불가!)"));
                    UpdateStatus($"누수 구독 #{id} 생성 — Tracker로 확인해 보세요");
                })
                .AddTo(this);

            // ObservableTracker로 현재 살아 있는 구독을 모두 나열
            m_TrackerButton.OnClickAsObservable().Subscribe(_ => DumpTracker()).AddTo(this);

            // 프로브 오브젝트 파괴 → AddTo(this)로 묶인 구독이 자동 해제되는 것을 확인
            m_DestroyProbeButton
                .OnClickAsObservable()
                .Subscribe(_ =>
                {
                    if (m_ProbeObject != null)
                    {
                        Destroy(m_ProbeObject);
                        UpdateStatus("프로브 파괴 — Tracker에서 프로브 구독이 사라졌는지 확인");
                    }
                    else
                    {
                        UpdateStatus("프로브가 이미 파괴되었습니다");
                    }
                })
                .AddTo(this);

            UpdateStatus("대기 중");
        }

        private void DumpTracker()
        {
            var sb = new StringBuilder();
            int count = 0;
            ObservableTracker.ForEachActiveTask(state =>
            {
                count++;
                if (count <= 8)
                    sb.AppendLine($"#{state.TrackingId} {state.FormattedType}");
            });
            if (count > 8)
                sb.AppendLine($"... 외 {count - 8}개");
            if (m_TrackerText != null)
                m_TrackerText.text = $"활성 구독 {count}개\n{sb}";
        }

        private void UpdateStatus(string lastAction)
        {
            if (m_StatusText != null)
                m_StatusText.text = $"관리 중 {m_Disposables.Count}개 · 누수 {m_LeakedCount}개 | {lastAction}";
        }

        private void OnDestroy()
        {
            m_Disposables.Dispose(); // 그릇째 폐기
        }
    }
}
