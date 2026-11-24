using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Meta;
using UnityEditor;

namespace Featherguard.EditorTools
{
    public static class PotionCatalog
    {
        sealed class Row
        {
            public string Id;
            public string Name;
            public string Description;
            public PotionTarget Target;
            public PotionEffect[] Effects;
            public (string Material, int Amount)[] Recipe;
            public string Land;
            public int AvailableAfter;
            public string Fight;
        }

        public static PotionConfig Build(Seeder seeder, IReadOnlyDictionary<string, StatusDefinition> statuses, MaterialConfig materials, WorldDefinition world)
        {
            StatusPotion Status(string id, int turns, int potency = 0, int times = 1) =>
                new() { application = new StatusApplication { status = statuses[id], turns = turns, potency = potency }, times = times };

            var rows = new[]
            {
                new Row { Id = "HealingDraught", Name = "Healing Draught", Description = "Heals 35% of max health.", Target = PotionTarget.Hero,
                    Effects = new PotionEffect[] { new HealPercentPotion { fraction = 0.35f } }, Recipe = new[] { ("MarshHerbs", 2), ("Flax", 1) } },
                new Row { Id = "CleansingBrew", Name = "Cleansing Brew", Description = "Removes every harmful status; no harmful status can be applied until the hero's next turn.", Target = PotionTarget.Hero,
                    Effects = new PotionEffect[] { new CleansePotion(), Status("Warded", 1) }, Recipe = new[] { ("Flax", 2), ("Amber", 1) },
                    Land = "RyeFields", AvailableAfter = 0, Fight = "Herbwife's Errand" },
                new Row { Id = "WolfsFury", Name = "Wolf's Fury", Description = "+40 Rage.", Target = PotionTarget.AllHeroes,
                    Effects = new PotionEffect[] { new RagePotion { amount = 40f } }, Recipe = new[] { ("WolfPelt", 1), ("Flax", 2) },
                    Land = "RyeFields", AvailableAfter = 0, Fight = "Abandoned Mill" },
                new Row { Id = "SheafWard", Name = "Sheaf Ward", Description = "Every hero gets a Shield of 15% of max health for 1 turn.", Target = PotionTarget.AllHeroes,
                    Effects = new PotionEffect[] { new ShieldPercentPotion { shield = statuses["Shield"], turns = 1, fraction = 0.15f } }, Recipe = new[] { ("LastSheaf", 1), ("Flax", 2) },
                    Land = "RyeFields", AvailableAfter = 0, Fight = "Scarecrow Field" },
                new Row { Id = "WispOil", Name = "Wisp Oil", Description = "Marks every enemy for 2 turns.", Target = PotionTarget.AllEnemies,
                    Effects = new PotionEffect[] { Status("Mark", 2) }, Recipe = new[] { ("WispInAJar", 1), ("Amber", 1) },
                    Land = "Marshes", AvailableAfter = 3, Fight = "Drowned Hut" },
                new Row { Id = "BogMist", Name = "Bog Mist", Description = "Weakens every enemy for 2 turns.", Target = PotionTarget.AllEnemies,
                    Effects = new PotionEffect[] { Status("Weaken", 2) }, Recipe = new[] { ("MarshHerbs", 2), ("Amber", 1) },
                    Land = "Marshes", AvailableAfter = 3, Fight = "Reed Maze" },
                new Row { Id = "FireFlask", Name = "Fire Flask", Description = "30 damage and 2 stacks of Burn.", Target = PotionTarget.Enemy,
                    Effects = new PotionEffect[] { new DamagePotion { power = 30 }, Status("Burn", 2, 5, 2) }, Recipe = new[] { ("Brimstone", 2), ("Flax", 1) },
                    Land = "BaldMountain", AvailableAfter = 6, Fight = "Smoking Crag" },
                new Row { Id = "StormDraught", Name = "Storm Draught", Description = "Immune to Stun for 3 turns.", Target = PotionTarget.Hero,
                    Effects = new PotionEffect[] { Status("Grounded", 3) }, Recipe = new[] { ("Thunderstone", 1), ("MarshHerbs", 1) },
                    Land = "BaldMountain", AvailableAfter = 6, Fight = "Thunder Ledge" },
                new Row { Id = "HeartwoodBrew", Name = "Heartwood Brew", Description = "+30% damage for 3 turns.", Target = PotionTarget.Hero,
                    Effects = new PotionEffect[] { Status("Might", 3) }, Recipe = new[] { ("OldOakHeartwood", 1), ("TinderFungus", 1) },
                    Land = "OldForest", AvailableAfter = 11, Fight = "Hollow Oak" },
                new Row { Id = "GromnicaLight", Name = "Gromnica Light", Description = "Removes Burn; no Burn can be applied for 3 turns.", Target = PotionTarget.AllHeroes,
                    Effects = new PotionEffect[] { new RemoveStatusPotion { status = statuses["Burn"] }, Status("Hallowed", 3) }, Recipe = new[] { ("GromnicaWax", 1), ("BoneAsh", 2) },
                    Land = "Nav", AvailableAfter = 15, Fight = "Grave Lights" },
                new Row { Id = "FirebirdDraught", Name = "Firebird Draught", Description = "A fallen hero returns with 40% of max health.", Target = PotionTarget.FallenHero,
                    Effects = new PotionEffect[] { new RevivePotion { fraction = 0.4f } }, Recipe = new[] { ("FirebirdFeather", 1), ("Brimstone", 2) },
                    Land = "RyeFields", AvailableAfter = 12, Fight = "Burnt Granary" },
                new Row { Id = "DeadWaterTincture", Name = "Dead Water Tincture", Description = "Heals to full and removes Bleed.", Target = PotionTarget.Hero,
                    Effects = new PotionEffect[] { new HealPercentPotion { fraction = 1f }, new RemoveStatusPotion { status = statuses["Bleed"] } }, Recipe = new[] { ("DeadWater", 1), ("MarshHerbs", 2) },
                    Land = "Marshes", AvailableAfter = 17, Fight = "Still Spring" },
                new Row { Id = "KupalaDewElixir", Name = "Kupala Dew Elixir", Description = "Every hero heals 30% of max health and loses every harmful status.", Target = PotionTarget.AllHeroes,
                    Effects = new PotionEffect[] { new HealPercentPotion { fraction = 0.3f }, new CleansePotion() }, Recipe = new[] { ("KupalaDew", 1), ("MarshHerbs", 2) },
                    Land = "BaldMountain", AvailableAfter = 21, Fight = "Serpent's Pass" },
                new Row { Id = "LivingWaterElixir", Name = "Living Water", Description = "Every fallen hero returns with 50% of max health; living heroes heal 50%.", Target = PotionTarget.AllHeroes,
                    Effects = new PotionEffect[] { new HealPercentPotion { fraction = 0.5f }, new RevivePotion { fraction = 0.5f } }, Recipe = new[] { ("LivingWater", 1), ("DeadWater", 1) },
                    Land = "OldForest", AvailableAfter = 23, Fight = "Leshy's Well" },
                new Row { Id = "FernFlowerElixir", Name = "Fern Flower Elixir", Description = "Fills the Rage meter; +30% damage for 2 turns.", Target = PotionTarget.AllHeroes,
                    Effects = new PotionEffect[] { new RagePotion { fill = true }, Status("Might", 2) }, Recipe = new[] { ("FernFlower", 1), ("KupalaDew", 1) },
                    Land = "Nav", AvailableAfter = 25, Fight = "Borderland Vigil" },
            };

            Seeder.EnsureFolder(ContentPaths.Potions);
            Seeder.EnsureFolder(ContentPaths.Recipes);
            var material = materials.materials.Where(m => m != null).ToDictionary(m => m.name);
            var recipes = new List<RecipeDefinition>();
            foreach (var row in rows)
            {
                var potion = seeder.Seed<PotionDefinition>($"{ContentPaths.Potions}/{row.Id}.asset", p =>
                {
                    p.displayName = row.Name;
                    p.description = row.Description;
                    p.target = row.Target;
                    p.effects = row.Effects;
                });
                recipes.Add(seeder.Seed<RecipeDefinition>($"{ContentPaths.Recipes}/{row.Id}.asset", r =>
                {
                    r.ingredients = row.Recipe.Select(i => new Ingredient { material = material[i.Material], amount = i.Amount }).ToArray();
                    r.knownFromStart = row.Land == null;
                }, r => r.potion = potion));
            }

            var encounters = Enumerable.Range(1, 9).Select(i => AssetDatabase.LoadAssetAtPath<EncounterDefinition>($"{ContentPaths.Encounters}/Battle0{i}.asset")).ToArray();
            int storyCount = world.storyMissions.Length;
            for (int i = 0; i < rows.Length; i++)
            {
                var row = rows[i];
                if (row.Land == null) continue;
                var land = world.lands.First(l => l != null && l.name == row.Land);
                var recipe = recipes[i];
                var fight = seeder.Seed<ActivityDefinition>($"{ContentPaths.Activities}/Schematic{row.Id}.asset", a =>
                {
                    a.displayName = row.Fight;
                    a.type = ActivityType.SingleFight;
                    a.threat = System.Math.Min(10, 1 + row.AvailableAfter * 9 / System.Math.Max(1, storyCount - 1));
                    a.availableAfter = row.AvailableAfter;
                    a.reward = RewardKind.Schematic;
                }, a =>
                {
                    a.land = land;
                    a.schematic = recipe;
                    if (a.encounter == null) a.encounter = encounters[System.Math.Min(row.AvailableAfter / 3, 7)];
                });
                if (land.sideActivities.Contains(fight)) continue;
                land.sideActivities = land.sideActivities.Append(fight).ToArray();
                EditorUtility.SetDirty(land);
            }

            return seeder.Seed<PotionConfig>($"{ContentPaths.Data}/PotionConfig.asset", null, c => c.recipes = recipes.ToArray());
        }
    }
}
