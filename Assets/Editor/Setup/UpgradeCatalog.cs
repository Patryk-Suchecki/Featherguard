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

            Add("SharpFeathers", "Ostre pióra", "+15% obrażeń", Rarity.Common, UpgradeScope.Hero, "Sword", modifiers: M(StatType.Damage, 0.15f));
            Add("ThickFeathers", "Gęste pióra", "+15% max HP", Rarity.Common, UpgradeScope.Hero, "Heart", modifiers: M(StatType.MaxHealth, 0.15f));
            Add("KeenEye", "Bystre oko", "+6% szansy na kryt", Rarity.Common, UpgradeScope.Hero, "Crit", modifiers: A(StatType.CritChance, 0.06f));
            Add("WarmNest", "Ciepłe gniazdo", "+20% leczenia", Rarity.Common, UpgradeScope.Hero, "Heal", modifiers: M(StatType.Healing, 0.2f));
            Add("DownJacket", "Puchowa kurtka", "-10% otrzymywanych obrażeń", Rarity.Common, UpgradeScope.Hero, "Defense", modifiers: M(StatType.DamageTaken, -0.1f));
            Add("Penny", "Grosik", "+15% zdobywanego złota", Rarity.Common, UpgradeScope.Run, "Coin", RunBonus.GoldGain, 0.15f);
            Add("Horseshoe", "Podkowa", "+1 szczęścia (lepsze bębny i karty)", Rarity.Common, UpgradeScope.Run, "Clover", RunBonus.Luck, 1f);
            Add("FuryBlade", "Ostrze furii", "+25% obrażeń", Rarity.Rare, UpgradeScope.Hero, "Sword", modifiers: M(StatType.Damage, 0.25f));
            Add("CruelTalons", "Okrutne szpony", "+30% obrażeń krytycznych", Rarity.Rare, UpgradeScope.Hero, "Crit", modifiers: A(StatType.CritDamage, 0.3f));
            Add("VampireBeak", "Wampirzy dziób", "15% zadanych obrażeń wraca jako HP", Rarity.Rare, UpgradeScope.Hero, "Lifesteal", modifiers: A(StatType.Lifesteal, 0.15f));
            Add("HotBlood", "Gorąca krew", "+30% przyrostu Furii", Rarity.Rare, UpgradeScope.Hero, "Rage", modifiers: M(StatType.RageGain, 0.3f));
            Add("SteelFeathers", "Stalowe pióra", "+25% max HP", Rarity.Rare, UpgradeScope.Hero, "Heart", modifiers: M(StatType.MaxHealth, 0.25f));
            Add("Clover", "Czterolistna koniczyna", "+2 szczęścia", Rarity.Rare, UpgradeScope.Run, "Clover", RunBonus.Luck, 2f);
            Add("Frenzy", "Szał", "+20% obrażeń i +12% szansy na kryt", Rarity.Epic, UpgradeScope.Hero, "Sword", modifiers: new[] { M(StatType.Damage, 0.2f), A(StatType.CritChance, 0.12f) });
            Add("Flock", "Stado", "+12% obrażeń dla wszystkich", Rarity.Epic, UpgradeScope.Party, "Sword", modifiers: M(StatType.Damage, 0.12f));
            Add("Bastion", "Bastion", "-20% otrzymywanych obrażeń dla wszystkich", Rarity.Epic, UpgradeScope.Party, "Defense", modifiers: M(StatType.DamageTaken, -0.2f));
            Add("Treasurer", "Skarbnik", "+40% zdobywanego złota", Rarity.Epic, UpgradeScope.Run, "Coin", RunBonus.GoldGain, 0.4f);
            Add("Phoenix", "Feniks", "+40% max HP i +30% leczenia", Rarity.Legendary, UpgradeScope.Hero, "Heart", modifiers: new[] { M(StatType.MaxHealth, 0.4f), M(StatType.Healing, 0.3f) });
            Add("Jackpot", "Jackpot", "+4 szczęścia", Rarity.Legendary, UpgradeScope.Run, "Clover", RunBonus.Luck, 4f);
            Add("ForestWrath", "Gniew lasu", "+25% obrażeń i +10% kryt dla wszystkich", Rarity.Legendary, UpgradeScope.Party, "Sword", modifiers: new[] { M(StatType.Damage, 0.25f), A(StatType.CritChance, 0.1f) });
            Add("CurseFrailty", "Kruchość", "-12% max HP dla wszystkich", Rarity.Common, UpgradeScope.Party, "Skull", curse: true, modifiers: M(StatType.MaxHealth, -0.12f));
            Add("CurseDull", "Tępe dzioby", "-10% obrażeń dla wszystkich", Rarity.Common, UpgradeScope.Party, "Skull", curse: true, modifiers: M(StatType.Damage, -0.1f));
            Add("CurseExposed", "Odsłonięcie", "+15% otrzymywanych obrażeń dla wszystkich", Rarity.Common, UpgradeScope.Party, "Skull", curse: true, modifiers: M(StatType.DamageTaken, 0.15f));
            Add("CurseGreed", "Dziurawa sakiewka", "-25% zdobywanego złota", Rarity.Common, UpgradeScope.Run, "Skull", RunBonus.GoldGain, -0.25f, curse: true);

            return (list.Where(u => !u.curse).ToArray(), list.Where(u => u.curse).ToArray());
        }
    }
}
