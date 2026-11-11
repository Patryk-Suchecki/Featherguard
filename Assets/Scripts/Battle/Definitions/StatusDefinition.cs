using System;
using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Status")]
    public class StatusDefinition : ScriptableObject
    {
        public string displayName;
        public string announcement;
        [Tooltip("Applied to the unit's stats while the status is active.")]
        public StatModifier[] modifiers = new StatModifier[0];
        [Tooltip("Deals the application's potency as damage at the start of the owner's turn.")]
        public bool damagePerTurn;
        [Tooltip("The owner skips its turn.")]
        public bool skipsTurn;
        [Tooltip("Opponents' single-target abilities must target the owner.")]
        public bool forcesTargeting;
        [Tooltip("Removed after the owner uses a damaging ability.")]
        public bool consumedOnAttack;
    }

    [Serializable]
    public class StatusApplication
    {
        public StatusDefinition status;
        [Min(1)] public int turns = 1;
        [Tooltip("Damage per turn for damage-over-time statuses.")]
        [Min(0)] public int potency;
        [Range(0f, 1f)] public float chance = 1f;
        public bool onSelf;
    }
}
