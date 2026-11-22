using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Meta;
using UnityEditor;

namespace Featherguard.EditorTools
{
    public static class WorldCatalog
    {
        static readonly (string Id, string Name, string Law)[] LandRows =
        {
            ("RyeFields", "Rye Fields", "Noon Heat: every 3rd turn, characters without a Shield lose part of their HP."),
            ("Marshes", "Marshes", "Depths: some enemies submerge; wisps switch targets."),
            ("BaldMountain", "Bald Mountain", "Storm: lightning strikes a random character; dodge it in a QTE."),
            ("OldForest", "Old Forest", "Misleading: the party's formation shuffles; enemies stay hidden until Marked."),
            ("Nav", "Nav", "Chill of Nav: healing is halved; the fallen rise as ghosts unless finished with fire."),
        };

        static readonly string[] StoryOrder =
        {
            "RyeFields", "RyeFields", "Marshes", "RyeFields", "Marshes",
            "BaldMountain", "Marshes", "BaldMountain", "RyeFields", "BaldMountain",
            "OldForest", "RyeFields", "Marshes", "OldForest", "Nav",
            "BaldMountain", "Marshes", "OldForest", "Nav", "OldForest",
            "BaldMountain", "Nav", "OldForest", "Nav", "Nav",
        };

        static readonly Dictionary<string, string> Bosses = new()
        {
            ["RyeFields"] = "Lady Midday",
            ["Marshes"] = "Water Maiden",
            ["BaldMountain"] = "Dragoness",
            ["OldForest"] = "Lady Leshy",
            ["Nav"] = "Boss of Nav",
        };

        public static WorldDefinition Build(Seeder seeder)
        {
            var encounters = Enumerable.Range(1, 9).Select(i => Encounter($"Battle0{i}")).ToArray();
            var elite = Encounter("BiesPack") ?? encounters[3];
            var lands = LandRows.ToDictionary(row => row.Id, row => seeder.Seed<LandDefinition>($"{ContentPaths.Lands}/{row.Id}.asset", land =>
            {
                land.displayName = row.Name;
                land.law = row.Law;
            }));

            var story = new List<ActivityDefinition>();
            for (int i = 0; i < StoryOrder.Length; i++)
            {
                string landId = StoryOrder[i];
                bool boss = StoryOrder.Skip(i + 1).All(id => id != landId);
                int threat = 1 + i * 9 / (StoryOrder.Length - 1);
                var encounter = boss ? encounters[8] : encounters[System.Math.Min(i / 3, 7)];
                string name = boss ? Bosses[landId] : $"Story {i + 1}";
                story.Add(Activity($"Story{i + 1:00}", name, boss ? ActivityType.BossFight : ActivityType.SingleFight, lands[landId], encounter, threat, 0, boss ? RewardKind.Loot : RewardKind.Exp));
            }

            int firstStory(string landId) => System.Array.IndexOf(StoryOrder, landId);
            foreach (var (landId, land) in lands)
            {
                int opens = firstStory(landId);
                int baseThreat = 1 + opens * 9 / (StoryOrder.Length - 1);
                int bossAfter = System.Array.LastIndexOf(StoryOrder, landId) + 1;
                if (land.sideActivities != null && land.sideActivities.Length > 0)
                {
                    FillRewards(land.sideActivities);
                    continue;
                }
                land.sideActivities = new[]
                {
                    Activity($"{landId}Single", $"{land.displayName} Skirmish", ActivityType.SingleFight, land, encounters[System.Math.Min(opens / 3, 7)], baseThreat, opens, SideReward(ActivityType.SingleFight)),
                    Activity($"{landId}Waves", $"{land.displayName} Ambush", ActivityType.WavesFight, land, encounters[System.Math.Min(opens / 3 + 2, 7)], baseThreat + 1, opens, SideReward(ActivityType.WavesFight)),
                    Activity($"{landId}Puzzle", $"{land.displayName} Riddle", ActivityType.PuzzleFight, land, encounters[System.Math.Min(opens / 3 + 1, 7)], baseThreat, opens, SideReward(ActivityType.PuzzleFight)),
                    Activity($"{landId}Elite", $"{land.displayName} Elite", ActivityType.EliteFight, land, elite, baseThreat + 2, opens + 1, SideReward(ActivityType.EliteFight)),
                    Activity($"{landId}Boss", $"{Bosses[landId]} Returns", ActivityType.BossFight, land, encounters[8], System.Math.Min(10, baseThreat + 3), bossAfter, SideReward(ActivityType.BossFight)),
                };
                EditorUtility.SetDirty(land);
            }

            ActivityDefinition Activity(string id, string name, ActivityType type, LandDefinition land, EncounterDefinition encounter, int threat, int availableAfter, RewardKind reward) =>
                seeder.Seed<ActivityDefinition>($"{ContentPaths.Activities}/{id}.asset", a =>
                {
                    a.displayName = name;
                    a.type = type;
                    a.threat = threat;
                    a.availableAfter = availableAfter;
                    a.reward = reward;
                }, a =>
                {
                    a.land = land;
                    if (a.encounter == null) a.encounter = encounter;
                    if (a.reward == RewardKind.None) a.reward = reward;
                });

            return seeder.Seed<WorldDefinition>($"{ContentPaths.World}/WorldDefinition.asset", world =>
            {
                world.lands = LandRows.Select(row => lands[row.Id]).ToArray();
                world.storyMissions = story.ToArray();
            });
        }

        static EncounterDefinition Encounter(string id) => AssetDatabase.LoadAssetAtPath<EncounterDefinition>($"{ContentPaths.Encounters}/{id}.asset");

        static RewardKind SideReward(ActivityType type) => type switch
        {
            ActivityType.SingleFight => RewardKind.Gold,
            ActivityType.WavesFight => RewardKind.Exp,
            ActivityType.PuzzleFight => RewardKind.Exp,
            ActivityType.EliteFight => RewardKind.Materials,
            _ => RewardKind.Loot,
        };

        static void FillRewards(IEnumerable<ActivityDefinition> activities)
        {
            foreach (var activity in activities.Where(a => a != null && (a.reward == RewardKind.None || a.reward == RewardKind.Schematic && a.schematic == null)))
            {
                activity.reward = SideReward(activity.type);
                EditorUtility.SetDirty(activity);
            }
        }
    }
}
