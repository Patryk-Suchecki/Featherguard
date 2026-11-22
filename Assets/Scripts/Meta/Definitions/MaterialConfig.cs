using UnityEngine;

namespace Featherguard.Meta
{
    [CreateAssetMenu(menuName = "Featherguard/Material Config")]
    public class MaterialConfig : ScriptableObject
    {
        [Tooltip("Every material, in the order the Storage lists them.")]
        public MaterialDefinition[] materials = new MaterialDefinition[0];

        [Header("Rewards of a won fight that rewards materials")]
        [Tooltip("How many materials a win gives, by activity type: Single, Waves, Puzzle, Elite, Boss.")]
        public int[] amountByType = { 2, 3, 2, 3, 4 };
        [Min(0f)] public float commonWeight = 70f;
        [Min(0f)] public float uncommonWeight = 25f;
        [Min(0f)] public float rareWeight = 5f;
        [Tooltip("Added to the Uncommon weight for every threat level above 1.")]
        [Min(0f)] public float uncommonPerThreat = 3f;
        [Tooltip("Added to the Rare weight for every threat level above 1.")]
        [Min(0f)] public float rarePerThreat = 1.5f;
        [Tooltip("Chance that a win also gives one universal Epic material.")]
        [Range(0f, 1f)] public float epicChance = 0.02f;
        [Tooltip("Chance that a win also gives one universal Legendary material.")]
        [Range(0f, 1f)] public float legendaryChance = 0.003f;
    }
}
