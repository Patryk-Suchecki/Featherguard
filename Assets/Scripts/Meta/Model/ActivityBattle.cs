using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;

namespace Featherguard.Meta
{
    public static class ActivityBattle
    {
        public static BattleSetup CreateSetup(ActivityDefinition activity, IEnumerable<UnitDefinition> party) =>
            CreateSetup(activity.encounter, party, activity.type == ActivityType.EliteFight);

        public static BattleSetup CreateSetup(EncounterDefinition encounter, IEnumerable<UnitDefinition> party, bool elite = false) => new()
        {
            Encounter = encounter,
            Heroes = party.Select(hero => new HeroLoadout { Definition = hero }).ToList(),
            EnemyHealthScale = encounter.enemyHealth,
            EnemyDamageScale = encounter.enemyDamage,
            Elite = elite,
        };

        public static int Waves(ActivityDefinition activity) => activity.encounter != null && activity.encounter.waves != null ? activity.encounter.waves.Length : 0;

        public static int Gold(ActivityDefinition activity, float daringMultiplier) =>
            activity.encounter != null ? (int)(activity.encounter.goldReward * daringMultiplier) : 0;
    }
}
