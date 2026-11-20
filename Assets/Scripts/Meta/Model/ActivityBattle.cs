using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;

namespace Featherguard.Meta
{
    public static class ActivityBattle
    {
        public static BattleSetup CreateSetup(ActivityDefinition activity, IEnumerable<UnitDefinition> party) => new()
        {
            Encounter = activity.encounter,
            Heroes = party.Select(hero => new HeroLoadout { Definition = hero }).ToList(),
            EnemyHealthScale = activity.encounter.enemyHealth,
            EnemyDamageScale = activity.encounter.enemyDamage,
            Elite = activity.type == ActivityType.EliteFight,
        };

        public static int Waves(ActivityDefinition activity) => activity.encounter != null && activity.encounter.waves != null ? activity.encounter.waves.Length : 0;

        public static int Gold(ActivityDefinition activity, float daringMultiplier) =>
            activity.encounter != null ? (int)(activity.encounter.goldReward * daringMultiplier) : 0;
    }
}
