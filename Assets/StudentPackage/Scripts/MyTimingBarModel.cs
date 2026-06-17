using System;
using R3;

public enum Zone
{
    None,
    Good,
    Perfect,
}

public enum Judgement
{
    Miss,
    Good,
    Perfect,
}

public sealed class MyTimingBarModel : IDisposable
{
    public const float PerfectHalfWidth = 0.04f;
    public const float GoodHalfWidth = 0.12f;

    private const int PerfectScore = 3;
    private const int GoodScore = 1;
    private const int ComboPerMultiplier = 5;

    public ReactiveProperty<int> Score { get; } = new(0);
    public ReactiveProperty<int> Combo { get; } = new(0);
    public ReactiveProperty<int> MaxCombo { get; } = new(0);

    private readonly CompositeDisposable m_Disposables = new();

    public MyTimingBarModel()
    {
        Score.AddTo(m_Disposables);
        Combo.AddTo(m_Disposables);
        MaxCombo.AddTo(m_Disposables);
    }

    public static Zone ZoneOf(float phase)
    {
        return Zone.None;
    }

    public static Judgement Judge(float phase)
    {
        return Judgement.Miss;
    }

    public Judgement ApplyHit(float phase)
    {
        return Judgement.Miss;
    }

    public void ResetForNewGame() { }

    public void Dispose() => m_Disposables.Dispose();
}
