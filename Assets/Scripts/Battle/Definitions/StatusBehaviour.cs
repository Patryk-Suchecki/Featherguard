using System;
using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle
{
    [Serializable]
    public abstract class StatusBehaviour
    {
        public virtual IEnumerable<StatModifier> Modifiers => Array.Empty<StatModifier>();
        public virtual bool DealsDamageOverTime => false;
        public virtual bool SkipsTurn => false;
        public virtual bool ForcesTargeting => false;
        public virtual bool AbsorbsDamage => false;
        public virtual bool ActsLast => false;
        public virtual bool ConsumedOnAttack => false;
        public virtual bool ConsumedByCriticalHit => false;
        public virtual bool Retaliates => false;
        public virtual IEnumerable<StatusApplication> RetaliationStatuses => Array.Empty<StatusApplication>();
        public virtual bool Blocks(StatusDefinition status) => false;
    }

    [Serializable]
    public sealed class BlockStatuses : StatusBehaviour
    {
        [Tooltip("Blocks every harmful status.")]
        public bool harmful;
        [Tooltip("Blocks these statuses.")]
        public StatusDefinition[] statuses = new StatusDefinition[0];

        public override bool Blocks(StatusDefinition status) => status != null && (harmful && status.harmful || Array.IndexOf(statuses, status) >= 0);
    }

    [Serializable]
    public sealed class Retaliate : StatusBehaviour
    {
        [Tooltip("Optional status put on each attacker, for example a chance to Stun.")]
        public StatusApplication onAttacker = new();

        public override bool Retaliates => true;
        public override IEnumerable<StatusApplication> RetaliationStatuses =>
            onAttacker != null && onAttacker.status != null ? new[] { onAttacker } : Array.Empty<StatusApplication>();
    }

    [Serializable]
    public sealed class ModifyStats : StatusBehaviour
    {
        [Tooltip("Applied to the owner's stats while the status is on.")]
        public StatModifier[] modifiers = new StatModifier[0];

        public override IEnumerable<StatModifier> Modifiers => modifiers;
    }

    [Serializable]
    public sealed class DamageOverTime : StatusBehaviour
    {
        public override bool DealsDamageOverTime => true;
    }

    [Serializable]
    public sealed class SkipTurn : StatusBehaviour
    {
        public override bool SkipsTurn => true;
    }

    [Serializable]
    public sealed class ForceTargeting : StatusBehaviour
    {
        public override bool ForcesTargeting => true;
    }

    [Serializable]
    public sealed class AbsorbDamage : StatusBehaviour
    {
        public override bool AbsorbsDamage => true;
    }

    [Serializable]
    public sealed class ActLast : StatusBehaviour
    {
        public override bool ActsLast => true;
    }

    [Serializable]
    public sealed class ConsumeOnAttack : StatusBehaviour
    {
        public override bool ConsumedOnAttack => true;
    }

    [Serializable]
    public sealed class ConsumeOnCriticalHit : StatusBehaviour
    {
        public override bool ConsumedByCriticalHit => true;
    }
}
