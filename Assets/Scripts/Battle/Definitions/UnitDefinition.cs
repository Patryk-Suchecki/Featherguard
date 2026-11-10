using System;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum Team { Heroes, Enemies }

    [Serializable]
    public class EnemyAiProfile
    {
        [Tooltip("Actions in every enemy turn (Elites: 2).")]
        [Min(1)] public int actionsPerTurn = 1;
        [Tooltip("Use the special ability every N turns (0 = never). With several actions it replaces the last one.")]
        [Min(0)] public int specialEvery;
        [Range(0f, 1f)] public float supportChance = 0.45f;
        [Tooltip("Allies below this health fraction are healing candidates.")]
        [Range(0f, 1f)] public float healThreshold = 0.6f;
        [Range(0f, 1f)] public float healChance = 0.75f;
        [Tooltip("Extra targeting weight for wounded heroes.")]
        public float woundedWeight = 2f;
        [Tooltip("Extra targeting weight for heroes the attack would finish.")]
        public float finishWeight = 3f;
    }

    [Serializable]
    public class BossProfile
    {
        [Tooltip("Health fraction that triggers the enrage phase (0 = never).")]
        [Range(0f, 1f)] public float enrageAt;
        public float enrageDamageBonus = 0.4f;
        public UnitDefinition[] enrageSummons = new UnitDefinition[0];
    }

    [CreateAssetMenu(menuName = "Featherguard/Unit")]
    public class UnitDefinition : ScriptableObject
    {
        public string displayName;
        public Team team;
        [Min(1)] public int maxHealth = 100;
        public AbilityDefinition attack;
        public AbilityDefinition support;
        public AbilityDefinition special;
        public EnemyAiProfile ai = new();
        public BossProfile boss = new();
    }
}
