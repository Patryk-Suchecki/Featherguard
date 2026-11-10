using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Game;
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
            var statuses = StatusCatalog.Build(seeder, art);
            var (abilities, visuals) = AbilityCatalog.Build(seeder, statuses);
            var units = UnitCatalog.Build(seeder, abilities, art);
            var rules = seeder.Seed<BattleRules>($"{ContentPaths.Data}/BattleRules.asset", null);
            var presentation = seeder.Seed<BattlePresentationConfig>($"{ContentPaths.Data}/BattlePresentation.asset", null, p => p.abilities = visuals);
            var run = RunCatalog.Build(seeder, units, art);
            var meta = MetaCatalog.Build(seeder, art);

            var config = seeder.Seed<GameConfig>($"{ContentPaths.Data}/GameConfig.asset", c =>
            {
                c.heroes = new[] { units["Knight"], units["Archer"], units["Owl"] };
            }, c =>
            {
                c.rules = rules;
                c.presentation = presentation;
                c.run = run;
                c.meta = meta;
                c.flashMaterial = art.Flash;
                c.particleMaterial = art.Particle;
                c.ringMaterial = art.Ring;
                c.sounds = art.Sounds;
                c.ui = art.Ui;
                c.feedback ??= new Feel.FeedbackTuning();
            });
            AssetDatabase.SaveAssets();

            SceneBuilder.Build(config);
            MobileSettings.Apply();
            Debug.Log(resetToDefaults ? "Featherguard: data reset to defaults and scene rebuilt." : "Featherguard: scene rebuilt.");
        }
    }
}
