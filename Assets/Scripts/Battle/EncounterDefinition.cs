using System;
using UnityEngine;

namespace Featherguard.Battle
{
    [CreateAssetMenu(menuName = "Featherguard/Encounter")]
    public class EncounterDefinition : ScriptableObject
    {
        public string displayName;
        public bool boss;
        public float enemyHealth = 1f;
        public float enemyDamage = 1f;
        public int goldReward = 10;
        public Wave[] waves;

        [Serializable]
        public class Wave
        {
            public UnitDefinition[] enemies;
        }
    }
}
