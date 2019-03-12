using System.Collections.Generic;
using System.Linq;
using CodingArena.Player;
using CodingArena.Player.Battlefield;
using CodingArena.Player.Implement;
using CodingArena.Player.TurnActions;

namespace CodingArena.Miso
{
    public class MySuperHero : IBotAI
    {
        private IOwnBot myOwnBot;
        private IEnemy myClosestEnemy;
        private const int BatteryLowPercentage = 20;
        private const int ShieldDamagedPercentage = 10;
        private int myLastShieldPercent = 100;
        private int myLastHealthPercent = 100;
        private const int ShieldSeriouslyDamagedPercentage = 10;
        private const int BatteryNotFullPercentage = 90;
        private const int ShieldNotFullPercentage = 60;
        private const int SeriouslyInjuredPercentage = 30;
        private const int SafeDistanceFromEnemy = 5;

        public string BotName => "Miso";

        public Model Model => Model.Rust;

        public ITurnAction GetTurnAction(IOwnBot ownBot, IReadOnlyCollection<IEnemy> enemies, IBattlefieldView battlefield)
        {
            if (myOwnBot == null)
            {
                myOwnBot = ownBot;
            }
            myClosestEnemy = FindClosestEnemy(enemies);
            ITurnAction turnAction;
            if (IsBatteryLow(BatteryLowPercentage) && NotUnderAttack())
            {
                UpdateCurrentShieldAndHealth();
                return RechargeTheBattery();
            }
            if (IsInSafeDistanceFromEnemies(SafeDistanceFromEnemy))
            {
                if (ShieldNotFullEnough(ShieldNotFullPercentage))
                {
                    UpdateCurrentShieldAndHealth();
                    return RechargeTheShieldToMaximum();
                }
                if (BatteryNotFullEnough(BatteryNotFullPercentage))
                {
                    UpdateCurrentShieldAndHealth();
                    return RechargeTheBattery();
                }
            }
            if (IsShieldTooDamaged(ShieldDamagedPercentage))
            {
                UpdateCurrentShieldAndHealth();
                return HaveEnoughEnergyForFullShield() ? RechargeTheShieldToMaximum() : RechargeTheBattery();
            }
            if (IsUnderAttack() 
                && IsNotInCornerOrEdge(battlefield) 
                && (IsSeriouslyInjured(SeriouslyInjuredPercentage) || IsShieldSeriouslyDamaged()))
            {
                UpdateCurrentShieldAndHealth();
                return RunAwayFromEnemy(myClosestEnemy);
            }
            if (IsCloseEnoughForAttackTheEnemy(myClosestEnemy))
            {
                turnAction = Attack(myClosestEnemy);
            }
            else
            {
                turnAction = MoveCloserToEnemy(myClosestEnemy);
            }
            UpdateCurrentShieldAndHealth();
            return turnAction;
        }

        private bool IsInSafeDistanceFromEnemies(int distance)
        {
            return myOwnBot.DistanceTo(myClosestEnemy) > distance;
        }

        private bool NotUnderAttack()
        {
            return IsUnderAttack() == false;
        }

        private bool ShieldNotFullEnough(int shieldThreshold)
        {
            return myOwnBot.Shield.Percent < shieldThreshold;
        }

        private bool BatteryNotFullEnough(int batteryThreshold)
        {
            return myOwnBot.Energy.Percent < batteryThreshold;
        }

        private void UpdateCurrentShieldAndHealth()
        {
            myLastShieldPercent = myOwnBot.Shield.Percent;
            myLastHealthPercent = myOwnBot.Health.Percent;
        }

        private bool IsBatteryLow(int batteryThreshold)
        {
            return myOwnBot.Energy.Percent < batteryThreshold;
        }

        private static ITurnAction RechargeTheBattery()
        {
            return TurnAction.Recharge.Battery();
        }

        private bool IsShieldTooDamaged(int shieldDamagedThreshold)
        {
            return myOwnBot.Shield.Percent < shieldDamagedThreshold;
        }

        private bool HaveEnoughEnergyForFullShield()
        {
            int amountToMaxShieldPoints = myOwnBot.Shield.Maximum - myOwnBot.Shield.Actual;
            int energyLeftAfterRecharge = myOwnBot.Energy.Actual - amountToMaxShieldPoints;
            return energyLeftAfterRecharge > BatteryLowPercentage;
        }

        private ITurnAction RechargeTheShieldToMaximum()
        {
            int amountToMaxShieldPoints = myOwnBot.Shield.Maximum - myOwnBot.Shield.Actual;
            return TurnAction.Recharge.Shield(amountToMaxShieldPoints);
        }

        private IEnemy FindClosestEnemy(IReadOnlyCollection<IEnemy> enemies)
        {
            var distanceToClosestEnemy = 10000.0;
            var closestEnemy = enemies.First();
            foreach (var enemy in enemies)
            {
                var distanceToCurrentEnemy = myOwnBot.DistanceTo(enemy);
                if (distanceToCurrentEnemy < distanceToClosestEnemy)
                {
                    distanceToClosestEnemy = distanceToCurrentEnemy;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }

        private bool IsUnderAttack()
        {
            bool isHealthDecreasing = myOwnBot.Health.Percent < myLastHealthPercent;
            bool isShieldDecreasing = myOwnBot.Shield.Percent < myLastShieldPercent;
            return isHealthDecreasing || isShieldDecreasing;
        }

        private bool IsNotInCornerOrEdge(IBattlefieldView battlefield)
        {
            int ownBotPositionX = myOwnBot.Position.X;
            int ownBotPositionY = myOwnBot.Position.Y;
            return battlefield.IsOutOfRange(ownBotPositionX + 1, ownBotPositionY) == false
                   && battlefield.IsOutOfRange(ownBotPositionX - 1, ownBotPositionY) == false
                   && battlefield.IsOutOfRange(ownBotPositionX, ownBotPositionY + 1) == false
                   && battlefield.IsOutOfRange(ownBotPositionX, ownBotPositionY - 1) == false;
        }

        private bool IsShieldSeriouslyDamaged()
        {            
            return myOwnBot.Shield.Percent < ShieldSeriouslyDamagedPercentage;
        }

        private bool IsSeriouslyInjured(int seriouslyInjuredPercentage)
        {
            return myOwnBot.Health.Percent < seriouslyInjuredPercentage;
        }

        private ITurnAction RunAwayFromEnemy(IEnemy enemy)
        {
            return TurnAction.Move.AwayFrom(enemy.Position);
        }

        private static ITurnAction Attack(IEnemy enemy)
        {
            return TurnAction.Attack(enemy);
        }

        private bool IsCloseEnoughForAttackTheEnemy(IEnemy enemy)
        {
            return myOwnBot.DistanceTo(enemy) <= 4;
        }

        private static ITurnAction MoveCloserToEnemy(IEnemy enemy)
        {
            return TurnAction.Move.Towards(enemy.Position);
        }
    }
}