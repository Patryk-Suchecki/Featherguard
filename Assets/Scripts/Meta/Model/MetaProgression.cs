using System.Collections.Generic;
using System.Linq;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.Meta
{
    public sealed class MetaProgression
    {
        readonly ISaveStore store;

        public MetaConfig Config { get; }
        public SaveData Data { get; }
        public IReadOnlyList<PerkDefinition> Perks => Config.perks;

        public MetaProgression(MetaConfig config, ISaveStore store)
        {
            Config = config;
            this.store = store;
            Data = store.Load();
        }

        public int Level(PerkDefinition perk) => Data.perks.FirstOrDefault(p => p.id == perk.Id)?.level ?? 0;
        public bool IsMaxed(PerkDefinition perk) => Level(perk) >= perk.maxLevel;
        public int Cost(PerkDefinition perk) => perk.CostFor(Level(perk));
        public bool CanBuy(PerkDefinition perk) => !IsMaxed(perk) && Data.feathers >= Cost(perk);

        public bool Buy(PerkDefinition perk)
        {
            if (!CanBuy(perk)) return false;
            Data.feathers -= Cost(perk);
            var entry = Data.perks.FirstOrDefault(p => p.id == perk.Id);
            if (entry == null) Data.perks.Add(entry = new PerkLevel { id = perk.Id });
            entry.level++;
            store.Save(Data);
            return true;
        }

        public int FeathersFor(RunSession session, bool completed) =>
            session.BattlesWon * Config.feathersPerBattle + session.Wallet.Earned / Config.goldPerFeather + (completed ? Config.completionBonus : 0);

        public int RecordRun(RunSession session, bool completed)
        {
            int earned = FeathersFor(session, completed);
            Data.feathers += earned;
            Data.runs++;
            if (completed) Data.wins++;
            Data.bestBattles = Mathf.Max(Data.bestBattles, session.BattlesWon);
            store.Save(Data);
            return earned;
        }

        public void ApplyTo(RunSession session)
        {
            foreach (var perk in Perks)
            {
                int level = Level(perk);
                if (level > 0 && perk.effect != null)
                    perk.effect.Apply(session, level);
            }
        }
    }
}
