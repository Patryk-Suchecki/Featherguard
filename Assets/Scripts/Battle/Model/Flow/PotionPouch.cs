using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Battle
{
    public readonly struct BattlePotion
    {
        public readonly PotionDefinition Potion;
        public readonly int Count;

        public BattlePotion(PotionDefinition potion, int count)
        {
            Potion = potion;
            Count = count;
        }
    }

    public sealed class PotionPouch
    {
        readonly List<PotionDefinition> potions = new();
        readonly Dictionary<PotionDefinition, int> counts = new();

        public PotionPouch(IEnumerable<BattlePotion> picked)
        {
            foreach (var entry in picked ?? Enumerable.Empty<BattlePotion>())
            {
                if (entry.Potion == null || entry.Count <= 0) continue;
                if (!counts.ContainsKey(entry.Potion)) potions.Add(entry.Potion);
                counts[entry.Potion] = Count(entry.Potion) + entry.Count;
            }
        }

        public IReadOnlyList<PotionDefinition> Potions => potions;

        public int Count(PotionDefinition potion) => potion != null && counts.TryGetValue(potion, out int count) ? count : 0;

        public bool Take(PotionDefinition potion)
        {
            int count = Count(potion);
            if (count <= 0) return false;
            counts[potion] = count - 1;
            return true;
        }
    }
}
