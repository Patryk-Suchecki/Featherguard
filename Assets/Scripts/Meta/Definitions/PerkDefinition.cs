using System;
using Featherguard.Battle;
using Featherguard.Core;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.Meta
{
    [CreateAssetMenu(menuName = "Featherguard/Perk")]
    public class PerkDefinition : ScriptableObject
    {
        public string displayName;
        public string description;
        public Sprite icon;
        [Min(1)] public int maxLevel = 1;
        [Min(0)] public int baseCost = 3;
        [SerializeReference, SubclassSelector] public PerkEffect effect;

        public string Id => name;
        public int CostFor(int level) => baseCost * (level + 1);
    }

    [Serializable]
    public abstract class PerkEffect
    {
        public abstract void Apply(RunSession session, int level);
    }

    [Serializable]
    public sealed class PartyStatPerk : PerkEffect
    {
        public StatType stat;
        public float multiplyPerLevel;
        public float addPerLevel;

        public override void Apply(RunSession session, int level)
        {
            foreach (var hero in session.Party.Heroes)
                session.Party.AddModifier(hero, new StatModifier(stat, addPerLevel * level, multiplyPerLevel * level));
        }
    }

    [Serializable]
    public sealed class LuckPerk : PerkEffect
    {
        public int perLevel = 1;

        public override void Apply(RunSession session, int level) => session.Luck += perLevel * level;
    }

    [Serializable]
    public sealed class StartingGoldPerk : PerkEffect
    {
        public int perLevel = 15;

        public override void Apply(RunSession session, int level) => session.Wallet.Deposit(perLevel * level);
    }

    [Serializable]
    public sealed class UnlockPerk : PerkEffect
    {
        public RunUnlock unlock;

        public override void Apply(RunSession session, int level) => session.Unlock(unlock);
    }
}
