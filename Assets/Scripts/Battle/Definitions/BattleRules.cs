using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Battle Rules")]
    public class BattleRules : ScriptableObject
    {
        [Header("Rage")]
        [Min(1f)] public float maxRage = 100f;
        [Tooltip("Rage gained per point of damage a hero deals.")]
        [Min(0f)] public float heroRagePerDamage = 0.45f;
        [Tooltip("Rage gained per point of damage an enemy deals.")]
        [Min(0f)] public float enemyRagePerDamage = 0.25f;

        [Header("Damage")]
        [Range(0f, 1f)] public float baseCritChance = 0.15f;
        [Min(1f)] public float baseCritDamage = 1.5f;
        [Range(0f, 0.5f)] public float damageVariance = 0.1f;

        [Header("Formation")]
        [Min(1)] public int heroSlotCount = 3;
        [Min(1)] public int enemySlotCount = 3;
    }
}
