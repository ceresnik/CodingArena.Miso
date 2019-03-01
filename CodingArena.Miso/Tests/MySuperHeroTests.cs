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

        [SetUp]
        public void SetUp()
        {
            sut = new MySuperHero();
            mOwnBot = CreateAndSetupOwnBot();
            mEnemy = CreateAndSetupEnemy(0, 5);
            mBattleField = new Mock<IBattlefieldView>().Object;
            enemiesCollection = new List<IEnemy> {mEnemy};
        }

        private static IEnergy CreateAndSetupEnergyMock()
        {
            var mEnergy = new Mock<IEnergy>().Object;
            Mock.Get(mEnergy).Setup(x => x.Percent).Returns(100);
            return mEnergy;
        }

        [TearDown]
        public void TearDown()
        {
            mOwnBot = null;
            mEnemy = null;
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
            var mShield = new Mock<IShield>().Object;
            Mock.Get(mShield).Setup(x => x.Percent).Returns(100);
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