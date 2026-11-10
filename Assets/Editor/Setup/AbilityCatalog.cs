using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Feedback;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class AbilityCatalog
    {
        static readonly Color Spark = new(1f, 0.62f, 0.15f);
        static readonly Color Ember = new(1f, 0.45f, 0.1f);

        public static (Dictionary<string, AbilityDefinition> Abilities, AbilityVisuals[] Visuals) Build(Seeder seeder, IReadOnlyDictionary<string, StatusDefinition> statuses)
        {
            var arrow = AssetDatabase.LoadAssetAtPath<GameObject>($"{ContentPaths.Environment}/Arrow.fbx");
            AbilityDelivery Melee() => new MeleeDelivery();
            AbilityDelivery Arrow() => new ProjectileDelivery { prefab = arrow, speed = 16f, arc = 0.12f, orientAlongPath = true };
            AbilityDelivery Magic() => new ProjectileDelivery { speed = 10f, arc = 0.05f };
            AbilityDelivery Instant() => new InstantDelivery();
            AbilityEffect Damage(int power) => new DamageEffect { power = power };
            AbilityEffect Heal(int power) => new HealEffect { power = power };
            AbilityEffect Status(string id, int turns = 1, int potency = 0, float chance = 1f, bool onSelf = false) => new ApplyStatusEffect
            {
                application = new StatusApplication { status = statuses[id], turns = turns, potency = potency, chance = chance, onSelf = onSelf },
            };

            var abilities = new Dictionary<string, AbilityDefinition>();
            var visuals = new List<AbilityVisuals>();

            void Add(string id, string name, AbilityTarget target, System.Func<AbilityDelivery> delivery, string state, Color color, SfxId cast, SfxId impact,
                int hits = 1, bool heavy = false, ImpactEffect impactEffect = null, params AbilityEffect[] effects)
            {
                var ability = seeder.Seed<AbilityDefinition>($"{ContentPaths.Abilities}/{id}.asset", a =>
                {
                    a.displayName = name;
                    a.target = target;
                    a.hits = hits;
                    a.effects = effects.ToArray();
                });
                abilities[id] = ability;
                visuals.Add(seeder.Seed<AbilityVisuals>($"{ContentPaths.AbilityVisuals}/{id}.asset", v =>
                {
                    v.delivery = delivery();
                    v.animationState = state;
                    v.heavy = heavy;
                    v.fxColor = color;
                    v.castSound = cast;
                    v.impactSound = impact;
                    v.impactEffect = impactEffect;
                }, v => v.ability = ability));
            }

            Add("KnightAttack", "Slash", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Hit, effects: Damage(30));
            Add("KnightSupport", "Cover", AbilityTarget.Ally, Instant, "Cast", new Color(0.45f, 0.75f, 1f), SfxId.None, SfxId.Shield, effects: new[] { Status("Shield"), Status("Taunt", onSelf: true) });
            Add("KnightSpecial", "Shield Bash", AbilityTarget.Enemy, Melee, "Attack", new Color(1f, 0.7f, 0.25f), SfxId.Slash, SfxId.Smash, heavy: true, effects: new[] { Damage(85), Status("Stun") });
            Add("ArcherAttack", "Double Shot", AbilityTarget.Enemy, Arrow, "Attack", Spark, SfxId.Arrow, SfxId.Hit, hits: 2, effects: new[] { Damage(14), Status("Poison", 2, 5, 0.3f) });
            Add("ArcherSupport", "Battle Cry", AbilityTarget.Ally, Instant, "Cast", new Color(1f, 0.6f, 0.2f), SfxId.None, SfxId.Buff, effects: Status("Empower"));
            Add("ArcherSpecial", "Arrow Rain", AbilityTarget.AllEnemies, Arrow, "Attack", Spark, SfxId.Arrow, SfxId.Hit, hits: 2, heavy: true, effects: new[] { Damage(24), Status("Poison", 2, 6) });
            Add("OwlAttack", "Spark", AbilityTarget.Enemy, Magic, "Attack", new Color(0.4f, 1f, 0.6f), SfxId.Magic, SfxId.Hit, effects: new[] { Damage(20), Status("Weaken", chance: 0.35f) });
            Add("OwlSupport", "Mend", AbilityTarget.Ally, Instant, "Cast", new Color(0.45f, 1f, 0.5f), SfxId.None, SfxId.Heal, effects: Heal(40));
            Add("OwlSpecial", "Forest Renewal", AbilityTarget.AllAllies, Instant, "Cast", new Color(0.5f, 1f, 0.6f), SfxId.None, SfxId.Heal, heavy: true, effects: Heal(60));
            Add("GruntAttack", "Club", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Hit, effects: Damage(15));
            Add("ShamanAttack", "Hex", AbilityTarget.Enemy, Magic, "Attack", new Color(0.8f, 0.4f, 1f), SfxId.Magic, SfxId.Hit, effects: new[] { Damage(12), Status("Weaken", chance: 0.5f) });
            Add("ShamanSupport", "Dark Mending", AbilityTarget.Ally, Instant, "Cast", new Color(0.8f, 0.45f, 1f), SfxId.None, SfxId.Heal, effects: Heal(25));
            Add("BruteAttack", "Crush", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Smash, heavy: true, effects: new[] { Damage(26), Status("Stun", chance: 0.25f) });
            Add("SpitterAttack", "Venom", AbilityTarget.Enemy, Magic, "Attack", new Color(0.5f, 1f, 0.2f), SfxId.Magic, SfxId.Hit, effects: new[] { Damage(9), Status("Poison", 2, 6, 0.7f) });
            Add("GuardAttack", "Spear Thrust", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Hit, effects: Damage(14));
            Add("GuardSupport", "Shield Wall", AbilityTarget.Ally, Instant, "Cast", new Color(0.45f, 0.7f, 1f), SfxId.None, SfxId.Shield, effects: new[] { Status("Shield"), Status("Taunt", onSelf: true) });
            Add("ChieftainSpecial", "Earthquake", AbilityTarget.AllEnemies, Instant, "Attack", new Color(1f, 0.45f, 0.15f), SfxId.Smash, SfxId.Smash, heavy: true, effects: new[] { Damage(22), Status("Weaken", chance: 0.5f) });
            Add("ChieftainAttack", "Chieftain's Wrath", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Smash, heavy: true, effects: new[] { Damage(34), Status("Stun", chance: 0.35f) });
            Add("BiesAttack", "Rending Claws", AbilityTarget.Enemy, Melee, "Attack", Ember, SfxId.Slash, SfxId.Smash, heavy: true, impactEffect: new ClawSlashEffect(), effects: Damage(24));
            Add("BiesSpecial", "Howl", AbilityTarget.AllEnemies, Instant, "Cast", Ember, SfxId.Rage, SfxId.Magic, heavy: true, effects: Status("Weaken"));
            return (abilities, visuals.ToArray());
        }
    }
}
