using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Feedback;
using Featherguard.Game;
using Featherguard.Meta;
using UnityEditor;
using UnityEngine;

namespace Featherguard.EditorTools
{
    public static class SetupMenu
    {
        [MenuItem("Featherguard/Rebuild Battle Scene")]
        public static void Run() => Build(false);

        [MenuItem("Featherguard/Reset Data To Defaults")]
        public static void ResetData()
        {
            if (Application.isBatchMode || EditorUtility.DisplayDialog("Featherguard", "Overwrite all game data assets with default values?", "Reset", "Cancel"))
                Build(true);
        }

        public static void Build(bool resetToDefaults)
        {
            AssetDatabase.Refresh();
            foreach (var guid in AssetDatabase.FindAssets("t:Model", new[] { "Assets/Art" }))
                AssetDatabase.ImportAsset(AssetDatabase.GUIDToAssetPath(guid), ImportAssetOptions.ForceUpdate);

            var seeder = new Seeder(resetToDefaults);
            var art = PresentationBuilder.Build(seeder);
            var (statuses, statusVisuals) = StatusCatalog.Build(seeder, art);
            var (abilities, visuals) = AbilityCatalog.Build(seeder, statuses);
            var (units, unitVisuals) = UnitCatalog.Build(seeder, abilities, art);
            var rules = seeder.Seed<BattleRules>($"{ContentPaths.Data}/BattleRules.asset", null);
            var presentation = seeder.Seed<BattlePresentationConfig>($"{ContentPaths.Data}/BattlePresentationConfig.asset", null, p =>
            {
                p.abilities = visuals;
                p.units = unitVisuals;
                p.statuses = statusVisuals;
            });
            EncounterCatalog.Build(seeder, units);
            var world = WorldCatalog.Build(seeder);
            var materials = MaterialCatalog.Build(seeder, world);
            var potions = PotionCatalog.Build(seeder, statuses, materials, world);
            var meta = seeder.Seed<MetaConfig>($"{ContentPaths.Data}/MetaConfig.asset", null);

            var config = seeder.Seed<GameConfig>($"{ContentPaths.Data}/GameConfig.asset", c =>
            {
                c.heroes = new[] { units["Warrior"], units["ThornWarden"], units["Healer"], units["Thundercaller"] };
            }, c =>
            {
                c.rules = rules;
                c.presentation = presentation;
                c.meta = meta;
                c.world = world;
                c.materials = materials;
                c.potions = potions;
                c.flashMaterial = art.Flash;
                c.particleMaterial = art.Particle;
                c.ringMaterial = art.Ring;
                c.toonMaterial = art.Toon;
                c.sounds = art.Sounds;
                c.ui = art.Ui;
                c.feedback ??= new FeedbackTuning();
            });
            AssetDatabase.SaveAssets();

            StrongholdSceneBuilder.Build(LoadConfig());
            WorldMapSceneBuilder.Build(LoadConfig());
            PreparationSceneBuilder.Build(LoadConfig());
            SceneBuilder.Build(LoadConfig());
            MobileSettings.Apply();
            Debug.Log(resetToDefaults ? "Featherguard: data reset to defaults and scene rebuilt." : "Featherguard: scene rebuilt.");
        }

        static GameConfig LoadConfig() => AssetDatabase.LoadAssetAtPath<GameConfig>($"{ContentPaths.Data}/GameConfig.asset");
    }
}
