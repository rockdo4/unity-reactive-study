using NUnit.Framework;

namespace ReactiveStudy.Clicker.Tests
{
    /// <summary>
    /// ClickerModel EditMode 단위 테스트.
    ///
    /// Model은 MonoBehaviour가 아닌 순수 C# 클래스이므로 씬·UI 없이 규칙을 직접 검증할 수 있다
    /// (교안 06이 약속한 "Model이 UI를 모르므로 규칙을 단위 테스트할 수 있다"의 실증).
    ///
    /// 여기서는 시간에 의존하지 않는 규칙(클릭·피버 배수·업그레이드 비용/성장·파생 상태)만 검증한다.
    /// 자동 수입 Interval·피버 만료 Timer 같은 시간 의존 스트림을 가상 시간으로 결정론 검증하려면
    /// FakeTimeProvider(Microsoft.Extensions.TimeProvider.Testing)가 필요한데, 이 패키지는
    /// netstandard 타깃이 없고(net462/net6/net8만) 본 프로젝트의 Api Compatibility Level이
    /// .NET Standard 2.0이라 현재 설치·사용할 수 없다. 시간 의존 테스트는 별도 과제로 둔다.
    /// </summary>
    public sealed class ClickerModelTests
    {
        private ClickerModel m_Model;

        [SetUp]
        public void SetUp()
        {
            m_Model = new ClickerModel();
        }

        [TearDown]
        public void TearDown()
        {
            // 생성자에서 건 자동 수입 Interval 구독을 정리해 테스트 간 누수를 막는다.
            m_Model.Dispose();
        }

        [Test]
        public void Click_IncreasesGoldByGoldPerClick()
        {
            long gain = m_Model.Click();

            Assert.AreEqual(1, gain, "기본 GoldPerClick(1)만큼 골드를 얻어야 한다");
            Assert.AreEqual(1, m_Model.Gold.Value);
        }

        [Test]
        public void Click_DuringFever_DoublesGain()
        {
            Assert.IsTrue(m_Model.TryStartFever(), "피버가 시작되어야 한다");
            Assert.IsTrue(m_Model.IsFever.Value);

            long gain = m_Model.Click();

            Assert.AreEqual(2, gain, "피버 중에는 클릭 골드가 2배여야 한다");
            Assert.AreEqual(2, m_Model.Gold.Value);
        }

        [Test]
        public void TryStartFever_IgnoredWhenAlreadyActive()
        {
            Assert.IsTrue(m_Model.TryStartFever());
            Assert.IsFalse(m_Model.TryStartFever(), "이미 피버 중이면 재시작은 무시되어야 한다");
        }

        [Test]
        public void TryBuyClickUpgrade_FailsWhenGoldInsufficient()
        {
            // 시작 골드 0, 비용 10 → 구매 실패하고 상태가 바뀌지 않는다.
            Assert.IsFalse(m_Model.TryBuyClickUpgrade());
            Assert.AreEqual(0, m_Model.Gold.Value);
            Assert.AreEqual(1, m_Model.GoldPerClick.Value);
            Assert.AreEqual(10, m_Model.ClickUpgradeCost.Value);
        }

        [Test]
        public void TryBuyClickUpgrade_DeductsCostAndGrowsPerClickAndCost()
        {
            m_Model.AddBonus(10); // 정확히 살 만큼만 지급

            Assert.IsTrue(m_Model.TryBuyClickUpgrade());
            Assert.AreEqual(0, m_Model.Gold.Value, "비용 10이 차감되어야 한다");
            Assert.AreEqual(2, m_Model.GoldPerClick.Value, "클릭당 골드가 +1 되어야 한다");
            Assert.AreEqual(16, m_Model.ClickUpgradeCost.Value, "다음 비용은 ceil(10 * 1.6) = 16");
        }

        [Test]
        public void TryBuyAutoUpgrade_DeductsCostAndGrowsPerSecondAndCost()
        {
            m_Model.AddBonus(25);

            Assert.IsTrue(m_Model.TryBuyAutoUpgrade());
            Assert.AreEqual(0, m_Model.Gold.Value);
            Assert.AreEqual(1, m_Model.GoldPerSecond.Value, "초당 골드가 +1 되어야 한다");
            Assert.AreEqual(45, m_Model.AutoUpgradeCost.Value, "다음 비용은 ceil(25 * 1.8) = 45");
        }

        [Test]
        public void CanBuyClickUpgrade_TracksAffordability()
        {
            // 파생 상태(CombineLatest)는 골드·비용이 바뀔 때마다 자동 갱신된다.
            Assert.IsFalse(m_Model.CanBuyClickUpgrade.CurrentValue, "골드 0 < 비용 10 → 살 수 없음");

            m_Model.AddBonus(10);

            Assert.IsTrue(m_Model.CanBuyClickUpgrade.CurrentValue, "골드 10 >= 비용 10 → 살 수 있음");
        }

        [Test]
        public void AddBonus_AddsGoldWithoutAffectingMultipliers()
        {
            m_Model.AddBonus(123);

            Assert.AreEqual(123, m_Model.Gold.Value);
            Assert.AreEqual(1, m_Model.GoldPerClick.Value);
            Assert.AreEqual(0, m_Model.GoldPerSecond.Value);
        }
    }
}
