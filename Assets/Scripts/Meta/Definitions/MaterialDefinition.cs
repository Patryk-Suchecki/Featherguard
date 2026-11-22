using UnityEngine;

namespace Featherguard.Meta
{
    public enum MaterialRarity { Common, Uncommon, Rare, Epic, Legendary }

    [CreateAssetMenu(menuName = "Featherguard/Material")]
    public class MaterialDefinition : ScriptableObject
    {
        public string displayName;
        public MaterialRarity rarity;
        [Tooltip("The land whose fights drop it; empty for the universal Epic and Legendary materials.")]
        public LandDefinition land;
    }
}
