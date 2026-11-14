using System;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum StatusStacking { Refresh, Stack }

    public enum StatusCondition { Always, PerfectTiming }

    [CreateAssetMenu(menuName = "Featherguard/Status")]
    public class StatusDefinition : ScriptableObject
    {
        public string displayName;
        public string announcement;
        [Tooltip("Removed by a cleanse.")]
        public bool harmful;
        [Tooltip("Refresh keeps one instance; Stack adds a stack up to the maximum on each application.")]
        public StatusStacking stacking;
        [Min(1)] public int maxStacks = 1;
        [SerializeReference, SubclassSelector] public StatusBehaviour[] behaviours = new StatusBehaviour[0];

        public IEnumerable<StatModifier> Modifiers => behaviours.Where(b => b != null).SelectMany(b => b.Modifiers);
        public bool DealsDamageOverTime => Any(b => b.DealsDamageOverTime);
        public bool SkipsTurn => Any(b => b.SkipsTurn);
        public bool ForcesTargeting => Any(b => b.ForcesTargeting);
        public bool AbsorbsDamage => Any(b => b.AbsorbsDamage);
        public bool ActsLast => Any(b => b.ActsLast);
        public bool ConsumedOnAttack => Any(b => b.ConsumedOnAttack);
        public bool ConsumedByCriticalHit => Any(b => b.ConsumedByCriticalHit);
        public int StackLimit => stacking == StatusStacking.Stack ? Math.Max(1, maxStacks) : 1;

        bool Any(Func<StatusBehaviour, bool> predicate) => behaviours.Any(b => b != null && predicate(b));
    }

    [Serializable]
    public class StatusApplication
    {
        public StatusDefinition status;
        [Min(1)] public int turns = 1;
        [Tooltip("Damage per turn for damage over time; base points for an absorbing Shield.")]
        [Min(0)] public int potency;
        [Range(0f, 1f)] public float chance = 1f;
        public bool onSelf;
        [Tooltip("PerfectTiming: only applied when the action's attack timing is Perfect.")]
        public StatusCondition condition;
    }
}
