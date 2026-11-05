using System;
using Featherguard.Core;
using UnityEngine;

namespace Featherguard.Run
{
    [Serializable]
    public class SlotSymbol
    {
        public string id;
        public string label;
        public Sprite icon;
        public Color tint = Color.white;
        public float weight = 10f;
        public float weightPerLuck;
        public float minWeight;
        public RunUnlock requires;
        public bool megaJackpot;
        [SerializeReference, SubclassSelector] public SlotReward pair;
        [SerializeReference, SubclassSelector] public SlotReward triple;

        public bool IsAvailable(RunSession session) => requires == RunUnlock.None || session.Has(requires);
        public float WeightFor(int luck) => Mathf.Max(minWeight, weight + weightPerLuck * luck);
    }

    [CreateAssetMenu(menuName = "Featherguard/Slot Machine")]
    public class SlotConfig : ScriptableObject
    {
        public SlotSymbol[] symbols = new SlotSymbol[0];
        [Tooltip("Chance that a reel copies the previous one.")]
        [Range(0f, 1f)] public float matchChance = 0.3f;
        public float matchChancePerLuck = 0.04f;
        [Range(0f, 1f)] public float maxMatchChance = 0.6f;
        [SerializeReference, SubclassSelector] public SlotReward noMatch = new GoldReward { amount = 5, text = "Nothing... +5 gold" };
        public string jackpotPrefix = "JACKPOT! ";
        public string megaJackpotPrefix = "MEGA JACKPOT! ";

        public float MatchChance(int luck) => Mathf.Min(maxMatchChance, matchChance + luck * matchChancePerLuck);
    }
}
