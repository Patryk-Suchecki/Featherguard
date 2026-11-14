using System.Collections.Generic;

namespace Featherguard.Battle
{
    public abstract class BattleEvent { }

    public sealed class UnitSpawned : BattleEvent
    {
        public BattleUnit Unit;
        public UnitSnapshot State;
        public bool Summoned;
    }

    public sealed class UnitRemoved : BattleEvent
    {
        public BattleUnit Unit;
    }

    public sealed class UnitChanged : BattleEvent
    {
        public BattleUnit Unit;
        public UnitSnapshot State;
    }

    public sealed class WaveStarted : BattleEvent
    {
        public EncounterDefinition Encounter;
        public int Wave;
        public int Waves;
        public int EncounterNumber;
        public int EncounterCount;
        public bool Elite;
    }

    public sealed class WaveSpawned : BattleEvent { }

    public sealed class TurnStarted : BattleEvent
    {
        public Team Team;
    }

    public sealed class EnemyWindup : BattleEvent
    {
        public BattleUnit Actor;
        public AbilityDefinition Ability;
        public BattleUnit Target;
    }

    public sealed class ActionPerformed : BattleEvent
    {
        public BattleUnit Actor;
        public AbilityDefinition Ability;
        public IReadOnlyList<BattleUnit> Targets;
        public bool Special;
        public ActionTiming Timing;
        public readonly List<Impact> Impacts = new();
        public readonly List<BattleEvent> Aftermath = new();
    }

    public sealed class Impact
    {
        public int Hit;
        public BattleUnit Target;
        public readonly List<BattleEvent> Outcomes = new();
    }

    public sealed class Damaged : BattleEvent
    {
        public BattleUnit Source;
        public BattleUnit Target;
        public AbilityDefinition Ability;
        public int Amount;
        public int Absorbed;
        public bool Critical;
    }

    public sealed class Healed : BattleEvent
    {
        public BattleUnit Target;
        public AbilityDefinition Ability;
        public int Amount;
    }

    public sealed class StatusApplied : BattleEvent
    {
        public BattleUnit Target;
        public StatusDefinition Status;
    }

    public sealed class StatusTicked : BattleEvent
    {
        public BattleUnit Unit;
        public StatusDefinition Status;
        public int Amount;
        public int Absorbed;
    }

    public sealed class Cleansed : BattleEvent
    {
        public BattleUnit Unit;
    }

    public sealed class Stunned : BattleEvent
    {
        public BattleUnit Unit;
    }

    public sealed class Died : BattleEvent
    {
        public BattleUnit Unit;
    }

    public sealed class Enraged : BattleEvent
    {
        public BattleUnit Boss;
    }

    public sealed class RageChanged : BattleEvent
    {
        public float Value;
        public float Max;
        public bool BecameReady;
    }

    public sealed class BattleEnded : BattleEvent
    {
        public bool Victory;
    }
}
