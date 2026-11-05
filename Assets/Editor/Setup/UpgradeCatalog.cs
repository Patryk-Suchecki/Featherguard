using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Run;

namespace Featherguard.EditorTools
{
    public static class UpgradeCatalog
    {
        public static (UpgradeDefinition[] upgrades, UpgradeDefinition[] curses) Build(Seeder seeder, PresentationAssets art)
        {
            var list = new List<UpgradeDefinition>();

            void Add(string id, string name, string description, Rarity rarity, UpgradeScope scope, string icon, RunBonus bonus = RunBonus.None, float amount = 0f,
                bool curse = false, params StatModifier[] modifiers)
            {
                list.Add(seeder.Seed<UpgradeDefinition>($"{ContentPaths.Upgrades}/{id}.asset", u =>
                {
                    u.displayName = name;
                    u.description = description;
                    u.rarity = rarity;
                    u.scope = scope;
                    u.runBonus = bonus;
                    u.runAmount = amount;
                    u.curse = curse;
                    u.modifiers = modifiers;
                }, u => u.icon = art.Icon(icon)));
            }

            StatModifier M(StatType stat, float multiply) => new(stat, multiply: multiply);
            StatModifier A(StatType stat, float add) => new(stat, add: add);

            Add("SharpFeathers", "Sharp Feathers", "+15% damage", Rarity.Common, UpgradeScope.Hero, "Sword", modifiers: M(StatType.Damage, 0.15f));
            Add("ThickFeathers", "Thick Plumage", "+15% max HP", Rarity.Common, UpgradeScope.Hero, "Heart", modifiers: M(StatType.MaxHealth, 0.15f));
            Add("KeenEye", "Keen Eye", "+6% crit chance", Rarity.Common, UpgradeScope.Hero, "Crit", modifiers: A(StatType.CritChance, 0.06f));
            Add("WarmNest", "Warm Nest", "+20% healing", Rarity.Common, UpgradeScope.Hero, "Heal", modifiers: M(StatType.Healing, 0.2f));
            Add("DownJacket", "Down Jacket", "-10% damage taken", Rarity.Common, UpgradeScope.Hero, "Defense", modifiers: M(StatType.DamageTaken, -0.1f));
            Add("Penny", "Lucky Penny", "+15% gold gained", Rarity.Common, UpgradeScope.Run, "Coin", RunBonus.GoldGain, 0.15f);
            Add("Horseshoe", "Horseshoe", "+1 luck (better reels and cards)", Rarity.Common, UpgradeScope.Run, "Clover", RunBonus.Luck, 1f);
            Add("FuryBlade", "Fury Blade", "+25% damage", Rarity.Rare, UpgradeScope.Hero, "Sword", modifiers: M(StatType.Damage, 0.25f));
            Add("CruelTalons", "Cruel Talons", "+30% crit damage", Rarity.Rare, UpgradeScope.Hero, "Crit", modifiers: A(StatType.CritDamage, 0.3f));
            Add("VampireBeak", "Vampire Beak", "15% of damage dealt returns as HP", Rarity.Rare, UpgradeScope.Hero, "Lifesteal", modifiers: A(StatType.Lifesteal, 0.15f));
            Add("HotBlood", "Hot Blood", "+30% rage gain", Rarity.Rare, UpgradeScope.Hero, "Rage", modifiers: M(StatType.RageGain, 0.3f));
            Add("SteelFeathers", "Steel Feathers", "+25% max HP", Rarity.Rare, UpgradeScope.Hero, "Heart", modifiers: M(StatType.MaxHealth, 0.25f));
            Add("Clover", "Four-Leaf Clover", "+2 luck", Rarity.Rare, UpgradeScope.Run, "Clover", RunBonus.Luck, 2f);
            Add("Frenzy", "Frenzy", "+20% damage and +12% crit chance", Rarity.Epic, UpgradeScope.Hero, "Sword", modifiers: new[] { M(StatType.Damage, 0.2f), A(StatType.CritChance, 0.12f) });
            Add("Flock", "Flock", "+12% damage for the whole party", Rarity.Epic, UpgradeScope.Party, "Sword", modifiers: M(StatType.Damage, 0.12f));
            Add("Bastion", "Bastion", "-20% damage taken for the whole party", Rarity.Epic, UpgradeScope.Party, "Defense", modifiers: M(StatType.DamageTaken, -0.2f));
            Add("Treasurer", "Treasurer", "+40% gold gained", Rarity.Epic, UpgradeScope.Run, "Coin", RunBonus.GoldGain, 0.4f);
            Add("Phoenix", "Phoenix", "+40% max HP and +30% healing", Rarity.Legendary, UpgradeScope.Hero, "Heart", modifiers: new[] { M(StatType.MaxHealth, 0.4f), M(StatType.Healing, 0.3f) });
            Add("Jackpot", "Jackpot", "+4 luck", Rarity.Legendary, UpgradeScope.Run, "Clover", RunBonus.Luck, 4f);
            Add("ForestWrath", "Forest Wrath", "+25% damage and +10% crit for the whole party", Rarity.Legendary, UpgradeScope.Party, "Sword", modifiers: new[] { M(StatType.Damage, 0.25f), A(StatType.CritChance, 0.1f) });
            Add("CurseFrailty", "Frailty", "-12% max HP for the whole party", Rarity.Common, UpgradeScope.Party, "Skull", curse: true, modifiers: M(StatType.MaxHealth, -0.12f));
            Add("CurseDull", "Dull Beaks", "-10% damage for the whole party", Rarity.Common, UpgradeScope.Party, "Skull", curse: true, modifiers: M(StatType.Damage, -0.1f));
            Add("CurseExposed", "Exposed", "+15% damage taken for the whole party", Rarity.Common, UpgradeScope.Party, "Skull", curse: true, modifiers: M(StatType.DamageTaken, 0.15f));
            Add("CurseGreed", "Leaky Purse", "-25% gold gained", Rarity.Common, UpgradeScope.Run, "Skull", RunBonus.GoldGain, -0.25f, curse: true);

            return (list.Where(u => !u.curse).ToArray(), list.Where(u => u.curse).ToArray());
        }
    }
}
