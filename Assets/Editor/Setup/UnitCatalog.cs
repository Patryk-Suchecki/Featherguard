using System.Collections.Generic;
using Featherguard.Battle;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class UnitCatalog
    {
        public static Dictionary<string, UnitDefinition> Build(Seeder seeder, IReadOnlyDictionary<string, AbilityDefinition> abilities, PresentationAssets art)
        {
            var units = new Dictionary<string, UnitDefinition>();

            void Add(string id, string name, Team team, int health, float height, float scale, string attack, string support, string special,
                System.Action<UnitDefinition> extra = null)
            {
                units[id] = seeder.Seed<UnitDefinition>($"{ContentPaths.Units}/{id}.asset", u =>
                {
                    u.displayName = name;
                    u.team = team;
                    u.maxHealth = health;
                    u.height = height;
                    u.modelScale = scale;
                    u.attack = abilities[attack];
                    u.support = support != null ? abilities[support] : null;
                    u.special = special != null ? abilities[special] : null;
                    u.ai = new EnemyAiProfile();
                    u.boss = new BossProfile();
                    extra?.Invoke(u);
                }, u =>
                {
                    u.model = AssetDatabase.LoadAssetAtPath<GameObject>($"{ContentPaths.Characters}/{id}.fbx");
                    u.portrait = art.Portrait(id);
                    u.animator = AnimatorBuilder.Build(id);
                });
            }

            Add("Knight", "Robin", Team.Heroes, 150, 1.4f, 1f, "KnightAttack", "KnightSupport", "KnightSpecial");
            Add("Archer", "Jay", Team.Heroes, 105, 1.4f, 1f, "ArcherAttack", "ArcherSupport", "ArcherSpecial");
            Add("Owl", "Owl", Team.Heroes, 115, 1.4f, 1f, "OwlAttack", "OwlSupport", "OwlSpecial");
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
            return units;
        }
    }
}
