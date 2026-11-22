using System;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;

namespace Featherguard.Meta
{
    public sealed class PotionLoadout
    {
        readonly PotionBag bag;
        readonly SaveData data;
        readonly ISaveStore store;

        public PotionLoadout(PotionBag bag, int slots, SaveData data, ISaveStore store)
        {
            this.bag = bag;
            Slots = slots;
            this.data = data;
            this.store = store;
        }

        public int Slots { get; }

        public IReadOnlyList<PotionStack> Picked
        {
            get
            {
                var picked = new List<PotionStack>();
                int left = Slots;
                foreach (var potion in bag.All)
                {
                    int count = Math.Min(Math.Min(Saved(potion), bag.Count(potion)), left);
                    if (count <= 0) continue;
                    picked.Add(new PotionStack(potion, count));
                    left -= count;
                }
                return picked;
            }
        }

        public int Total => Picked.Sum(s => s.Count);

        public int Count(PotionDefinition potion) => Picked.FirstOrDefault(s => s.Potion == potion).Count;

        public bool CanAdd(PotionDefinition potion) => potion != null && Total < Slots && Count(potion) < bag.Count(potion);

        public bool Add(PotionDefinition potion)
        {
            if (!CanAdd(potion)) return false;
            Set(potion, Count(potion) + 1);
            return true;
        }

        public bool Remove(PotionDefinition potion)
        {
            int count = Count(potion);
            if (count <= 0) return false;
            Set(potion, count - 1);
            return true;
        }

        int Saved(PotionDefinition potion) => data.battlePotions.FirstOrDefault(e => e.id == potion.name)?.count ?? 0;

        void Set(PotionDefinition potion, int count)
        {
            data.battlePotions = Picked.Where(s => s.Potion != potion).Select(s => new ItemCount { id = s.Potion.name, count = s.Count }).ToList();
            if (count > 0) data.battlePotions.Add(new ItemCount { id = potion.name, count = count });
            store.Save(data);
        }
    }
}
