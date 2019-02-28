using System.Collections.Generic;
using CodingArena.Player;
using CodingArena.Player.Battlefield;
using CodingArena.Player.TurnActions;
using Moq;
using NUnit.Framework;

namespace CodingArena.Miso.Tests
{
    [TestFixture]
    public class MySuperHeroTests
    {
        private MySuperHero sut;
        private IOwnBot mOwnBot;
        private IEnemy enemyMock;
        private IBattlefieldView mBattleField;
        private IReadOnlyCollection<IEnemy> enemiesCollection;

        [SetUp]
        public void SetUp()
        {
            sut = new MySuperHero();
            mOwnBot = CreateAndSetupOwnBot();
            enemyMock = new Mock<IEnemy>().Object;
            mBattleField = new Mock<IBattlefieldView>().Object;
            enemiesCollection = new List<IEnemy> {enemyMock};
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
            enemyMock = null;
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
        public void WhenNoEnemy_ReturnsIdle()
        {
            var result = sut.GetTurnAction(mOwnBot, new List<IEnemy>(), mBattleField);
            Assert.IsInstanceOf(TurnAction.Idle().GetType(), result, 
                "When the list of enemies is empty, Idle action must be returned.");
        }

        [Test]
        public void WhenEnemysDistanceIsMoreThan9_MyBotMoves()
        {
            var battlefieldPlaceOfOwnBot = new Mock<IBattlefieldPlace>().Object;
            Mock.Get(battlefieldPlaceOfOwnBot).Setup(x => x.X).Returns(0);
            Mock.Get(battlefieldPlaceOfOwnBot).Setup(x => x.Y).Returns(0);
            Mock.Get(mBattleField).Setup(x => x[mOwnBot]).Returns(battlefieldPlaceOfOwnBot);
            var battlefieldPlaceOfEnemyBot = new Mock<IBattlefieldPlace>().Object;
            Mock.Get(battlefieldPlaceOfEnemyBot).Setup(x => x.X).Returns(10);
            Mock.Get(battlefieldPlaceOfEnemyBot).Setup(x => x.Y).Returns(0);
            Mock.Get(mBattleField).Setup(x => x[enemyMock]).Returns(battlefieldPlaceOfEnemyBot);
            var result = sut.GetTurnAction(mOwnBot, enemiesCollection, mBattleField);
            Assert.IsInstanceOf(TurnAction.Move.Towards(battlefieldPlaceOfEnemyBot).GetType(), result,
                "When the distance to the enemy is 10, Move Towards action must be returned.");
        }
    }
}