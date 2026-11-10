using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle
{
    public static class Targeting
    {
        public static IEnumerable<BattleUnit> Candidates(AbilityDefinition ability, Squad allies, Squad opponents)
        {
            if (!ability.TargetsEnemies) return allies.Living;
            if (ability.TargetsAll) return opponents.Living;
            var taunting = opponents.Taunting.ToList();
            return taunting.Count > 0 ? taunting : opponents.Living;
        }

        public static bool IsValid(AbilityDefinition ability, BattleUnit target, Squad allies, Squad opponents) =>
            target != null && target.IsAlive && Candidates(ability, allies, opponents).Contains(target);

        public static bool IsBlockedByTaunt(AbilityDefinition ability, BattleUnit target, Squad opponents) =>
            ability.TargetsEnemies && !ability.TargetsAll && target != null && target.IsAlive && target.Team == opponents.Team
            && opponents.Taunting.Any() && !target.ForcesTargeting;

        public static List<BattleUnit> Resolve(AbilityDefinition ability, BattleUnit chosen, Squad allies, Squad opponents)
        {
            if (ability.TargetsAll)
                return (ability.TargetsEnemies ? opponents : allies).Living.ToList();
            return chosen != null && chosen.IsAlive ? new List<BattleUnit> { chosen } : new List<BattleUnit>();
        }
    }
}
