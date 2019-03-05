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
        private IOwnBot myOwnBot;
        private IEnemy myEnemyOne;
        private IEnemy myEnemyTwo;
        private IBattlefieldView myBattleField;
        private static IShield myShield;
        private IReadOnlyCollection<IEnemy> enemiesCollection;
        private const int ShieldTooDamagedPercentage = 9;
        private const int BatteryLowPercentage = 19;
        private const int BatteryOkPercentage = 20;
        private const int MaximumEnergy = 500;
        private const int MaximumShield = 200;

        [SetUp]
        public void SetUp()
        {
            sut = new MySuperHero();
            myShield = CreateAndSetupShield();
            myOwnBot = CreateAndSetupOwnBot(myShield);
            myEnemyOne = CreateAndSetupEnemy(0, 5);
            myEnemyTwo = CreateAndSetupEnemy(45, 45);
            myBattleField = new Mock<IBattlefieldView>().Object;
            enemiesCollection = new List<IEnemy> {myEnemyOne};
        }

        [TearDown]
        public void TearDown()
        {
            myOwnBot = null;
            myEnemyOne = null;
            myShield = null;
            myBattleField = null;
            enemiesCollection = null;
            sut = null;
        }

        [Test]
        public void ReturnsTurnActionObject()
        {
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.Not.Null, "Calling the GetTurnAction() method must return not null object.");
        }

        [Test]
        public void WhenClosestEnemyDistanceIs5_MyBotMoves()
        {
            //prepare
            Mock.Get(myOwnBot).Setup(x => x.DistanceTo(myEnemyOne)).Returns(5);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Move.Towards(new Mock<IBattlefieldPlace>().Object).GetType()),
                "When the distance to the enemy is 5, Move Towards action must be returned.");
        }

        [Test]
        public void WhenClosestEnemyDistanceIs4_MyBotAttacks()
        {
            //prepare
            Mock.Get(myOwnBot).Setup(x => x.DistanceTo(myEnemyOne)).Returns(4);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Attack(myEnemyOne).GetType()),
                "When the distance to the enemy is 4, Attack action must be returned.");
        }

        [Test]
        public void WhenBatteryIsLow_RechargeTheBattery()
        {
            SetBatteryPercentage(myOwnBot, BatteryLowPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When battery percentage is as low as {BatteryLowPercentage}, Recharge battery action must be returned.");
        }

        [Test]
        public void WhenBatteryIsNotLow_BatteryIsNotRecharged()
        {
            SetBatteryPercentage(myOwnBot, BatteryOkPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.Not.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When battery percentage is as high as {BatteryOkPercentage}, Recharge battery action must not be returned.");
        }

        [Test]
        public void WhenShieldIsTooDamaged_RechargeTheShieldToMax()
        {
            SetShieldPercentage(myShield, ShieldTooDamagedPercentage);
            const int actualShieldPoints = 100;
            SetShieldActual(myShield, actualShieldPoints);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            //int shieldPointsToMax = MaximumShield - actualShieldPoints;
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Shield(It.IsAny<int>()).GetType()),
                $"When shield percentage is as low as {ShieldTooDamagedPercentage}, Recharge shield action must be returned.");
            //TODO: how to check how many shield points were recharged?
        }

        [Test]
        public void WhenShieldIsTooDamagedAndNotEnoughBattery_RechargeTheBattery()
        {
            SetShieldPercentage(myShield, ShieldTooDamagedPercentage);
            SetBatteryPercentage(myOwnBot, BatteryLowPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When shield percentage is as low as {ShieldTooDamagedPercentage} and battery as low as {BatteryLowPercentage}, " +
                "Recharge battery action must be returned.");
        }

        //[Test]
        //public void TwoEnemies_AttackTheCloserOne()
        //{
        //    enemiesCollection.
        //}

        [Test]
        public void WhenNobodyIsCloseAndShieldIsLow_RechargeTheShield()
        {
            Mock.Get(myOwnBot).Setup(x => x.DistanceTo(myEnemyOne)).Returns(30);
        }

        private void SetBatteryPercentage(IOwnBot ownBot, int batteryPercentage)
        {
            var mEnergy = new Mock<IEnergy>().Object;
            Mock.Get(mEnergy).Setup(x => x.Percent).Returns(batteryPercentage);
            Mock.Get(ownBot).Setup(x => x.Energy).Returns(mEnergy);
        }

        private static IOwnBot CreateAndSetupOwnBot(IShield shield)
        {
            var ownBot = new Mock<IOwnBot>().Object;
            Mock.Get(ownBot).Setup(x => x.Health).Returns(CreateAndSetupHealth());
            Mock.Get(ownBot).Setup(x => x.Shield).Returns(shield);
            Mock.Get(ownBot).Setup(x => x.Energy).Returns(CreateAndSetupEnergy());
            return ownBot;
        }

        private static IHealth CreateAndSetupHealth()
        {
            var health = new Mock<IHealth>().Object;
            Mock.Get(health).Setup(x => x.Percent).Returns(100);
            return health;
        }

        private static IShield CreateAndSetupShield()
        {
            var shield = new Mock<IShield>().Object;
            Mock.Get(shield).Setup(x => x.Percent).Returns(100);
            Mock.Get(shield).Setup(x => x.Maximum).Returns(MaximumShield);
            Mock.Get(shield).Setup(x => x.Actual).Returns(MaximumShield);
            return shield;
        }

        private void SetShieldPercentage(IShield shield, int shieldPercentage)
        {
            Mock.Get(shield).Setup(x => x.Percent).Returns(shieldPercentage);
            Mock.Get(shield).Setup(x => x.Actual).Returns(MaximumShield * shieldPercentage / 100);
        }

        private void SetShieldActual(IShield shield, int actualShieldPoints)
        {
            Mock.Get(shield).Setup(x => x.Actual).Returns(actualShieldPoints);
        }

        private static IEnergy CreateAndSetupEnergy()
        {
            var mEnergy = new Mock<IEnergy>().Object;
            Mock.Get(mEnergy).Setup(x => x.Percent).Returns(100);
            Mock.Get(mEnergy).Setup(x => x.Maximum).Returns(MaximumEnergy);
            Mock.Get(mEnergy).Setup(x => x.Actual).Returns(MaximumEnergy);
            return mEnergy;
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