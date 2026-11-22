using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;

namespace Featherguard.EditorTools
{
    public static class EncounterCatalog
    {
        public static void Build(Seeder seeder, IReadOnlyDictionary<string, UnitDefinition> units)
        {
            Encounters(seeder, units);
            TestEncounters(seeder, units);
        }

        static void Encounters(Seeder seeder, IReadOnlyDictionary<string, UnitDefinition> u)
        {
            var g = u["Grunt"];
            var s = u["Shaman"];
            var b = u["Brute"];
            var c = u["Chieftain"];
            var p = u["Spitter"];
            var t = u["Guard"];

            EncounterDefinition Add(string id, string name, float health, float damage, int gold, bool boss, params UnitDefinition[][] waves) =>
                seeder.Seed<EncounterDefinition>($"{ContentPaths.Encounters}/{id}.asset", e =>
                {
                    e.displayName = name;
                    e.enemyHealth = health;
                    e.enemyDamage = damage;
                    e.goldReward = gold;
                    e.boss = boss;
                    e.waves = waves.Select(w => new EncounterDefinition.Wave { enemies = w }).ToArray();
                });

            Add("Battle01", "Forest Edge", 1f, 1f, 10, false, new[] { g, g });
            Add("Battle02", "Woodland Path", 1f, 1f, 12, false, new[] { g, s, g });
            Add("Battle03", "Old Oak", 1f, 1f, 14, false, new[] { g, g }, new[] { s, g });
            Add("Battle04", "Windfall", 1.05f, 1f, 16, false, new[] { g, b, p });
            Add("Battle05", "Misty Ravine", 1.1f, 1.05f, 18, false, new[] { p, t, s }, new[] { g, b });
            Add("Battle06", "Marsh", 1.15f, 1.1f, 20, false, new[] { p, s, p }, new[] { g, t, g });
            Add("Battle07", "Stone Ford", 1.2f, 1.1f, 22, false, new[] { b, t, b }, new[] { s, p });
            Add("Battle08", "Lizard Camp", 1.3f, 1.15f, 25, false, new[] { t, s, b }, new[] { p, b, g });
            Add("Battle09", "Chieftain", 1.3f, 1.15f, 40, true, new[] { g, t, g }, new[] { p, c, s });
        }

        static void TestEncounters(Seeder seeder, IReadOnlyDictionary<string, UnitDefinition> u)
        {
            void Add(string id, string name, params UnitDefinition[] enemies) =>
                seeder.Seed<EncounterDefinition>($"{ContentPaths.Encounters}/{id}.asset", e =>
                {
                    e.displayName = name;
                    e.goldReward = 10;
                    e.waves = new[] { new EncounterDefinition.Wave { enemies = enemies } };
                });

            Add("BiesTest", "Bies (test)", u["Bies"]);
            Add("BiesPack", "Bies and Wolves (test)", u["Wolf"], u["Bies"], u["Wolf"]);
        }
    }
}
