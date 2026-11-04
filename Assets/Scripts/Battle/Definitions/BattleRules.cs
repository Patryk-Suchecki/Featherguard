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

        [Header("Damage")]
        [Range(0f, 1f)] public float baseCritChance = 0.15f;
        public float baseCritDamage = 1.5f;
        [Range(0f, 0.5f)] public float damageVariance = 0.1f;

        [Header("Formation")]
        [Min(1)] public int heroSlotCount = 3;
        [Min(1)] public int enemySlotCount = 3;
    }
}
