using System.Collections.Generic;

namespace Featherguard.Battle
{
    public sealed class HeroLoadout
    {
        public UnitDefinition Definition;
        public int? Health;
        public IReadOnlyList<StatModifier> Modifiers = System.Array.Empty<StatModifier>();
    }

    public sealed class BattleSetup
    {
        public EncounterDefinition Encounter;
        public IReadOnlyList<HeroLoadout> Heroes;
        public float StartRage;
        public float EnemyHealthScale = 1f;
        public float EnemyDamageScale = 1f;
        public int EncounterNumber = 1;
        public int EncounterCount = 1;
        public bool Elite;
        public bool AwaitBlocks;
    }

    public sealed class BattleResult
    {
        public bool Victory;
        public IReadOnlyList<BattleUnit> Heroes;
    }
}
