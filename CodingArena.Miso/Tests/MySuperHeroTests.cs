using CodingArena.Player;
using CodingArena.Player.Battlefield;
using CodingArena.Player.TurnActions;
using Moq;
using NUnit.Framework;
using System.Collections.Generic;

namespace CodingArena.Miso.Tests
{
    [TestFixture]
    public class MySuperHeroTests
    {
        private MySuperHero sut;
        private IOwnBot mOwnBot;
        private IEnemy mEnemy;
        private IBattlefieldView mBattleField;
        private IReadOnlyCollection<IEnemy> enemiesCollection;
        private static IShield mShield;
        private const int MaximumEnergy = 500;
        private const int MaximumShield = 200;

        [SetUp]
        public void SetUp()
        {
            sut = new MySuperHero();
            mOwnBot = CreateAndSetupOwnBot();
            mEnemy = CreateAndSetupEnemy(0, 5);
            mBattleField = new Mock<IBattlefieldView>().Object;
            enemiesCollection = new List<IEnemy> {mEnemy};
        }

        [TearDown]
        public void TearDown()
        {
            mOwnBot = null;
            mEnemy = null;
            mShield = null;
            mBattleField = null;
            enemiesCollection = null;
            sut = null;
        }

        [Test]
        public void ReturnsTurnActionObject()
        {
            //act
            var result = sut.GetTurnAction(mOwnBot, enemiesCollection, mBattleField);
            //verify
            Assert.That(result, Is.Not.Null, "Calling the GetTurnAction() method must return not null object.");
        }

        [Test]
        public void WhenClosestEnemyDistanceIs5_MyBotMoves()
        {
            //prepare
            Mock.Get(mOwnBot).Setup(x => x.DistanceTo(mEnemy)).Returns(5);
            //act
            var result = sut.GetTurnAction(mOwnBot, enemiesCollection, mBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Move.Towards(new Mock<IBattlefieldPlace>().Object).GetType()),
                "When the distance to the enemy is 5, Move Towards action must be returned.");
        }

        [Test]
        public void WhenClosestEnemyDistanceIs4_MyBotAttacks()
        {
            //prepare
            Mock.Get(mOwnBot).Setup(x => x.DistanceTo(mEnemy)).Returns(4);
            //act
            var result = sut.GetTurnAction(mOwnBot, enemiesCollection, mBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Attack(mEnemy).GetType()),
                "When the distance to the enemy is 4, Attack action must be returned.");
        }

        [Test]
        public void WhenBatteryIsLow_RechargeTheBattery()
        {
            int batteryLowPercentage = 19;
            SetBatteryPercentage(mOwnBot, batteryLowPercentage);
            //act
            var result = sut.GetTurnAction(mOwnBot, enemiesCollection, mBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When battery percentage is as low as {batteryLowPercentage}, Recharge battery action must be returned.");
        }

        [Test]
        public void WhenBatteryIsNotLow_BatteryIsNotRecharged()
        {
            int batteryLowPercentage = 20;
            SetBatteryPercentage(mOwnBot, batteryLowPercentage);
            //act
            var result = sut.GetTurnAction(mOwnBot, enemiesCollection, mBattleField);
            //verify
            Assert.That(result, Is.Not.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When battery percentage is as high as {batteryLowPercentage}, Recharge battery action must not be returned.");
        }

        [Test]
        public void WhenShieldIsTooDamaged_RechargeTheShield()
        {
            int shieldLowPercentage = 9;
            SetShieldPercentage(mOwnBot, shieldLowPercentage);
            //act
            var result = sut.GetTurnAction(mOwnBot, enemiesCollection, mBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Shield(It.IsAny<int>()).GetType()),
                $"When shield percentage is as low as {shieldLowPercentage}, Recharge shield action must be returned.");
        }

        private static IEnergy CreateAndSetupEnergyMock()
        {
            var mEnergy = new Mock<IEnergy>().Object;
            Mock.Get(mEnergy).Setup(x => x.Percent).Returns(100);
            Mock.Get(mEnergy).Setup(x => x.Maximum).Returns(MaximumEnergy);
            Mock.Get(mEnergy).Setup(x => x.Actual).Returns(MaximumEnergy);
            return mEnergy;
        }

        private void SetShieldPercentage(IOwnBot ownBot, int shieldPercentage)
        {
            Mock.Get(mShield).Setup(x => x.Percent).Returns(shieldPercentage);
            Mock.Get(mShield).Setup(x => x.Actual).Returns(MaximumShield * shieldPercentage / 100);
            Mock.Get(ownBot).Setup(x => x.Shield).Returns(mShield);
        }

        private void SetBatteryPercentage(IOwnBot ownBot, int batteryPercentage)
        {
            var mEnergy = new Mock<IEnergy>().Object;
            Mock.Get(mEnergy).Setup(x => x.Percent).Returns(batteryPercentage);
            Mock.Get(ownBot).Setup(x => x.Energy).Returns(mEnergy);
        }

        private static IOwnBot CreateAndSetupOwnBot()
        {
            var ownBot = new Mock<IOwnBot>().Object;
            Mock.Get(ownBot).Setup(x => x.Health).Returns(CreateAndSetupHealthMock());
            Mock.Get(ownBot).Setup(x => x.Shield).Returns(CreateAndSetupShieldMock());
            Mock.Get(ownBot).Setup(x => x.Energy).Returns(CreateAndSetupEnergyMock());
            return ownBot;
        }

        private static IShield CreateAndSetupShieldMock()
        {
            mShield = new Mock<IShield>().Object;
            Mock.Get(mShield).Setup(x => x.Percent).Returns(100);
            Mock.Get(mShield).Setup(x => x.Maximum).Returns(MaximumShield);
            Mock.Get(mShield).Setup(x => x.Actual).Returns(MaximumShield);
            return mShield;
        }

        private static IHealth CreateAndSetupHealthMock()
        {
            var mHealth = new Mock<IHealth>().Object;
            Mock.Get(mHealth).Setup(x => x.Percent).Returns(100);
            return mHealth;
        }

        private IEnemy CreateAndSetupEnemy(int xPosition, int yPosition)
        {
            var enemy = new Mock<IEnemy>().Object;
            Mock.Get(enemy).Setup(a => a.Position.X).Returns(xPosition);
            Mock.Get(enemy).Setup(a => a.Position.Y).Returns(yPosition);
            return enemy;
        }
    }
}