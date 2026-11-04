using System.Collections.Generic;
using Featherguard.Core;

namespace Featherguard.Tests
{
    public sealed class FakeRandom : IRandomSource
    {
        readonly Queue<float> values;
        readonly float fallback;

        public FakeRandom(float fallback, params float[] sequence)
        {
            this.fallback = fallback;
            values = new Queue<float>(sequence);
        }

        public float Value => values.Count > 0 ? values.Dequeue() : fallback;
        public float Range(float min, float max) => min + (max - min) * Value;
        public int Range(int minInclusive, int maxExclusive) => minInclusive + (int)(Value * (maxExclusive - minInclusive - 0.0001f));
    }
}
