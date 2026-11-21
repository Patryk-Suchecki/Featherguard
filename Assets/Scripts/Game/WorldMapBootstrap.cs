using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;
using Featherguard.Feedback;
using Featherguard.Meta;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public class WorldMapBootstrap : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Camera sceneCamera;
        [SerializeField] WorldMapLand[] lands;
        [Tooltip("One marker per ActivityType, in the enum's order.")]
        [SerializeField] GameObject[] markerTemplates;
        [SerializeField] GameObject mainQuestTemplate;

        readonly List<RaycastResult> uiHits = new();
        WorldProgress world;
        WorldMapHud hud;
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
            if (world.Chosen != null) world.ClearChoice();

            var markers = new List<MapMarkerLabel>();
            foreach (var land in lands)
            {
                bool open = world.IsOpen(land.Land);
                land.SetOpen(open);
                if (!open) continue;
                var activities = world.ActivitiesIn(land.Land);
                for (int i = 0; i < activities.Count && i < land.SlotCount; i++)
                    markers.Add(SpawnMarker(activities[i], land.Slot(i)));
            }

            hud = gameObject.AddComponent<WorldMapHud>();
            hud.Init(sceneCamera,
                lands.Select(l => new MapLandLabel(l.LabelAnchor, l.Land.displayName, world.IsOpen(l.Land), world.StoryWonIn(l.Land), world.StoryCountIn(l.Land))).ToList(),
                markers,
                world.StoryWon, world.StoryCount,
                Back, Prototype);
        }

        void Update()
        {
            if (leaving) return;
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;
            var position = pointer.position.ReadValue();
            if (OverUi(position)) return;
            if (!Physics.Raycast(sceneCamera.ScreenPointToRay(position), out var hit, 300f)) return;

            var marker = hit.collider.GetComponentInParent<MapMarker>();
            if (marker != null)
            {
                Prepare(marker.Activity);
                return;
            }
            var land = hit.collider.GetComponentInParent<WorldMapLand>();
            if (land == null) return;
            hud.Toast(world.IsOpen(land.Land)
                ? $"{land.Land.displayName}: {land.Land.law}"
                : $"{land.Land.displayName} is locked: the story has not reached it yet");
        }

        MapMarkerLabel SpawnMarker(ActivityDefinition activity, Transform slot)
        {
            bool story = world.IsStoryMission(activity);
            var template = story ? mainQuestTemplate : markerTemplates[(int)activity.type];
            var marker = Instantiate(template, slot.position, slot.rotation, slot);
            marker.SetActive(true);
            marker.AddComponent<MapMarker>().Bind(activity);
            var rating = world.Rate(activity);
            return new MapMarkerLabel(marker.transform, activity.displayName, story, rating, world.IsDaring(rating));
        }

        void Prepare(ActivityDefinition activity)
        {
            if (!world.Choose(activity)) return;
            leaving = true;
            FeedbackHub.Ui(SfxId.Whoosh);
            SceneManager.LoadScene(GameScenes.Preparation);
        }

        void Back()
        {
            leaving = true;
            FeedbackHub.Ui(SfxId.Whoosh);
            SceneManager.LoadScene(GameScenes.Stronghold);
        }

        void Prototype()
        {
            leaving = true;
            FeedbackHub.Ui(SfxId.Rage);
            SceneManager.LoadScene(GameScenes.Battle);
        }

        bool OverUi(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            return uiHits.Count > 0;
        }
    }
}
