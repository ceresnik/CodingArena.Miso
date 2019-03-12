using System;
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
        private IShield myShield;
        private IHealth myHealth;
        private IEnergy myEnergy;
        private IReadOnlyCollection<IEnemy> enemiesCollection;
        private const int OwnBotInitialPositionX = 25;
        private const int OwnBotInitialPositionY = 25;
        private const int EnemyOneInitialPositionX = 0;
        private const int EnemyOneInitialPositionY = 5;
        private const int HealthTooLowPercentage = 40;
        private const int ShieldTooDamagedPercentage = 9;
        private const int ShieldNotFullPercentage = 59;
        private const int BatteryNotFullPercentage = 90;
        private const int BatteryLowPercentage = 9;
        private const int BatteryOkPercentage = 20;
        private const int MaximumHealth = 500;
        private const int MaximumShield = 200;
        private const int MaximumEnergy = 500;

        [SetUp]
        public void SetUp()
        {
            sut = new MySuperHero();
            myShield = CreateAndSetupShield();
            SetShieldPercentageAndActualShieldPoints(100);
            myHealth = CreateAndSetupHealth();
            myEnergy = CreateAndSetupEnergy();
            myOwnBot = CreateAndSetupOwnBot(OwnBotInitialPositionX, OwnBotInitialPositionY);
            myEnemyOne = CreateAndSetupEnemy(EnemyOneInitialPositionX, EnemyOneInitialPositionY);
            SetDistanceToEnemy(myEnemyOne);
            myBattleField = new Mock<IBattlefieldView>().Object;
            enemiesCollection = new List<IEnemy> {myEnemyOne};
        }

        private void SetDistanceToEnemy(IEnemy enemy)
        {
            double distanceToEnemy = Math.Sqrt(Math.Pow(myOwnBot.Position.X - EnemyOneInitialPositionX, 2) 
                                               + Math.Pow(myOwnBot.Position.Y - EnemyOneInitialPositionY, 2));
            Mock.Get(myOwnBot).Setup(x => x.DistanceTo(enemy)).Returns(distanceToEnemy);
        }

        [TearDown]
        public void TearDown()
        {
            myShield = null;
            myHealth = null;
            myEnergy = null;
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
        public void WhenUnderAttackAndShieldIsTooDamagedAndNotEnoughBattery_RunAwayFromEnemy()
        {
            Mock.Get(myOwnBot).Setup(x => x.Position)
                .Returns(CreateAndSetupPosition(EnemyOneInitialPositionX + 1, EnemyOneInitialPositionY + 1));
            SetDistanceToEnemy(myEnemyOne);
            SetShieldPercentageAndActualShieldPoints(ShieldTooDamagedPercentage);
            SetBatteryPercentageAndActualEnergyPoints(BatteryLowPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Move.AwayFrom(new Mock<IBattlefieldPlace>().Object).GetType()),
                $"When under attack and shield percentage is as low as {ShieldTooDamagedPercentage} " +
                $"and battery is as low as {BatteryLowPercentage}, " +
                "Run away from enemy action must be returned.");
        }

        [Test]
        public void WhenNobodyIsCloseAndShieldIsNotOk_RechargeTheShield()
        {
            SetShieldPercentageAndActualShieldPoints(ShieldNotFullPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Shield(It.IsAny<int>()).GetType()),
                $"When nobody is close and shield percentage is as low as {ShieldNotFullPercentage}, " +
                "Recharge Shield action must be returned.");
        }

        [Test]
        public void WhenNobodyIsCloseAndBatteryIsNotOk_BatteryIsRecharged()
        {
            SetBatteryPercentageAndActualEnergyPoints(BatteryNotFullPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Battery().GetType()),
                $"When nobody is close and battery percentage is as low as {BatteryNotFullPercentage}, " +
                "Recharge battery action must be returned.");
        }

        [Test]
        public void WhenIsVeryMuchInjuredAndShieldIsDamaged_RechargeTheShield()
        {
            SetShieldPercentageAndActualShieldPoints(ShieldTooDamagedPercentage);
            SetHealthPercentageAndActualHealthPoints(HealthTooLowPercentage);
            //act
            var result = sut.GetTurnAction(myOwnBot, enemiesCollection, myBattleField);
            //verify
            Assert.That(result, Is.InstanceOf(TurnAction.Recharge.Shield(It.IsAny<int>()).GetType()),
                $"When health percentage is as low as {HealthTooLowPercentage} and shield percentage is as low as " +
                $"{ShieldTooDamagedPercentage}, Recharge Shield action must be returned.");
        }

        private IOwnBot CreateAndSetupOwnBot(int positionX, int positionY)
        {
            var ownBot = new Mock<IOwnBot>().Object;
            Mock.Get(ownBot).Setup(x => x.Health).Returns(myHealth);
            Mock.Get(ownBot).Setup(x => x.Shield).Returns(myShield);
            Mock.Get(ownBot).Setup(x => x.Energy).Returns(myEnergy);
            Mock.Get(ownBot).Setup(x => x.Position).Returns(CreateAndSetupPosition(positionX, positionY));
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
            return shield;
        }

        private IBattlefieldPlace CreateAndSetupPosition(int positionX, int positionY)
        {
            var mPosition = new Mock<IBattlefieldPlace>().Object;
            Mock.Get(mPosition).Setup(x => x.X).Returns(positionX);
            Mock.Get(mPosition).Setup(x => x.Y).Returns(positionY);
            return mPosition;
        }

        private void SetHealthPercentageAndActualHealthPoints(int healthTooLowPercentage)
        {
            Mock.Get(myHealth).Setup(x => x.Percent).Returns(healthTooLowPercentage);
            Mock.Get(myHealth).Setup(x => x.Actual).Returns(healthTooLowPercentage * MaximumHealth / 100);
        }

        private void SetShieldPercentageAndActualShieldPoints(int shieldPercentage)
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