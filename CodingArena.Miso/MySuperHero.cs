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
        public string BotName => "Miso";
        private IOwnBot myOwnBot;
        private const int BatteryLowPercentage = 20;
        private const int ShieldDamagedPercentage = 10;
        private int myLastShieldPercent = 100;
        private int myLastHealthPercent = 100;
        private int batteryNotFullPercentage = 100;
        private IEnemy myClosestEnemy;
        private const int SafeDistanceFromEnemy = 5;

        public ITurnAction GetTurnAction(IOwnBot ownBot, IReadOnlyCollection<IEnemy> enemies, IBattlefieldView battlefield)
        {
            if (myOwnBot == null)
            {
                myOwnBot = ownBot;
            }
            myClosestEnemy = FindClosestEnemy(ownBot, enemies);
            ITurnAction turnAction;
            if (IsBatteryLow(ownBot, BatteryLowPercentage) && NotUnderAttack(ownBot))
            {
                UpdateCurrentShieldAndHealth(ownBot);
                return RechargeTheBattery();
            }
            if (IsInSafeDistanceFromEnemies(SafeDistanceFromEnemy) && BatteryNotFullEnough(ownBot, batteryNotFullPercentage))
            {
                UpdateCurrentShieldAndHealth(ownBot);
                return RechargeTheBattery();
            }
            if (IsShieldTooDamaged(ownBot, ShieldDamagedPercentage))
            {
                UpdateCurrentShieldAndHealth(ownBot);
                return HaveEnoughEnergyForFullShield(ownBot) ? RechargeTheShieldToMaximum(ownBot) : RechargeTheBattery();
            }
            if (IsUnderAttack(ownBot) 
                && IsNotInCornerOrEdge(battlefield, ownBot) 
                && (IsSeriouslyInjured(ownBot) || IsShieldSeriouslyDamaged(ownBot)))
            {
                UpdateCurrentShieldAndHealth(ownBot);
                return RunAwayFromEnemy(myClosestEnemy);
            }
            if (IsCloseEnoughForAttackTheEnemy(ownBot, myClosestEnemy))
            {
                turnAction = Attack(myClosestEnemy);
            }
            else
            {
                turnAction = MoveCloserToEnemy(myClosestEnemy);
            }
            UpdateCurrentShieldAndHealth(ownBot);
            return turnAction;
        }

        private bool IsInSafeDistanceFromEnemies(int distance)
        {
            return myOwnBot.DistanceTo(myClosestEnemy) > distance;
        }

        private bool NotUnderAttack(IOwnBot ownBot)
        {
            return IsUnderAttack(ownBot) == false;
        }

        private bool BatteryNotFullEnough(IOwnBot ownBot, int batteryThreshold)
        {
            return ownBot.Energy.Percent < batteryThreshold;
        }

        private void UpdateCurrentShieldAndHealth(IOwnBot ownBot)
        {
            myLastShieldPercent = ownBot.Shield.Percent;
            myLastHealthPercent = ownBot.Health.Percent;
        }

        private static bool IsBatteryLow(IOwnBot ownBot, int batteryThreshold)
        {
            return ownBot.Energy.Percent < batteryThreshold;
        }

        private static ITurnAction RechargeTheBattery()
        {
            return TurnAction.Recharge.Battery();
        }

        private static bool IsShieldTooDamaged(IOwnBot ownBot, int shieldDamagedThreshold)
        {
            return ownBot.Shield.Percent < shieldDamagedThreshold;
        }

        private bool HaveEnoughEnergyForFullShield(IOwnBot ownBot)
        {
            int amountToMaxShieldPoints = ownBot.Shield.Maximum - ownBot.Shield.Actual;
            int energyLeftAfterRecharge = ownBot.Energy.Actual - amountToMaxShieldPoints;
            return energyLeftAfterRecharge > BatteryLowPercentage;
        }

        private static ITurnAction RechargeTheShieldToMaximum(IOwnBot ownBot)
        {
            int amountToMaxShieldPoints = ownBot.Shield.Maximum - ownBot.Shield.Actual;
            return TurnAction.Recharge.Shield(amountToMaxShieldPoints);
        }

        private IEnemy FindClosestEnemy(IOwnBot ownBot, IReadOnlyCollection<IEnemy> enemies)
        {
            var distanceToClosestEnemy = 10000.0;
            var closestEnemy = enemies.First();
            foreach (var enemy in enemies)
            {
                var distanceToCurrentEnemy = ownBot.DistanceTo(enemy);
                if (distanceToCurrentEnemy < distanceToClosestEnemy)
                {
                    distanceToClosestEnemy = distanceToCurrentEnemy;
                    closestEnemy = enemy;
                }
            }
            return closestEnemy;
        }

        private bool IsUnderAttack(IOwnBot ownBot)
        {
            bool isHealthDecreasing = ownBot.Health.Percent < myLastHealthPercent;
            bool isShieldDecreasing = ownBot.Shield.Percent < myLastShieldPercent;
            return isHealthDecreasing || isShieldDecreasing;
        }

        private bool IsNotInCornerOrEdge(IBattlefieldView battlefield, IOwnBot ownBot)
        {
            int ownBotPositionX = ownBot.Position.X;
            int ownBotPositionY = ownBot.Position.Y;
            return battlefield.IsOutOfRange(ownBotPositionX + 1, ownBotPositionY) == false
                   && battlefield.IsOutOfRange(ownBotPositionX - 1, ownBotPositionY) == false
                   && battlefield.IsOutOfRange(ownBotPositionX, ownBotPositionY + 1) == false
                   && battlefield.IsOutOfRange(ownBotPositionX, ownBotPositionY - 1) == false;
        }

        private bool IsShieldSeriouslyDamaged(IOwnBot ownBot)
        {
            return ownBot.Shield.Percent < 10;
        }

        private bool IsSeriouslyInjured(IOwnBot ownBot)
        {
            return ownBot.Health.Percent < 30;
        }

        private ITurnAction RunAwayFromEnemy(IEnemy enemy)
        {
            return TurnAction.Move.AwayFrom(enemy.Position);
        }

        private static ITurnAction Attack(IEnemy closestEnemy)
        {
            return TurnAction.Attack(closestEnemy);
        }

        private bool IsCloseEnoughForAttackTheEnemy(IOwnBot ownBot, IEnemy enemy)
        {
            return ownBot.DistanceTo(enemy) <= 4;
        }

        private static ITurnAction MoveCloserToEnemy(IEnemy enemy)
        {
            return TurnAction.Move.Towards(enemy.Position);
        }
    }
}