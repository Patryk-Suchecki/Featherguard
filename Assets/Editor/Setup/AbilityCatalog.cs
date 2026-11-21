using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class AbilityCatalog
    {
        static readonly Color Spark = new(1f, 0.62f, 0.15f);
        static readonly Color Ember = new(1f, 0.45f, 0.1f);

        public static (Dictionary<string, AbilityDefinition> Abilities, AbilityVisuals[] Visuals) Build(Seeder seeder, IReadOnlyDictionary<string, StatusDefinition> statuses)
        {
            AbilityDelivery Melee() => new MeleeDelivery();
            AbilityDelivery Magic() => new ProjectileDelivery { speed = 10f, arc = 0.05f };
            AbilityDelivery Instant() => new InstantDelivery();
            AbilityEffect Damage(int power) => new DamageEffect { power = power };
            AbilityEffect Heal(int power) => new HealEffect { power = power };
            AbilityEffect Status(string id, int turns = 1, int potency = 0, float chance = 1f, bool onSelf = false, bool perfect = false) => new ApplyStatusEffect
            {
                application = new StatusApplication
                {
                    status = statuses[id], turns = turns, potency = potency, chance = chance, onSelf = onSelf,
                    condition = perfect ? StatusCondition.PerfectTiming : StatusCondition.Always,
                },
            };

            var abilities = new Dictionary<string, AbilityDefinition>();
            var visuals = new List<AbilityVisuals>();

            void Add(string id, string name, AbilityTarget target, System.Func<AbilityDelivery> delivery, string state, Color color, SfxId cast, SfxId impact,
                int hits = 1, bool heavy = false, ImpactEffect impactEffect = null, TimingGesture gesture = null, bool alwaysTimed = false, params AbilityEffect[] effects)
            {
                var ability = seeder.Seed<AbilityDefinition>($"{ContentPaths.Abilities}/{id}.asset", a =>
                {
                    a.displayName = name;
                    a.target = target;
                    a.hits = hits;
                    a.effects = effects.ToArray();
                    a.alwaysTimed = alwaysTimed;
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
                    v.gesture = gesture;
                }, v => v.ability = ability));
            }

            void AddHeroes()
            {
                var amber = new Color(1f, 0.72f, 0.25f);
                var leaf = new Color(0.45f, 0.95f, 0.45f);
                var thorn = new Color(0.5f, 0.78f, 0.3f);
                var storm = new Color(0.55f, 0.78f, 1f);

                Add("WarriorAttack", "Sword Cut", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Hit,
                    gesture: new SlashGesture { angle = 0f, closeTime = 0.9f, tolerance = 40f },
                    effects: new[] { Damage(30), Status("Bleed", 3, 5, perfect: true) });
                Add("WarriorSupport", "War Shout", AbilityTarget.AllAllies, Instant, "Cast", new Color(1f, 0.6f, 0.2f), SfxId.Rage, SfxId.Buff,
                    gesture: new SeriesGesture { count = 2, interval = 0.5f, approach = 0.9f }, alwaysTimed: true,
                    effects: new[] { Status("Empower", 2), Status("Empower", 3, perfect: true) });
                Add("WarriorSpecial", "Hero's Blow", AbilityTarget.Enemy, Melee, "Attack", new Color(1f, 0.7f, 0.25f), SfxId.Slash, SfxId.Smash, heavy: true,
                    gesture: new SequenceGesture
                    {
                        windowScale = 0.85f,
                        steps = new TimingGesture[]
                        {
                            new SlashGesture { angle = 0f, closeTime = 0.8f, tolerance = 35f },
                            new SlashGesture { angle = 135f, closeTime = 0.8f, tolerance = 35f },
                            new SlashGesture { angle = 270f, closeTime = 0.8f, tolerance = 35f },
                        },
                    }, alwaysTimed: true,
                    effects: new[] { Damage(85), Status("Stun", perfect: true) });

                Add("ThornWardenAttack", "Thorn Lash", AbilityTarget.Enemy, Melee, "Attack", thorn, SfxId.Slash, SfxId.Hit,
                    gesture: new RingGesture { closeTime = 0.8f },
                    effects: new[] { Damage(16), Status("Taunt", onSelf: true), Status("Weaken", perfect: true) });
                Add("ThornWardenSupport", "Thorn Guard", AbilityTarget.Ally, Instant, "Cast", thorn, SfxId.None, SfxId.Shield,
                    gesture: new TraceGesture { shape = GestureShape.Thorns, size = 300f, timeLimit = 2.2f, tolerance = 55f }, alwaysTimed: true,
                    effects: new[] { Status("Thorns", 2, 8), Status("Shield", potency: 20, perfect: true) });
                Add("ThornWardenSpecial", "Bramble Wall", AbilityTarget.AllAllies, Instant, "Cast", thorn, SfxId.None, SfxId.Shield, heavy: true,
                    gesture: new SequenceGesture
                    {
                        windowScale = 0.85f,
                        steps = new TimingGesture[]
                        {
                            new TraceGesture { shape = GestureShape.Shield, size = 260f, timeLimit = 2f, tolerance = 50f },
                            new RingGesture { closeTime = 0.7f },
                        },
                    }, alwaysTimed: true,
                    effects: new[] { Status("Thorns", 1, 20), Status("Taunt", onSelf: true), Status("Shield", potency: 25, perfect: true) });

                Add("HealerAttack", "Amber Spark", AbilityTarget.Enemy, Magic, "Attack", amber, SfxId.Magic, SfxId.Hit,
                    gesture: new ChargeGesture { fillTime = 1.1f, zone = 0.8f },
                    effects: new[] { Damage(14), Status("Mark", perfect: true) });
                Add("HealerSupport", "Oak Blessing", AbilityTarget.Ally, Instant, "Cast", leaf, SfxId.None, SfxId.Heal,
                    gesture: new GuideGesture { shape = GestureShape.Wave, size = 320f, travelTime = 1.5f, tolerance = 70f }, alwaysTimed: true,
                    effects: new AbilityEffect[] { new HealEffect { power = 40, overflowShield = statuses["Shield"] }, new PerfectTimingEffect { effect = new CleanseEffect() } });
                Add("HealerSpecial", "Grace of the Oak", AbilityTarget.AllAllies, Instant, "Cast", leaf, SfxId.None, SfxId.Heal, heavy: true,
                    gesture: new SignGesture { shape = GestureShape.Circle, size = 230f, showTime = 0.9f, drawTime = 2.5f }, alwaysTimed: true,
                    effects: new[] { Heal(45), new CleanseEffect(), Status("Shield", potency: 25, perfect: true) });

                Add("ThundercallerAttack", "Chain Lightning", AbilityTarget.Enemy, Magic, "Attack", storm, SfxId.Magic, SfxId.Hit,
                    gesture: new SeriesGesture { count = 3, interval = 0.32f, approach = 0.75f },
                    effects: new[] { new ChainDamageEffect { power = 22, falloff = 0.6f }, Status("Stun", chance: 0.5f, perfect: true) });
                Add("ThundercallerSupport", "Spark Ward", AbilityTarget.Ally, Instant, "Cast", storm, SfxId.None, SfxId.Shield,
                    gesture: new SpinGesture { turns = 2, timeLimit = 2f }, alwaysTimed: true,
                    effects: new[] { Status("Sparks", 2, 8), Status("Empower", perfect: true) });
                Add("ThundercallerSpecial", "Perun's Storm", AbilityTarget.AllEnemies, Magic, "Attack", storm, SfxId.Magic, SfxId.Smash, heavy: true,
                    gesture: new SignGesture { shape = GestureShape.Lightning, size = 260f, showTime = 0.8f, drawTime = 2.2f, goodScore = 0.82f, perfectScore = 0.91f }, alwaysTimed: true,
                    effects: new[] { Damage(50), Status("Stun", perfect: true) });
            }

            Add("GruntAttack", "Club", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Hit, effects: Damage(15));
            Add("ShamanAttack", "Hex", AbilityTarget.Enemy, Magic, "Attack", new Color(0.8f, 0.4f, 1f), SfxId.Magic, SfxId.Hit, effects: new[] { Damage(12), Status("Weaken", chance: 0.5f) });
            Add("ShamanSupport", "Dark Mending", AbilityTarget.Ally, Instant, "Cast", new Color(0.8f, 0.45f, 1f), SfxId.None, SfxId.Heal, effects: Heal(25));
            Add("BruteAttack", "Crush", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Smash, heavy: true, effects: new[] { Damage(26), Status("Stun", chance: 0.25f) });
            Add("SpitterAttack", "Venom", AbilityTarget.Enemy, Magic, "Attack", new Color(0.5f, 1f, 0.2f), SfxId.Magic, SfxId.Hit, effects: new[] { Damage(9), Status("Poison", 2, 6, 0.7f) });
            Add("GuardAttack", "Spear Thrust", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Hit, effects: Damage(14));
            Add("GuardSupport", "Shield Wall", AbilityTarget.Ally, Instant, "Cast", new Color(0.45f, 0.7f, 1f), SfxId.None, SfxId.Shield, effects: new[] { Status("Shield", potency: 30), Status("Taunt", onSelf: true) });
            Add("ChieftainSpecial", "Earthquake", AbilityTarget.AllEnemies, Instant, "Attack", new Color(1f, 0.45f, 0.15f), SfxId.Smash, SfxId.Smash, heavy: true, effects: new[] { Damage(22), Status("Weaken", chance: 0.5f) });
            Add("ChieftainAttack", "Chieftain's Wrath", AbilityTarget.Enemy, Melee, "Attack", Spark, SfxId.Slash, SfxId.Smash, heavy: true, effects: new[] { Damage(34), Status("Stun", chance: 0.35f) });
            Add("BiesAttack", "Rending Claws", AbilityTarget.Enemy, Melee, "Attack", Ember, SfxId.Slash, SfxId.Smash, heavy: true, impactEffect: new ClawSlashEffect(), effects: new[] { Damage(24), Status("Bleed", 3, 6) });
            Add("BiesSpecial", "Howl", AbilityTarget.AllEnemies, Instant, "Cast", Ember, SfxId.Rage, SfxId.Magic, heavy: true, effects: Status("Weaken"));
            Add("WolfAttack", "Bite", AbilityTarget.Enemy, Melee, "Attack", Ember, SfxId.Slash, SfxId.Hit, effects: Damage(14));
            Add("WolfSpecial", "Pack Howl", AbilityTarget.AllAllies, Instant, "Cast", Ember, SfxId.Rage, SfxId.Buff, effects: Status("Empower"));
            AddHeroes();
            return (abilities, visuals.ToArray());
        }
    }
}
