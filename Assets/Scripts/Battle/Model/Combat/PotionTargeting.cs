using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle
{
    public static class PotionTargeting
    {
        public static IEnumerable<BattleUnit> Candidates(PotionDefinition potion, Squad heroes, Squad enemies) => potion.target switch
        {
            PotionTarget.Hero => heroes.Living,
            PotionTarget.FallenHero => heroes.Where(h => !h.IsAlive),
            PotionTarget.AllHeroes => potion.TargetsFallen ? heroes : heroes.Living,
            _ => enemies.Living,
        };

        public static bool IsValid(PotionDefinition potion, BattleUnit target, Squad heroes, Squad enemies) =>
            target != null && Candidates(potion, heroes, enemies).Contains(target);

        public static List<BattleUnit> Resolve(PotionDefinition potion, BattleUnit chosen, Squad heroes, Squad enemies) =>
            potion.TargetsAll ? Candidates(potion, heroes, enemies).ToList() : new List<BattleUnit> { chosen };
    }
}
