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
        private const int batteryLowPercentage = 10;
        private const int shieldDamagedPercentage = 1;

        public ITurnAction GetTurnAction(IOwnBot ownBot, IReadOnlyCollection<IEnemy> enemies, IBattlefieldView battlefield)
        {            
            ITurnAction turnAction;
            if (IsLowBattery(ownBot, batteryLowPercentage))
            {
                return RechargeTheBattery();
            }
            if (IsShieldTooDamaged(ownBot, shieldDamagedPercentage) && HaveEnoughEnergy(ownBot))
            {
                return RechargeTheShieldToMaximum(ownBot);
            }
            var closestEnemy = FindClosestEnemy(ownBot, enemies);
            if (IsUnderAttack(ownBot) && IsNotInCornerOrEdge(battlefield, ownBot) && IsInjured(ownBot))
            {
                return MoveAwayFromEnemy(ownBot, closestEnemy);
            }
            if (IsCloseEnoughForAttackTheEnemy(ownBot, closestEnemy))
            {
                turnAction = Attack(closestEnemy);
            }
            else
            {
                turnAction = MoveCloserToEnemy(ownBot, closestEnemy);
            }
            return turnAction;
        }

        private static bool IsLowBattery(IOwnBot ownBot, int batteryThreshold)
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

        private bool HaveEnoughEnergy(IOwnBot ownBot)
        {
            int amountToMaxShieldPoints = ownBot.Shield.Maximum - ownBot.Shield.Actual;
            int energyLeftAfterRecharge = ownBot.Energy.Actual - amountToMaxShieldPoints;
            return energyLeftAfterRecharge > batteryLowPercentage;
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
            return ownBot.Shield.Percent < 10;
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

        private bool IsInjured(IOwnBot ownBot)
        {
            return ownBot.Health.Percent < 30;
        }

        private ITurnAction MoveAwayFromEnemy(IOwnBot ownBot, IEnemy enemy)
        {
            if (ownBot.Position.X > enemy.Position.X)
            {
                return TurnAction.Move.East();
            }
            if (ownBot.Position.X < enemy.Position.X)
            {
                return TurnAction.Move.West();
            }
            if (ownBot.Position.Y > enemy.Position.Y)
            {
                return TurnAction.Move.North();
            }
            if (ownBot.Position.Y < enemy.Position.Y)
            {
                return TurnAction.Move.South();
            }
            return TurnAction.Move.East();
        }

        private static ITurnAction Attack(IEnemy closestEnemy)
        {
            return TurnAction.Attack(closestEnemy);
        }

        private bool IsCloseEnoughForAttackTheEnemy(IOwnBot ownBot, IEnemy enemy)
        {
            return ownBot.DistanceTo(enemy) <= 4;
        }

        private static ITurnAction MoveCloserToEnemy(IOwnBot ownBot, IEnemy closestEnemy)
        {
            return TurnAction.Move.Towards(ownBot.Position, closestEnemy.Position);
        }
    }
}
