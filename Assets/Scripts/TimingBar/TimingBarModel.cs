using System;
using R3;

namespace ReactiveStudy.TimingBar
{
    /// <summary>마커가 트랙(0~1 위상)에서 머무는 구간.</summary>
    public enum Zone
    {
        None,
        Good,
        Perfect,
    }

    /// <summary>한 번의 "치기" 판정 결과.</summary>
    public enum Judgement
    {
        Miss,
        Good,
        Perfect,
    }

    /// <summary>
    /// 타이밍 바의 상태와 규칙. MonoBehaviour가 아닌 순수 C# 클래스다(MVP의 Model).
    /// 점수·콤보는 ReactiveProperty — 값이 바뀌면 구독자(Presenter)가 자동으로 UI를 갱신한다.
    /// 위상(마커 위치) 계산·프레임·Time 등 Unity 의존은 Presenter가 맡고,
    /// Model은 "위상 → 판정 → 점수"라는 순수 규칙만 안다. 그래서 UI 없이 규칙을 테스트할 수 있다.
    /// </summary>
    public sealed class TimingBarModel : IDisposable
    {
        // 트랙을 0~1로 볼 때 중앙(0.5) 기준 반폭. Perfect 가 Good 안에 겹친다.
        public const float PerfectHalfWidth = 0.04f;
        public const float GoodHalfWidth = 0.12f;

        private const int PerfectScore = 3;
        private const int GoodScore = 1;
        private const int ComboPerMultiplier = 5; // 5콤보마다 점수 배수 +1

        public ReactiveProperty<int> Score { get; } = new(0);
        public ReactiveProperty<int> Combo { get; } = new(0);
        public ReactiveProperty<int> MaxCombo { get; } = new(0);

        private readonly CompositeDisposable m_Disposables = new();

        public TimingBarModel()
        {
            Score.AddTo(m_Disposables);
            Combo.AddTo(m_Disposables);
            MaxCombo.AddTo(m_Disposables);
        }

        /// <summary>위상(0~1)이 어느 구간인지 — 순수 함수.</summary>
        public static Zone ZoneOf(float phase)
        {
            float distance = Math.Abs(phase - 0.5f);
            if (distance <= PerfectHalfWidth)
                return Zone.Perfect;
            if (distance <= GoodHalfWidth)
                return Zone.Good;
            return Zone.None;
        }

        /// <summary>위상(0~1) → 판정 — 순수 함수.</summary>
        public static Judgement Judge(float phase) =>
            ZoneOf(phase) switch
            {
                Zone.Perfect => Judgement.Perfect,
                Zone.Good => Judgement.Good,
                _ => Judgement.Miss,
            };

        /// <summary>
        /// 한 번의 치기를 반영한다. Miss 면 콤보를 0으로,
        /// 아니면 콤보 +1·최고 콤보 갱신·(기본점 × 콤보 배수)만큼 점수를 더한다.
        /// 콤보 누적을 Model 이 단일 소유하므로 규칙을 그대로 단위 테스트할 수 있다.
        /// </summary>
        public Judgement ApplyHit(float phase)
        {
            Judgement judgement = Judge(phase);
            if (judgement == Judgement.Miss)
            {
                Combo.Value = 0;
                return judgement;
            }

            Combo.Value += 1;
            if (Combo.Value > MaxCombo.Value)
                MaxCombo.Value = Combo.Value;

            int basePoints = judgement == Judgement.Perfect ? PerfectScore : GoodScore;
            int multiplier = 1 + Combo.Value / ComboPerMultiplier;
            Score.Value += basePoints * multiplier;
            return judgement;
        }

        /// <summary>새 게임을 위해 상태를 초기화한다.</summary>
        public void ResetForNewGame()
        {
            Score.Value = 0;
            Combo.Value = 0;
            MaxCombo.Value = 0;
        }

        public void Dispose() => m_Disposables.Dispose();
    }
}
