using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace Featherguard.Battle
{
    public enum PerkId { ToughFeathers, SharpBeaks, Horseshoe, Purse, Gems, Legends }

    public class PerkInfo
    {
        public PerkId Id;
        public string Name;
        public string Description;
        public int MaxLevel;
        public int BaseCost;

        public int CostFor(int level) => BaseCost * (level + 1);
    }

    [Serializable]
    public class SaveData
    {
        public int feathers;
        public int runs;
        public int wins;
        public int bestBattles;
        public List<int> perkLevels = new();
    }

    public static class MetaProgress
    {
        public static readonly PerkInfo[] Perks =
        {
            new() { Id = PerkId.ToughFeathers, Name = "Twarde pióra", Description = "+6% max HP drużyny", MaxLevel = 5, BaseCost = 3 },
            new() { Id = PerkId.SharpBeaks, Name = "Ostre dzioby", Description = "+5% obrażeń drużyny", MaxLevel = 5, BaseCost = 3 },
            new() { Id = PerkId.Horseshoe, Name = "Podkowa", Description = "+1 szczęścia na start", MaxLevel = 3, BaseCost = 5 },
            new() { Id = PerkId.Purse, Name = "Sakiewka", Description = "+15 zł na start", MaxLevel = 4, BaseCost = 2 },
            new() { Id = PerkId.Gems, Name = "Klejnoty", Description = "Klejnot na bębnach automatu", MaxLevel = 1, BaseCost = 8 },
            new() { Id = PerkId.Legends, Name = "Legendy", Description = "Legendarne karty w puli", MaxLevel = 1, BaseCost = 10 },
        };

        static SaveData data;

        public static string SavePath => Path.Combine(Application.persistentDataPath, "featherguard_save.json");

        public static SaveData Data
        {
            get
            {
                if (data == null) Load();
                return data;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad() => data = null;

        static void Load()
        {
            try
            {
                data = File.Exists(SavePath) ? JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath)) : null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"Featherguard: could not read save ({e.Message}).");
                data = null;
            }
            data ??= new SaveData();
            while (data.perkLevels.Count < Perks.Length)
                data.perkLevels.Add(0);
        }

        public static void Save() => File.WriteAllText(SavePath, JsonUtility.ToJson(Data, true));

        public static int Level(PerkId id) => Data.perkLevels[(int)id];

        public static bool CanBuy(PerkInfo perk) => Level(perk.Id) < perk.MaxLevel && Data.feathers >= perk.CostFor(Level(perk.Id));

        public static void Buy(PerkInfo perk)
        {
            if (!CanBuy(perk)) return;
            Data.feathers -= perk.CostFor(Level(perk.Id));
            Data.perkLevels[(int)perk.Id]++;
            Save();
        }

        public static int FeathersFor(bool completed) => RunState.BattlesWon + RunState.GoldEarned / 50 + (completed ? 10 : 0);

        public static int RecordRun(bool completed)
        {
            int earned = FeathersFor(completed);
            Data.feathers += earned;
            Data.runs++;
            if (completed) Data.wins++;
            Data.bestBattles = Mathf.Max(Data.bestBattles, RunState.BattlesWon);
            Save();
            return earned;
        }

        public static void ApplyToRun()
        {
            RunState.Luck = Level(PerkId.Horseshoe);
            RunState.Gold = Level(PerkId.Purse) * 15;
            RunState.GemsUnlocked = Level(PerkId.Gems) > 0;
            RunState.LegendariesUnlocked = Level(PerkId.Legends) > 0;
            foreach (var hero in RunState.Heroes.Where(_ => Level(PerkId.ToughFeathers) > 0))
                RunState.AddModifier(hero, new StatModifier(StatType.MaxHealth, multiply: 0.06f * Level(PerkId.ToughFeathers)));
            foreach (var hero in RunState.Heroes.Where(_ => Level(PerkId.SharpBeaks) > 0))
                RunState.AddModifier(hero, new StatModifier(StatType.Damage, multiply: 0.05f * Level(PerkId.SharpBeaks)));
        }
    }
}
