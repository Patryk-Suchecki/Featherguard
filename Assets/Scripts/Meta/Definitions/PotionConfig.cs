using UnityEngine;

namespace Featherguard.Meta
{
    [CreateAssetMenu(menuName = "Featherguard/Potion Config")]
    public class PotionConfig : ScriptableObject
    {
        [Tooltip("Every recipe, in the order the Wise Woman's Hut lists them.")]
        public RecipeDefinition[] recipes = new RecipeDefinition[0];
        [Tooltip("How many potions the party can take into one battle.")]
        [Min(0)] public int battleSlots = 3;
        [Tooltip("The most potions brewed at once.")]
        [Min(1)] public int maxBrew = 10;
    }
}
