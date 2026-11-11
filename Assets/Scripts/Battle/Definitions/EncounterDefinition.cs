using System;
using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Encounter")]
    public class EncounterDefinition : ScriptableObject
    {
        public string displayName;
        public bool boss;
        [Tooltip("Multiplies every enemy's health.")]
        [Min(0.1f)] public float enemyHealth = 1f;
        [Tooltip("Multiplies every enemy's damage.")]
        [Min(0f)] public float enemyDamage = 1f;
        [Min(0)] public int goldReward = 10;
        public Wave[] waves;

        [Serializable]
        public class Wave
        {
            public UnitDefinition[] enemies;
        }
    }
}
