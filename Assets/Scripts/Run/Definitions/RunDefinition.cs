using System;
using Featherguard.Battle;
using UnityEngine;

namespace Featherguard.Run
{
    public enum PathNodeType { Normal, Elite, Shop, Chest, Campfire }

    [Serializable]
    public class PathNode
    {
        public PathNodeType type;
        public string title;
        [TextArea] public string description;
        public Sprite icon;
        public Color color = Color.gray;
        public float weight = 1f;
    }

    [CreateAssetMenu(menuName = "Featherguard/Run")]
    public class RunDefinition : ScriptableObject
    {
        public EncounterDefinition[] encounters;
        public UpgradeDefinition[] upgrades;
        public UpgradeDefinition[] curses;
        public EconomyConfig economy;
        public SlotConfig slots;
        public RarityTable rarity = new();
        [Min(1)] public int pathChoices = 2;
        public PathNode[] pathNodes = new PathNode[0];
        public PathNode[] beforeBossNodes = new PathNode[0];
    }
}
