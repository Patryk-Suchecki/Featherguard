using System;
using System.Collections.Generic;
using System.Linq;

namespace Featherguard.Core
{
    public static class WeightedRandom
    {
        public static T Pick<T>(IReadOnlyList<T> items, Func<T, float> weight, IRandomSource random)
        {
            if (items.Count == 0) throw new ArgumentException("Cannot pick from an empty list.", nameof(items));
            float total = items.Sum(i => Math.Max(0f, weight(i)));
            if (total <= 0f) return items[random.Range(0, items.Count)];
            float roll = random.Value * total;
            foreach (var item in items)
            {
                roll -= Math.Max(0f, weight(item));
                if (roll <= 0f) return item;
            }
            return items[^1];
        }

        public static T PickUniform<T>(IReadOnlyList<T> items, IRandomSource random) => items[random.Range(0, items.Count)];
    }
}
