using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;
using Featherguard.Feedback;
using Featherguard.Meta;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public class WorldMapBootstrap : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Camera sceneCamera;

        WorldProgress world;
        bool leaving;

        void Start()
        {
            GameClock.Paused = false;
            var feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, sceneCamera, null, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            world = SavedGame.World(config);
            if (world.Chosen != null) world.ClearChoice();

            var lands = config.world.lands.Select(DescribeLand).ToList();
            gameObject.AddComponent<WorldMapHud>().Init(lands, world.StoryWon, world.StoryCount, Back, Prototype);
        }

        MapLand DescribeLand(LandDefinition land)
        {
            bool open = world.IsOpen(land);
            return new MapLand
            {
                Name = land.displayName,
                Law = land.law,
                Open = open,
                StoryWon = world.StoryWonIn(land),
                StoryTotal = world.StoryCountIn(land),
                Activities = open ? world.ActivitiesIn(land).Select(DescribeActivity).ToList() : new List<MapActivity>(),
            };
        }

        MapActivity DescribeActivity(ActivityDefinition activity)
        {
            var rating = world.Rate(activity);
            return new MapActivity
            {
                Name = activity.displayName,
                Type = activity.type,
                Waves = ActivityBattle.Waves(activity),
                Story = world.IsStoryMission(activity),
                Rating = rating,
                Daring = world.IsDaring(rating),
                Reward = activity.reward,
                Select = () => Prepare(activity),
            };
        }

        void Prepare(ActivityDefinition activity)
        {
            if (leaving || !world.Choose(activity)) return;
            leaving = true;
            FeedbackHub.Ui(SfxId.Whoosh);
            SceneManager.LoadScene(GameScenes.Preparation);
        }

        void Back()
        {
            if (leaving) return;
            leaving = true;
            FeedbackHub.Ui(SfxId.Whoosh);
            SceneManager.LoadScene(GameScenes.Stronghold);
        }

        void Prototype()
        {
            if (leaving) return;
            leaving = true;
            FeedbackHub.Ui(SfxId.Rage);
            SceneManager.LoadScene(GameScenes.Battle);
        }
    }
}
