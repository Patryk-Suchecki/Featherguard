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
    public class StrongholdBootstrap : MonoBehaviour
    {
        const float HoldTime = 0.45f;
        const float FlightTime = 0.7f;

        [SerializeField] GameConfig config;
        [SerializeField] Camera sceneCamera;
        [SerializeField] Transform campfire;
        [SerializeField] Transform campView;
        [SerializeField] CampSeat[] seats;

        StrongholdHud hud;
        CampHud campHud;
        HeroRoster roster;
        StrongholdPlace campPlace;
        IScreenGestures gestures;
        CameraFlight flight;
        Pose overview;
        bool inCamp;
        bool fromPreparation;
        bool leaving;

        void Start()
        {
            GameClock.Paused = false;
            DynamicGI.UpdateEnvironment();
            var feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, sceneCamera, null, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            DebugTools.Install(config);
            overview = new Pose(sceneCamera.transform.position, sceneCamera.transform.rotation);
            gestures = new PointerGestures(HoldTime);
            flight = gameObject.AddComponent<CameraFlight>();
            flight.Init(sceneCamera.transform, FlightTime);

            roster = SavedGame.Roster(config);
            SeatHeroes();

            var places = FindObjectsByType<StrongholdPlace>();
            campPlace = places.FirstOrDefault(p => p.Kind == StrongholdPlaceKind.Camp);
            hud = gameObject.AddComponent<StrongholdHud>();
            hud.Init(sceneCamera, places.Select(p => (p.LabelAnchor, p.DisplayName)), () => hud.ComingSoon("Settings"));
            campHud = gameObject.AddComponent<CampHud>();
            campHud.Init(sceneCamera, seats.Select(s => s.LabelAnchor).ToList(), LeaveCamp, () => hud.ComingSoon("Storage"), () => hud.ComingSoon("Potions"));
            ShowSeats();
            fromPreparation = SavedGame.World(config).Chosen != null;
            if (fromPreparation) EnterCamp(instant: true);
        }

        void Update()
        {
            if (leaving || flight.Flying) return;
            var gesture = gestures.Read(holds: inCamp);
            switch (gesture.Kind)
            {
                case GestureKind.Hold:
                    HoldInCamp(gesture.Position);
                    return;
                case GestureKind.Tap when inCamp:
                    TapInCamp(gesture.Position);
                    return;
                case GestureKind.Tap:
                    TapInStronghold(gesture.Position);
                    return;
            }
        }

        void HoldInCamp(Vector2 position)
        {
            var seat = SeatAt(position);
            var hero = seat != null ? roster.AtSeat(seat.Index) : null;
            if (hero != null) campHud.ShowDetails(hero, roster.InParty(hero));
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
                    SceneManager.LoadScene(GameScenes.WorldMap);
                    return;
                default:
                    hud.ComingSoon(place.DisplayName);
                    return;
            }
        }

        void EnterCamp(bool instant = false)
        {
            inCamp = true;
            if (campPlace != null) campPlace.GetComponent<Collider>().enabled = false;
            hud.SetOverviewVisible(false);
            ShowSeats();
            var view = new Pose(campView.position, campView.rotation);
            if (instant)
            {
                flight.Jump(view);
                campHud.SetVisible(true);
                return;
            }
            FeedbackHub.Ui(SfxId.Whoosh);
            flight.Fly(view, () => campHud.SetVisible(true));
        }

        void LeaveCamp()
        {
            if (flight.Flying) return;
            FeedbackHub.Ui(SfxId.Whoosh);
            if (fromPreparation)
            {
                leaving = true;
                SceneManager.LoadScene(GameScenes.Preparation);
                return;
            }
            inCamp = false;
            campHud.SetVisible(false);
            if (campPlace != null) campPlace.GetComponent<Collider>().enabled = true;
            ShowSeats();
            flight.Fly(overview, () => hud.SetOverviewVisible(true));
        }

        void SeatHeroes()
        {
            var visuals = new BattleVisuals(config.presentation).Units;
            foreach (var seat in seats)
            {
                var hero = roster.AtSeat(seat.Index);
                if (hero == null) continue;
                var model = UnitModels.Place(visuals.Of(hero), seat.transform, campfire.position - seat.transform.position);
                if (model != null) seat.Hero = model;
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
    }
}
