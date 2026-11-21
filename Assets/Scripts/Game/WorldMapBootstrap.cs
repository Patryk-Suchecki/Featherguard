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
        [SerializeField] WorldMapLand[] lands;

        WorldProgress world;
        bool leaving;

        void Start()
        {
            GameClock.Paused = false;
            DynamicGI.UpdateEnvironment();
            var feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, sceneCamera, null, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            world = SavedGame.World(config);
            if (world.Chosen != null) world.ClearChoice();

            foreach (var land in lands)
                land.SetOpen(world.IsOpen(land.Land));
            gameObject.AddComponent<WorldMapHud>().Init(sceneCamera, lands.Select(DescribeLand).ToList(), world.StoryWon, world.StoryCount, Back, Prototype);
        }

        MapLand DescribeLand(WorldMapLand land)
        {
            bool open = world.IsOpen(land.Land);
            return new MapLand
            {
                Name = land.Land.displayName,
                Open = open,
                StoryWon = world.StoryWonIn(land.Land),
                StoryTotal = world.StoryCountIn(land.Land),
                LabelAnchor = land.LabelAnchor,
                ActivitiesAnchor = land.ActivitiesAnchor,
                Activities = open ? world.ActivitiesIn(land.Land).Select(DescribeActivity).ToList() : new List<MapActivity>(),
            };
        }

        MapActivity DescribeActivity(ActivityDefinition activity) => new()
        {
            Name = activity.displayName,
            Type = activity.type,
            Story = world.IsStoryMission(activity),
            Rating = world.Rate(activity),
            Reward = activity.reward,
            Select = () => Prepare(activity),
        };

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
