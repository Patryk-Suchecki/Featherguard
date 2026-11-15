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

            void Add(string id, string name, string announcement, Color tint, string icon, bool harmful, params StatusBehaviour[] behaviours) =>
                AddStacking(id, name, announcement, tint, icon, harmful, 1, behaviours);

            void AddStacking(string id, string name, string announcement, Color tint, string icon, bool harmful, int maxStacks, params StatusBehaviour[] behaviours)
            {
                var status = seeder.Seed<StatusDefinition>($"{ContentPaths.Statuses}/{id}.asset", s =>
                {
                    s.displayName = name;
                    s.announcement = announcement;
                    s.harmful = harmful;
                    s.stacking = maxStacks > 1 ? StatusStacking.Stack : StatusStacking.Refresh;
                    s.maxStacks = maxStacks;
                    s.behaviours = behaviours;
                });
                statuses[id] = status;
                visuals.Add(seeder.Seed<StatusVisuals>($"{ContentPaths.StatusVisuals}/{id}.asset", v => v.tint = tint, v =>
                {
                    v.status = status;
                    v.icon = art.Icon(icon);
                }));
            }

            StatusBehaviour Modify(StatType stat, float add = 0f, float multiply = 0f) => new ModifyStats { modifiers = new[] { new StatModifier(stat, add, multiply) } };

            Add("Shield", "Shield", "Shielded", new Color(0.3f, 0.6f, 1f), "Shield", false, new AbsorbDamage());
            Add("Empower", "Empower", "Empowered!", new Color(1f, 0.55f, 0.15f), "Empower", false, Modify(StatType.Damage, multiply: 0.25f));
            Add("Poison", "Poison", "Poisoned", new Color(0.4f, 0.85f, 0.25f), "Poison", true, new DamageOverTime());
            Add("Stun", "Stun", "Stunned!", new Color(1f, 0.85f, 0.2f), "Stun", true, new SkipTurn());
            Add("Weaken", "Weaken", "Weakened", new Color(0.65f, 0.4f, 0.9f), "Weaken", true, Modify(StatType.Damage, multiply: -0.3f));
            Add("Taunt", "Taunt", "Taunting", new Color(0.95f, 0.3f, 0.3f), "Taunt", false, new ForceTargeting());
            Add("Bleed", "Bleed", "Bleeding", new Color(0.75f, 0.1f, 0.12f), "Lifesteal", true, new DamageOverTime());
            AddStacking("Burn", "Burn", "Burning", new Color(1f, 0.5f, 0.1f), "Rage", true, 5, new DamageOverTime());
            Add("Freeze", "Freeze", "Frozen", new Color(0.55f, 0.85f, 1f), "Defense", true, Modify(StatType.DamageTaken, multiply: 0.3f), new ActLast());
            Add("Curse", "Curse", "Cursed", new Color(0.45f, 0.2f, 0.55f), "Weaken", true, Modify(StatType.Damage, multiply: -0.3f));
            Add("Mark", "Mark", "Marked", new Color(1f, 0.8f, 0.25f), "Crit", true, Modify(StatType.CritChanceTaken, add: 0.5f), new ConsumeOnCriticalHit());
            return (statuses, visuals.ToArray());
        }
    }
}
