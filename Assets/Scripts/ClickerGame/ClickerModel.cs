using System;
using R3;

namespace ReactiveStudy.Clicker
{
    /// <summary>
    /// 클리커 게임의 상태와 규칙. MonoBehaviour가 아닌 순수 C# 클래스다.
    /// 상태는 전부 ReactiveProperty — 값이 바뀌면 구독자(Presenter)에게 자동 통지된다.
    /// Model은 UI를 전혀 모른다(MVP의 Model 역할).
    /// </summary>
    public sealed class ClickerModel : IDisposable
    {
        private const double ClickUpgradeCostGrowth = 1.6;
        private const double AutoUpgradeCostGrowth = 1.8;
        private const int FeverMultiplier = 2;

        public ReactiveProperty<long> Gold { get; } = new(0);
        public ReactiveProperty<long> GoldPerClick { get; } = new(1);
        public ReactiveProperty<long> GoldPerSecond { get; } = new(0);
        public ReactiveProperty<long> ClickUpgradeCost { get; } = new(10);
        public ReactiveProperty<long> AutoUpgradeCost { get; } = new(25);
        public ReactiveProperty<bool> IsFever { get; } = new(false);

        // 파생 상태: 골드·비용이 바뀔 때마다 자동으로 다시 계산된다 (CombineLatest)
        public ReadOnlyReactiveProperty<bool> CanBuyClickUpgrade { get; }
        public ReadOnlyReactiveProperty<bool> CanBuyAutoUpgrade { get; }

        public TimeSpan FeverDuration { get; } = TimeSpan.FromSeconds(5);

        private readonly CompositeDisposable m_Disposables = new();

        public ClickerModel()
        {
            CanBuyClickUpgrade = Gold.CombineLatest(ClickUpgradeCost, (gold, cost) => gold >= cost)
                .ToReadOnlyReactiveProperty()
                .AddTo(m_Disposables);

            CanBuyAutoUpgrade = Gold.CombineLatest(AutoUpgradeCost, (gold, cost) => gold >= cost)
                .ToReadOnlyReactiveProperty()
                .AddTo(m_Disposables);

            // 자동 수입: 1초마다 GoldPerSecond만큼 지급 (도우미를 사기 전에는 0이라 변화 없음)
            Observable
                .Interval(TimeSpan.FromSeconds(1))
                .Where(_ => GoldPerSecond.Value > 0)
                .Subscribe(_ => Gold.Value += GoldPerSecond.Value)
                .AddTo(m_Disposables);

            Gold.AddTo(m_Disposables);
            GoldPerClick.AddTo(m_Disposables);
            GoldPerSecond.AddTo(m_Disposables);
            ClickUpgradeCost.AddTo(m_Disposables);
            AutoUpgradeCost.AddTo(m_Disposables);
            IsFever.AddTo(m_Disposables);
        }

        /// <summary>클릭 1회 — 피버 중이면 2배.</summary>
        public long Click()
        {
            long gain = GoldPerClick.Value * (IsFever.Value ? FeverMultiplier : 1);
            Gold.Value += gain;
            return gain;
        }

        /// <summary>콤보 보너스 등 추가 골드 지급.</summary>
        public void AddBonus(long amount)
        {
            Gold.Value += amount;
        }

        public bool TryBuyClickUpgrade()
        {
            long cost = ClickUpgradeCost.Value;
            if (Gold.Value < cost)
                return false;
            Gold.Value -= cost;
            GoldPerClick.Value += 1;
            ClickUpgradeCost.Value = (long)Math.Ceiling(cost * ClickUpgradeCostGrowth);
            return true;
        }

        public bool TryBuyAutoUpgrade()
        {
            long cost = AutoUpgradeCost.Value;
            if (Gold.Value < cost)
                return false;
            Gold.Value -= cost;
            GoldPerSecond.Value += 1;
            AutoUpgradeCost.Value = (long)Math.Ceiling(cost * AutoUpgradeCostGrowth);
            return true;
        }

        /// <summary>피버 시작 — FeverDuration 동안 클릭 골드 2배. 이미 피버 중이면 무시.</summary>
        public bool TryStartFever()
        {
            if (IsFever.Value)
                return false;
            IsFever.Value = true;
            Observable.Timer(FeverDuration).Subscribe(_ => IsFever.Value = false).AddTo(m_Disposables);
            return true;
        }

        public void Dispose()
        {
            m_Disposables.Dispose();
        }
    }
}
