using NUnit.Framework;

namespace ReactiveStudy.TimingBar.Tests
{
    /// <summary>
    /// TimingBarModel EditMode 단위 테스트.
    ///
    /// Model은 MonoBehaviour가 아닌 순수 C# 클래스이므로 씬·UI 없이 규칙을 직접 검증할 수 있다
    /// ("Model이 UI를 모르므로 규칙을 단위 테스트할 수 있다"의 실증).
    /// 여기서는 위상→판정, 콤보 누적/리셋, 점수 배수처럼 시간에 의존하지 않는 규칙만 검증한다.
    /// </summary>
    public sealed class TimingBarModelTests
    {
        private TimingBarModel m_Model;

        [SetUp]
        public void SetUp() => m_Model = new TimingBarModel();

        [TearDown]
        public void TearDown() => m_Model.Dispose();

        [Test]
        public void ZoneOf_Center_IsPerfect()
        {
            Assert.AreEqual(Zone.Perfect, TimingBarModel.ZoneOf(0.5f));
        }

        [Test]
        public void ZoneOf_BetweenPerfectAndGood_IsGood()
        {
            // PerfectHalfWidth(0.04) < 0.10 <= GoodHalfWidth(0.12)
            Assert.AreEqual(Zone.Good, TimingBarModel.ZoneOf(0.5f + 0.10f));
        }

        [Test]
        public void ZoneOf_Outside_IsNone()
        {
            Assert.AreEqual(Zone.None, TimingBarModel.ZoneOf(0.9f));
        }

        [Test]
        public void ApplyHit_Perfect_AddsScoreAndIncrementsCombo()
        {
            Judgement judgement = m_Model.ApplyHit(0.5f);

            Assert.AreEqual(Judgement.Perfect, judgement);
            Assert.AreEqual(1, m_Model.Combo.Value);
            Assert.AreEqual(3, m_Model.Score.Value, "Perfect 3점 × 배수 1 = 3");
        }

        [Test]
        public void ApplyHit_Good_AddsOnePointTimesMultiplier()
        {
            // Good 존(0.5 + 0.10)을 두 번 — 콤보 1, 2에서 배수는 모두 1
            Assert.AreEqual(Judgement.Good, m_Model.ApplyHit(0.6f));
            Assert.AreEqual(Judgement.Good, m_Model.ApplyHit(0.6f));
            Assert.AreEqual(2, m_Model.Combo.Value);
            Assert.AreEqual(2, m_Model.Score.Value, "Good 1점씩 두 번");
        }

        [Test]
        public void ApplyHit_Miss_ResetsComboButKeepsMaxCombo()
        {
            m_Model.ApplyHit(0.5f);
            m_Model.ApplyHit(0.5f);
            Assert.AreEqual(2, m_Model.Combo.Value);

            Judgement judgement = m_Model.ApplyHit(0.95f);

            Assert.AreEqual(Judgement.Miss, judgement);
            Assert.AreEqual(0, m_Model.Combo.Value, "Miss는 콤보를 0으로 리셋한다");
            Assert.AreEqual(2, m_Model.MaxCombo.Value, "최고 콤보는 유지된다");
        }

        [Test]
        public void ApplyHit_ComboMultiplier_AppliesEveryFifthCombo()
        {
            // Perfect 5회: 콤보 1~4는 배수 1(3점), 콤보 5는 배수 2(6점) → 3*4 + 6 = 18
            for (int i = 0; i < 5; i++)
                m_Model.ApplyHit(0.5f);

            Assert.AreEqual(5, m_Model.Combo.Value);
            Assert.AreEqual(18, m_Model.Score.Value);
        }

        [Test]
        public void ResetForNewGame_ClearsAllState()
        {
            m_Model.ApplyHit(0.5f);
            m_Model.ResetForNewGame();

            Assert.AreEqual(0, m_Model.Score.Value);
            Assert.AreEqual(0, m_Model.Combo.Value);
            Assert.AreEqual(0, m_Model.MaxCombo.Value);
        }
    }
}
