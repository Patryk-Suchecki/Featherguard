using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;

namespace Featherguard.Meta
{
    public readonly struct PotionStack
    {
        public readonly PotionDefinition Potion;
        public readonly int Count;

        public PotionStack(PotionDefinition potion, int count)
        {
            Potion = potion;
            Count = count;
        }
    }

    public sealed class PotionBag
    {
        readonly IReadOnlyList<PotionDefinition> potions;
        readonly SaveData data;
        readonly ISaveStore store;

        public PotionBag(IReadOnlyList<PotionDefinition> potions, SaveData data, ISaveStore store)
        {
            this.potions = potions;
            this.data = data;
            this.store = store;
        }

        public IReadOnlyList<PotionDefinition> All => potions;

        public IReadOnlyList<PotionStack> Owned =>
            potions.Select(p => new PotionStack(p, Count(p))).Where(s => s.Count > 0).ToList();

        public int Count(PotionDefinition potion) => Entry(potion)?.count ?? 0;

        public void Add(PotionDefinition potion, int count)
        {
            if (potion == null || count <= 0) return;
            var entry = Entry(potion);
            if (entry == null) data.potions.Add(entry = new ItemCount { id = potion.name });
            entry.count += count;
            store.Save(data);
        }

        public bool Remove(PotionDefinition potion, int count = 1)
        {
            var entry = Entry(potion);
            if (entry == null || count <= 0 || entry.count < count) return false;
            entry.count -= count;
            if (entry.count == 0) data.potions.Remove(entry);
            store.Save(data);
            return true;
        }

        public void Clear()
        {
            data.potions.Clear();
            store.Save(data);
        }

        ItemCount Entry(PotionDefinition potion) => potion == null ? null : data.potions.FirstOrDefault(e => e.id == potion.name);
    }
}
