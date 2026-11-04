using UnityEngine;

namespace Featherguard.Run
{
    [CreateAssetMenu(menuName = "Featherguard/Economy")]
    public class EconomyConfig : ScriptableObject
    {
        [Header("Battle")]
        public int goldPerKill = 3;
        [Range(0f, 1f)] public float restHeal = 0.2f;
        [Range(0f, 1f)] public float reviveHealth = 0.3f;

        [Header("Elite")]
        public float eliteStrength = 1.35f;
        public int eliteGoldMultiplier = 2;
        public Rarity eliteCardRarity = Rarity.Rare;

        [Header("Cards")]
        [Min(1)] public int cardChoices = 3;
        public int rerollBaseCost = 10;
        public int rerollCostStep = 5;

        [Header("Shop")]
        [Min(1)] public int shopOffers = 3;
        public int[] goldPrices = { 25, 45, 80, 130 };
        public float[] bloodPrices = { 0.1f, 0.18f, 0.28f, 0.4f };
        public int shopHealCost = 20;
        [Range(0f, 1f)] public float shopHealFraction = 0.3f;

        [Header("Path")]
        [Range(0f, 1f)] public float campfireHeal = 0.35f;
        public Rarity chestRarity = Rarity.Epic;

        [Header("Double or nothing")]
        [Range(0f, 1f)] public float gambleBaseChance = 0.5f;
        public float gambleChancePerLuck = 0.03f;
        [Range(0f, 1f)] public float gambleMaxChance = 0.65f;

        public int GoldPrice(Rarity rarity) => goldPrices[Mathf.Clamp((int)rarity, 0, goldPrices.Length - 1)];
        public float BloodPrice(Rarity rarity) => bloodPrices[Mathf.Clamp((int)rarity, 0, bloodPrices.Length - 1)];
        public int RerollCost(int rerolls) => rerollBaseCost + rerolls * rerollCostStep;
        public float GambleChance(int luck) => Mathf.Min(gambleMaxChance, gambleBaseChance + luck * gambleChancePerLuck);
    }
}
