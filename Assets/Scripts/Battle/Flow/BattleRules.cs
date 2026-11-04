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
        public float lightKnockback = 0.22f;
        public float heavyKnockback = 0.4f;

        [Header("Formation")]
        public Vector3[] heroSlots = { new(-2.3f, 0f, 0.45f), new(-3.7f, 0f, -0.55f), new(-5.0f, 0f, 0.6f) };
        public Vector3[] enemySlots = { new(2.3f, 0f, 0.45f), new(3.7f, 0f, -0.55f), new(5.0f, 0f, 0.6f) };
        public float spawnOffset = 6f;
        public float heroYaw = 115f;

        [Header("Timing")]
        public float enemyTurnDelay = 0.8f;
        public float betweenEnemyActions = 0.2f;
        public float afterAction = 0.35f;
        public float specialWindup = 0.5f;
        public float waveDelay = 0.6f;
        public float spawnInterval = 0.18f;
        public float afterSpawn = 0.8f;

        [Header("Selection")]
        public Color selectedRing = new(1f, 0.85f, 0.2f, 1f);
        public Color enemyRing = new(1f, 0.3f, 0.25f, 0.75f);
        public Color allyRing = new(0.35f, 1f, 0.5f, 0.7f);
        [Tooltip("Tap tolerance around units as a fraction of screen height.")]
        [Range(0f, 0.3f)] public float tapTolerance = 0.12f;
    }
}
