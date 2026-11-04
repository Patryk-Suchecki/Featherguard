using System;

namespace Featherguard.Battle
{
    public sealed class RageMeter
    {
        public float Max { get; }
        public float Value { get; private set; }
        public float Fraction => Max <= 0f ? 0f : Value / Max;
        public bool Ready => Value >= Max;

        public event Action BecameReady;

        public RageMeter(float max, float initial = 0f)
        {
            Max = max;
            Value = Math.Clamp(initial, 0f, max);
        }

        public void Add(float amount)
        {
            bool wasReady = Ready;
            Value = Math.Clamp(Value + amount, 0f, Max);
            if (!wasReady && Ready) BecameReady?.Invoke();
        }

        public void Consume() => Value = 0f;

        public void Set(float value) => Value = Math.Clamp(value, 0f, Max);
    }
}
