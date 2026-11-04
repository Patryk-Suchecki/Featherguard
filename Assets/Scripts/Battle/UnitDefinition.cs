using UnityEngine;

namespace Featherguard.Battle
{
    public enum Team { Heroes, Enemies }

    [CreateAssetMenu(menuName = "Featherguard/Unit")]
    public class UnitDefinition : ScriptableObject
    {
        public string displayName;
        public Team team;
        public GameObject model;
        public Sprite portrait;
        public RuntimeAnimatorController animator;
        public float modelScale = 1f;
        public float height = 1.4f;
        public int maxHealth = 100;
        public AbilityDefinition attack;
        public AbilityDefinition support;
        public AbilityDefinition special;
        public int specialEvery;
        [Range(0f, 1f)] public float enrageAt;
        public UnitDefinition[] enrageSummons = new UnitDefinition[0];
    }
}
