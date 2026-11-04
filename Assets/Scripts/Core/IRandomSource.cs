namespace Featherguard.Core
{
    public interface IRandomSource
    {
        float Value { get; }
        float Range(float min, float max);
        int Range(int minInclusive, int maxExclusive);
    }

    public sealed class UnityRandomSource : IRandomSource
    {
        public static readonly UnityRandomSource Shared = new();

        public float Value => UnityEngine.Random.value;
        public float Range(float min, float max) => UnityEngine.Random.Range(min, max);
        public int Range(int minInclusive, int maxExclusive) => UnityEngine.Random.Range(minInclusive, maxExclusive);
    }

    public sealed class SeededRandomSource : IRandomSource
    {
        readonly System.Random random;

        public SeededRandomSource(int seed) => random = new System.Random(seed);

        public float Value => (float)random.NextDouble();
        public float Range(float min, float max) => min + (max - min) * Value;
        public int Range(int minInclusive, int maxExclusive) => random.Next(minInclusive, maxExclusive);
    }
}
