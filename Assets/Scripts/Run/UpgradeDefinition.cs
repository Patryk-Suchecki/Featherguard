using Featherguard.Battle;
using UnityEngine;

namespace Featherguard.Run
{
    public enum Rarity { Common, Rare, Epic, Legendary }
    public enum UpgradeScope { Hero, Party, Run }
    public enum RunBonus { None, Luck, GoldGain }

    [CreateAssetMenu(menuName = "Featherguard/Upgrade")]
    public class UpgradeDefinition : ScriptableObject
    {
        public string displayName;
        [TextArea] public string description;
        public Sprite icon;
        public Rarity rarity;
        public UpgradeScope scope;
        public bool curse;
        public StatModifier[] modifiers = new StatModifier[0];
        public RunBonus runBonus;
        public float runAmount;
    }
}
