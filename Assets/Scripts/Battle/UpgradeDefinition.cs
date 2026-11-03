using UnityEngine;

namespace Featherguard.Battle
{
    public enum Rarity { Common, Rare, Epic, Legendary }
    public enum UpgradeScope { Hero, Party, Run }
    public enum RunBonus { None, Luck, GoldGain }

    [CreateAssetMenu(menuName = "Featherguard/Upgrade")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Rarity rarity;
        public UpgradeScope scope;
        public bool curse;
        public StatModifier[] modifiers = new StatModifier[0];
        public RunBonus runBonus;
        public float runAmount;

        public static Color Tint(Rarity rarity) => rarity switch
        {
            Rarity.Rare => new Color(0.25f, 0.55f, 1f),
            Rarity.Epic => new Color(0.7f, 0.3f, 1f),
            Rarity.Legendary => new Color(1f, 0.7f, 0.1f),
            _ => new Color(0.6f, 0.65f, 0.7f),
        };

        public static string Label(Rarity rarity) => rarity switch
        {
            Rarity.Rare => "RZADKA",
            Rarity.Epic => "EPICKA",
            Rarity.Legendary => "LEGENDARNA",
            _ => "ZWYKŁA",
        };
    }
}
