using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Battle Rules")]
    public class BattleRules : ScriptableObject
    {
        [Header("Rage")]
        public float maxRage = 100f;
        public float heroRagePerDamage = 0.45f;
        public float enemyRagePerDamage = 0.25f;

        [Header("Timing")]
        [Tooltip("Extra damage for a good attack timing.")]
        [Min(0f)] public float goodAttackBonus = 0.25f;
        [Tooltip("Extra damage for a perfect attack timing.")]
        [Min(0f)] public float perfectAttackBonus = 0.5f;
        [Tooltip("Damage removed by a good block.")]
        [Range(0f, 1f)] public float goodBlockReduction = 0.3f;
        [Tooltip("Damage removed by a perfect block.")]
        [Range(0f, 1f)] public float perfectBlockReduction = 0.6f;
        [Tooltip("Rage added once for an action with a perfect attack or block.")]
        [Min(0f)] public float perfectTimingRage = 10f;

        [Header("Damage")]
        [Range(0f, 1f)] public float baseCritChance = 0.15f;
        public float baseCritDamage = 1.5f;
        [Range(0f, 0.5f)] public float damageVariance = 0.1f;

        [Header("Formation")]
        [Min(1)] public int heroSlotCount = 3;
        [Min(1)] public int enemySlotCount = 3;
    }
}
