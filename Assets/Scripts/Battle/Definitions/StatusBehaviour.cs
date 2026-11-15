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
