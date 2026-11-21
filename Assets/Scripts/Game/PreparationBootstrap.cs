using System.Linq;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Feedback;
using Featherguard.Meta;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public class PreparationBootstrap : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Camera sceneCamera;
        [SerializeField] Transform[] heroSpots;
        [SerializeField] Transform[] enemySpots;

        WorldProgress world;
        HeroRoster roster;
        PreparationHud hud;
        bool leaving;

        void Start()
        {
            GameClock.Paused = false;
            DynamicGI.UpdateEnvironment();
            var feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, sceneCamera, null, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            DebugTools.Install(config);
            world = SavedGame.World(config);
            var activity = world.Chosen;
            if (activity == null)
            {
                leaving = true;
                SceneManager.LoadScene(GameScenes.WorldMap);
                return;
            }
            roster = SavedGame.Roster(config);
            PlaceUnits(activity);

            var rating = world.Rate(activity);
            hud = gameObject.AddComponent<PreparationHud>();
            hud.Init(new PreparationInfo
            {
                Name = activity.displayName,
                Story = world.IsStoryMission(activity),
                Type = activity.type,
                Waves = ActivityBattle.Waves(activity),
                Land = activity.land.displayName,
                Law = activity.land.law,
                Rating = rating,
                Daring = world.IsDaring(rating),
                Reward = activity.reward,
                Party = roster.Party.Select(h => h.displayName).ToList(),
                PartyComplete = roster.IsPartyComplete,
            }, Back, ChangeParty, () => hud.Toast("Potions: coming soon"), Fight);
        }

        void PlaceUnits(ActivityDefinition activity)
        {
            var units = new BattleVisuals(config.presentation).Units;
            var party = roster.Party;
            for (int i = 0; i < party.Count && i < heroSpots.Length; i++)
                UnitModels.Place(units.Of(party[i]), heroSpots[i], Vector3.right);
            var wave = activity.encounter.waves.FirstOrDefault();
            if (wave == null) return;
            for (int i = 0; i < wave.enemies.Length && i < enemySpots.Length; i++)
                UnitModels.Place(units.Of(wave.enemies[i]), enemySpots[i], Vector3.left);
        }

        void Back()
        {
            if (leaving) return;
            world.ClearChoice();
            Leave(GameScenes.WorldMap, SfxId.Whoosh);
        }

        void ChangeParty()
        {
            if (leaving) return;
            Leave(GameScenes.Stronghold, SfxId.Whoosh);
        }

        void Fight()
        {
            if (leaving) return;
            if (!roster.IsPartyComplete)
            {
                hud.Toast($"Pick {HeroRoster.PartySize} heroes in the Camp first");
                return;
            }
            Leave(GameScenes.Battle, SfxId.Rage);
        }

        void Leave(string scene, SfxId sound)
        {
            leaving = true;
            FeedbackHub.Ui(sound);
            SceneManager.LoadScene(scene);
        }
    }
}
