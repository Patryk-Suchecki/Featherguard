using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle.Presentation;
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
    public class StrongholdBootstrap : MonoBehaviour
    {
        const float HoldTime = 0.45f;
        const float FlightTime = 0.7f;

        [SerializeField] GameConfig config;
        [SerializeField] Camera sceneCamera;
        [SerializeField] Transform campfire;
        [SerializeField] Transform campView;
        [SerializeField] CampSeat[] seats;

        readonly List<RaycastResult> uiHits = new();
        StrongholdHud hud;
        CampHud campHud;
        HeroRoster roster;
        StrongholdPlace campPlace;
        Pose overview;
        Coroutine flight;
        bool inCamp;
        bool leaving;
        bool pressing;
        bool holdHandled;
        float pressStart;
        Vector2 pressPosition;

        void Start()
        {
            GameClock.Paused = false;
            DynamicGI.UpdateEnvironment();
            var feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, sceneCamera, null, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            overview = new Pose(sceneCamera.transform.position, sceneCamera.transform.rotation);

            roster = SavedGame.Roster(config);
            SeatHeroes();

            var places = FindObjectsByType<StrongholdPlace>();
            campPlace = places.FirstOrDefault(p => p.Kind == StrongholdPlaceKind.Camp);
            hud = gameObject.AddComponent<StrongholdHud>();
            hud.Init(sceneCamera, places.Select(p => (p.LabelAnchor, p.DisplayName)), () => hud.ComingSoon("Settings"));
            campHud = gameObject.AddComponent<CampHud>();
            campHud.Init(sceneCamera, seats.Select(s => s.LabelAnchor).ToList(), LeaveCamp, () => hud.ComingSoon("Storage"), () => hud.ComingSoon("Potions"));
            ShowSeats();
        }

        void Update()
        {
            if (leaving || flight != null) return;
            var pointer = Pointer.current;
            if (pointer == null) return;
            var position = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                pressing = !OverUi(position);
                holdHandled = false;
                pressStart = Time.unscaledTime;
                pressPosition = position;
            }
            if (!pressing) return;

            if (inCamp && !holdHandled && Time.unscaledTime - pressStart >= HoldTime)
            {
                holdHandled = true;
                var held = SeatAt(pressPosition);
                if (held != null && roster.AtSeat(held.Index) != null)
                    campHud.ShowDetails(roster.AtSeat(held.Index), roster.InParty(roster.AtSeat(held.Index)));
            }
            if (!pointer.press.wasReleasedThisFrame) return;
            pressing = false;
            if (holdHandled) return;
            if (inCamp) TapInCamp(position);
            else TapInStronghold(position);
        }

        void TapInStronghold(Vector2 position)
        {
            if (!Physics.Raycast(sceneCamera.ScreenPointToRay(position), out var hit, 200f)) return;
            if (hit.collider.GetComponentInParent<CampSeat>() != null)
            {
                EnterCamp();
                return;
            }
            var place = hit.collider.GetComponentInParent<StrongholdPlace>();
            if (place != null) Select(place);
        }

        void TapInCamp(Vector2 position)
        {
            var seat = SeatAt(position);
            if (seat == null) return;
            var hero = roster.AtSeat(seat.Index);
            if (hero == null)
            {
                hud.Toast("This seat waits for a hero who has not joined yet");
                return;
            }
            switch (roster.Toggle(hero))
            {
                case PartyChange.PartyFull:
                    hud.Toast("The party is full: take someone out first");
                    return;
                case PartyChange.Added:
                    FeedbackHub.Ui(SfxId.Buff);
                    break;
                default:
                    FeedbackHub.Ui(SfxId.Click);
                    break;
            }
            ShowSeats();
        }

        void Select(StrongholdPlace place)
        {
            place.Pulse();
            switch (place.Kind)
            {
                case StrongholdPlaceKind.Camp:
                    EnterCamp();
                    return;
                case StrongholdPlaceKind.Embark when !roster.IsPartyComplete:
                    hud.Toast($"Pick {HeroRoster.PartySize} heroes at the campfire first");
                    return;
                case StrongholdPlaceKind.Embark:
                    leaving = true;
                    FeedbackHub.Ui(SfxId.Rage);
                    SceneManager.LoadScene(GameScenes.Battle);
                    return;
                default:
                    hud.ComingSoon(place.DisplayName);
                    return;
            }
        }

        void EnterCamp()
        {
            FeedbackHub.Ui(SfxId.Whoosh);
            inCamp = true;
            if (campPlace != null) campPlace.GetComponent<Collider>().enabled = false;
            hud.SetOverviewVisible(false);
            ShowSeats();
            flight = StartCoroutine(Fly(new Pose(campView.position, campView.rotation), () => campHud.SetVisible(true)));
        }

        void LeaveCamp()
        {
            if (flight != null) return;
            FeedbackHub.Ui(SfxId.Whoosh);
            inCamp = false;
            campHud.SetVisible(false);
            if (campPlace != null) campPlace.GetComponent<Collider>().enabled = true;
            ShowSeats();
            flight = StartCoroutine(Fly(overview, () => hud.SetOverviewVisible(true)));
        }

        IEnumerator Fly(Pose target, System.Action arrived)
        {
            var camera = sceneCamera.transform;
            var start = new Pose(camera.position, camera.rotation);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / FlightTime)
            {
                float k = Mathf.SmoothStep(0f, 1f, t);
                camera.SetPositionAndRotation(Vector3.Lerp(start.position, target.position, k), Quaternion.Slerp(start.rotation, target.rotation, k));
                yield return null;
            }
            camera.SetPositionAndRotation(target.position, target.rotation);
            flight = null;
            arrived();
        }

        void SeatHeroes()
        {
            var visuals = new BattleVisuals(config.presentation).Units;
            foreach (var seat in seats)
            {
                var hero = roster.AtSeat(seat.Index);
                if (hero == null) continue;
                var look = visuals.Of(hero);
                if (look.model == null) continue;
                var facing = campfire.position - seat.transform.position;
                facing.y = 0f;
                var model = Instantiate(look.model, seat.transform.position, Quaternion.LookRotation(facing) * Quaternion.Euler(0f, 180f, 0f), seat.transform);
                model.transform.localScale = Vector3.one * look.modelScale;
                var animator = model.GetComponent<Animator>();
                if (animator == null) animator = model.AddComponent<Animator>();
                animator.runtimeAnimatorController = look.animator;
                animator.applyRootMotion = false;
                seat.Hero = model;
            }
        }

        void ShowSeats()
        {
            foreach (var seat in seats)
            {
                var hero = roster.AtSeat(seat.Index);
                if (seat.Hero != null)
                    seat.Hero.SetActive(inCamp || roster.InParty(hero));
            }
            campHud.Refresh(seats.Select(SeatOf).ToList(), roster.Party.Count, HeroRoster.PartySize);
        }

        (string, SeatState) SeatOf(CampSeat seat)
        {
            var hero = roster.AtSeat(seat.Index);
            if (hero == null) return ("", SeatState.Empty);
            return (hero.displayName, roster.InParty(hero) ? SeatState.InParty : SeatState.Resting);
        }

        CampSeat SeatAt(Vector2 position) =>
            Physics.Raycast(sceneCamera.ScreenPointToRay(position), out var hit, 200f) ? hit.collider.GetComponentInParent<CampSeat>() : null;

        bool OverUi(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            return uiHits.Count > 0;
        }
    }
}
