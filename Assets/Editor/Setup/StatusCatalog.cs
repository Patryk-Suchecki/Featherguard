using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class StatusCatalog
    {
        public static (Dictionary<string, StatusDefinition> Statuses, StatusVisuals[] Visuals) Build(Seeder seeder, PresentationAssets art)
        {
            var statuses = new Dictionary<string, StatusDefinition>();
            var visuals = new List<StatusVisuals>();

            void Add(string id, string name, string announcement, Color tint, System.Action<StatusDefinition> rules)
            {
                var status = seeder.Seed<StatusDefinition>($"{ContentPaths.Statuses}/{id}.asset", s =>
                {
                    s.displayName = name;
                    s.announcement = announcement;
                    s.modifiers = new StatModifier[0];
                    s.damagePerTurn = s.skipsTurn = s.forcesTargeting = s.consumedOnAttack = false;
                    rules(s);
                });
                statuses[id] = status;
                visuals.Add(seeder.Seed<StatusVisuals>($"{ContentPaths.StatusVisuals}/{id}.asset", v => v.tint = tint, v =>
                {
                    v.status = status;
                    v.icon = art.Icon(id);
                }));
            }

            Add("Shield", "Shield", "Shielded", new Color(0.3f, 0.6f, 1f), s => s.modifiers = new[] { new StatModifier(StatType.DamageTaken, multiply: -0.5f) });
            Add("Empower", "Empower", "Empowered!", new Color(1f, 0.55f, 0.15f), s =>
            {
                s.modifiers = new[] { new StatModifier(StatType.Damage, multiply: 0.5f) };
                s.consumedOnAttack = true;
            });
            Add("Poison", "Poison", "Poisoned", new Color(0.4f, 0.85f, 0.25f), s => s.damagePerTurn = true);
            Add("Stun", "Stun", "Stunned!", new Color(1f, 0.85f, 0.2f), s => s.skipsTurn = true);
            Add("Weaken", "Weaken", "Weakened", new Color(0.65f, 0.4f, 0.9f), s => s.modifiers = new[] { new StatModifier(StatType.Damage, multiply: -0.3f) });
            Add("Taunt", "Taunt", "Taunting", new Color(0.95f, 0.3f, 0.3f), s => s.forcesTargeting = true);
            return (statuses, visuals.ToArray());
        }
    }
}
