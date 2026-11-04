using System.Collections.Generic;
using Featherguard.Battle;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class StatusCatalog
    {
        public static Dictionary<string, StatusDefinition> Build(Seeder seeder, PresentationAssets art)
        {
            var statuses = new Dictionary<string, StatusDefinition>();

            void Add(string id, string name, string announcement, Color tint, System.Action<StatusDefinition> rules)
            {
                statuses[id] = seeder.Seed<StatusDefinition>($"{ContentPaths.Statuses}/{id}.asset", s =>
                {
                    s.displayName = name;
                    s.announcement = announcement;
                    s.tint = tint;
                    s.modifiers = new StatModifier[0];
                    s.damagePerTurn = s.skipsTurn = s.forcesTargeting = s.consumedOnAttack = false;
                    rules(s);
                }, s => s.icon = art.Icon(id));
            }

            Add("Shield", "Tarcza", "Tarcza", new Color(0.3f, 0.6f, 1f), s => s.modifiers = new[] { new StatModifier(StatType.DamageTaken, multiply: -0.5f) });
            Add("Empower", "Moc", "Moc!", new Color(1f, 0.55f, 0.15f), s =>
            {
                s.modifiers = new[] { new StatModifier(StatType.Damage, multiply: 0.5f) };
                s.consumedOnAttack = true;
            });
            Add("Poison", "Trucizna", "Trucizna", new Color(0.4f, 0.85f, 0.25f), s => s.damagePerTurn = true);
            Add("Stun", "Ogłuszenie", "Ogłuszenie!", new Color(1f, 0.85f, 0.2f), s => s.skipsTurn = true);
            Add("Weaken", "Osłabienie", "Osłabienie", new Color(0.65f, 0.4f, 0.9f), s => s.modifiers = new[] { new StatModifier(StatType.Damage, multiply: -0.3f) });
            Add("Taunt", "Prowokacja", "Prowokacja", new Color(0.95f, 0.3f, 0.3f), s => s.forcesTargeting = true);
            return statuses;
        }
    }
}
