using System.Collections.Generic;

namespace Featherguard.Battle
{
    public readonly struct StatusSnapshot
    {
        public readonly StatusDefinition Definition;
        public readonly int Turns;
        public readonly int Stacks;
        public readonly int Points;

        public StatusSnapshot(StatusDefinition definition, int turns, int stacks = 1, int points = 0)
        {
            Definition = definition;
            Turns = turns;
            Stacks = stacks;
            Points = points;
        }
    }

    public sealed class UnitSnapshot
    {
        public int Health;
        public int MaxHealth;
        public IReadOnlyList<StatusSnapshot> Statuses;

        public bool Alive => Health > 0;
        public float HealthFraction => MaxHealth <= 0 ? 0f : (float)Health / MaxHealth;
    }
}
