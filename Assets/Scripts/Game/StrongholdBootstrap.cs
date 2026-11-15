using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Feedback;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public class StrongholdBootstrap : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Camera sceneCamera;
        [SerializeField] Transform campfire;
        [SerializeField] Transform[] seats;

        readonly List<RaycastResult> uiHits = new();
        StrongholdHud hud;
        bool leaving;

        void Start()
        {
            GameClock.Paused = false;
            DynamicGI.UpdateEnvironment();
            var feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, sceneCamera, null, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;

            SeatParty();
            var places = FindObjectsByType<StrongholdPlace>();
            hud = gameObject.AddComponent<StrongholdHud>();
            hud.Init(sceneCamera, places.Select(p => (p.LabelAnchor, p.DisplayName)), () => hud.ComingSoon("Settings"));
        }

        void Update()
        {
            if (leaving) return;
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;
            var position = pointer.position.ReadValue();
            if (OverUi(position)) return;
            if (!Physics.Raycast(sceneCamera.ScreenPointToRay(position), out var hit, 200f)) return;
            var place = hit.collider.GetComponentInParent<StrongholdPlace>();
            if (place != null) Select(place);
        }

        void Select(StrongholdPlace place)
        {
            place.Pulse();
            if (place.Kind != StrongholdPlaceKind.Embark)
            {
                hud.ComingSoon(place.DisplayName);
                return;
            }
            leaving = true;
            FeedbackHub.Ui(SfxId.Rage);
            SceneManager.LoadScene(GameScenes.Battle);
        }

        void SeatParty()
        {
            var visuals = new BattleVisuals(config.presentation).Units;
            for (int i = 0; i < seats.Length && i < config.heroes.Length; i++)
            {
                var look = visuals.Of(config.heroes[i]);
                if (look.model == null) continue;
                var seat = seats[i];
                var facing = campfire.position - seat.position;
                facing.y = 0f;
                var model = Instantiate(look.model, seat.position, Quaternion.LookRotation(facing) * Quaternion.Euler(0f, 180f, 0f), seat);
                model.transform.localScale = Vector3.one * look.modelScale;
                var animator = model.GetComponent<Animator>();
                if (animator == null) animator = model.AddComponent<Animator>();
                animator.runtimeAnimatorController = look.animator;
                animator.applyRootMotion = false;
            }
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
