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
        private IBattlefieldView myBattleField;
        private static IShield myShield;
        private static IHealth myHealth;
        private static IEnergy myEnergy;
        private static IBattlefieldPlace myPosition;
        private IReadOnlyCollection<IEnemy> enemiesCollection;
        private const int ShieldTooDamagedPercentage = 9;
        private const int ShieldNotOkPercentage = 90;
        private const int BatteryLowPercentage = 19;
        private const int BatteryOkPercentage = 20;
        private const int MaximumEnergy = 500;
        private const int MaximumShield = 200;

        [SetUp]
        public void SetUp()
        {
            sut = new MySuperHero();
            myShield = CreateAndSetupShield();
            myHealth = CreateAndSetupHealth();
            myEnergy = CreateAndSetupEnergy();
            myPosition = CreateAndSetupPosition();
            myOwnBot = CreateAndSetupOwnBot();
            myEnemyOne = CreateAndSetupEnemy(0, 5);
            CreateAndSetupEnemy(45, 45);
            myBattleField = new Mock<IBattlefieldView>().Object;
            enemiesCollection = new List<IEnemy> {myEnemyOne};
        }

        [TearDown]
        public void TearDown()
        {
            myShield = null;
            myHealth = null;
            myEnergy = null;
            myPosition = null;
            myOwnBot = null;
            myEnemyOne = null;
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
            SetBatteryPercentageAndActualEnergyPoints(BatteryLowPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When battery percentage is as low as {BatteryLowPercentage}, Recharge battery action must be returned.");
        }

        [Test]
        public void WhenBatteryIsNotLow_BatteryIsNotRecharged()
        {
            SetBatteryPercentageAndActualEnergyPoints(BatteryOkPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.Not.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When battery percentage is as high as {BatteryOkPercentage}, Recharge battery action must not be returned.");
        }

        [Test]
        public void WhenShieldIsTooDamaged_RechargeTheShieldToMax()
        {
            SetShieldPercentageAndActualShieldPoints(ShieldTooDamagedPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Shield(It.IsAny<int>()).GetType()),
                $"When shield percentage is as low as {ShieldTooDamagedPercentage}, Recharge shield action must be returned.");
            //TODO: how to check how many shield points were recharged?
            //int shieldPointsToMax = MaximumShield - actualShieldPoints;
        }

        [Test]
        public void WhenShieldIsTooDamagedAndNotEnoughBattery_RechargeTheBattery()
        {
            SetShieldPercentageAndActualShieldPoints(ShieldTooDamagedPercentage);
            SetBatteryPercentageAndActualEnergyPoints(BatteryLowPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When shield percentage is as low as {ShieldTooDamagedPercentage} and battery as low as {BatteryLowPercentage}, " +
                "Recharge battery action must be returned.");
        }

        [Test]
        public void WhenNobodyIsCloseAndShieldIsNotOk_RechargeTheShield()
        {
            Mock.Get(myOwnBot).Setup(x => x.DistanceTo(myEnemyOne)).Returns(30);
            SetShieldPercentageAndActualShieldPoints(ShieldNotOkPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Shield(It.IsAny<int>()).GetType()),
                $"When nobody is close and shield percentage is as low as {ShieldNotOkPercentage}, " +
                "Recharge Shield action must be returned.");
        }

        private static IOwnBot CreateAndSetupOwnBot()
        {
            var ownBot = new Mock<IOwnBot>().Object;
            Mock.Get(ownBot).Setup(x => x.Health).Returns(myHealth);
            Mock.Get(ownBot).Setup(x => x.Shield).Returns(myShield);
            Mock.Get(ownBot).Setup(x => x.Energy).Returns(myEnergy);
            Mock.Get(ownBot).Setup(x => x.Position).Returns(myPosition);
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
            Mock.Get(shield).Setup(x => x.Maximum).Returns(MaximumShield);
            Mock.Get(shield).Setup(x => x.Percent).Returns(100);
            Mock.Get(shield).Setup(x => x.Actual).Returns(MaximumShield);
            return shield;
        }

        private IBattlefieldPlace CreateAndSetupPosition()
        {
            var mPosition = new Mock<IBattlefieldPlace>().Object;
            Mock.Get(mPosition).Setup(x => x.X).Returns(25);
            Mock.Get(mPosition).Setup(x => x.Y).Returns(25);
            return mPosition;
        }

        private static void SetShieldPercentageAndActualShieldPoints(int shieldPercentage)
        {
            Mock.Get(myShield).Setup(x => x.Percent).Returns(shieldPercentage);
            Mock.Get(myShield).Setup(x => x.Actual).Returns(shieldPercentage * MaximumShield / 100);
        }

        private void SetBatteryPercentageAndActualEnergyPoints(int batteryPercentage)
        {
            Mock.Get(myEnergy).Setup(x => x.Percent).Returns(batteryPercentage);
            Mock.Get(myEnergy).Setup(x => x.Actual).Returns(batteryPercentage * MaximumEnergy / 100);
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