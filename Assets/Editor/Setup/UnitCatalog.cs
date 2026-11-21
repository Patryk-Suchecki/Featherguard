using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class UnitCatalog
    {
        public static (Dictionary<string, UnitDefinition> Units, UnitVisuals[] Visuals) Build(Seeder seeder, IReadOnlyDictionary<string, AbilityDefinition> abilities, PresentationAssets art)
        {
            var units = new Dictionary<string, UnitDefinition>();
            var visuals = new List<UnitVisuals>();

            void Add(string id, string name, Team team, int health, float height, float scale, string attack, string support, string special,
                System.Action<UnitDefinition> extra = null, string model = null, HeroClass heroClass = HeroClass.None)
            {
                model ??= id;
                var unit = seeder.Seed<UnitDefinition>($"{ContentPaths.Units}/{id}.asset", u =>
                {
                    u.displayName = name;
                    u.team = team;
                    u.heroClass = heroClass;
                    u.maxHealth = health;
                    u.attack = abilities[attack];
                    u.support = support != null ? abilities[support] : null;
                    u.special = special != null ? abilities[special] : null;
                    u.ai = new EnemyAiProfile();
                    u.boss = new BossProfile();
                    extra?.Invoke(u);
                });
                units[id] = unit;
                visuals.Add(seeder.Seed<UnitVisuals>($"{ContentPaths.UnitVisuals}/{id}.asset", v =>
                {
                    v.height = height;
                    v.modelScale = scale;
                }, v =>
                {
                    v.unit = unit;
                    v.model = AssetDatabase.LoadAssetAtPath<GameObject>($"{ContentPaths.Characters}/{model}.fbx");
                    v.portrait = art.Portrait(model);
                    v.animator = AnimatorBuilder.Build(model);
                }));
            }

            Add("Knight", "Robin", Team.Heroes, 150, 1.4f, 1f, "KnightAttack", "KnightSupport", "KnightSpecial", heroClass: HeroClass.Defender);
            Add("Archer", "Jay", Team.Heroes, 105, 1.4f, 1f, "ArcherAttack", "ArcherSupport", "ArcherSpecial", heroClass: HeroClass.Warrior);
            Add("Owl", "Owl", Team.Heroes, 115, 1.4f, 1f, "OwlAttack", "OwlSupport", "OwlSpecial", heroClass: HeroClass.Support);
            Add("Warrior", "Warrior", Team.Heroes, 140, 1.4f, 1f, "WarriorAttack", "WarriorSupport", "WarriorSpecial", model: "Knight", heroClass: HeroClass.Warrior);
            Add("ThornWarden", "Thorn Warden", Team.Heroes, 165, 1.4f, 1.15f, "ThornWardenAttack", "ThornWardenSupport", "ThornWardenSpecial", model: "Knight", heroClass: HeroClass.Defender);
            Add("Healer", "Healer", Team.Heroes, 110, 1.4f, 1f, "HealerAttack", "HealerSupport", "HealerSpecial", model: "Owl", heroClass: HeroClass.Support);
            Add("Thundercaller", "Thundercaller of Perun", Team.Heroes, 100, 1.4f, 1f, "ThundercallerAttack", "ThundercallerSupport", "ThundercallerSpecial", model: "Archer", heroClass: HeroClass.Mage);
            Add("Grunt", "Thug", Team.Enemies, 80, 1.5f, 1f, "GruntAttack", null, null);
            Add("Shaman", "Shaman", Team.Enemies, 70, 1.5f, 1f, "ShamanAttack", "ShamanSupport", null);
            Add("Brute", "Brute", Team.Enemies, 170, 1.5f, 1.4f, "BruteAttack", null, null);
            Add("Spitter", "Spitter", Team.Enemies, 65, 1.5f, 1f, "SpitterAttack", null, null);
            Add("Guard", "Shieldbearer", Team.Enemies, 110, 1.5f, 1.1f, "GuardAttack", "GuardSupport", null);
            Add("Chieftain", "Chieftain", Team.Enemies, 460, 1.5f, 1.6f, "ChieftainAttack", null, "ChieftainSpecial", u =>
            {
                u.ai.specialEvery = 3;
                u.boss.enrageAt = 0.5f;
                u.boss.enrageSummons = new[] { units["Grunt"], units["Grunt"] };
            });
            Add("Bies", "Bies", Team.Enemies, 260, 2.26f, 1f, "BiesAttack", null, "BiesSpecial", u =>
            {
                u.ai.actionsPerTurn = 2;
                u.ai.specialEvery = 3;
            });
            Add("Wolf", "Wolf", Team.Enemies, 70, 1.1f, 1f, "WolfAttack", null, "WolfSpecial", u => u.ai.specialEvery = 3);
            return (units, visuals.ToArray());
        }
    }
}
