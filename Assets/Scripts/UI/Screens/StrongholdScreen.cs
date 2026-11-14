using System.Collections;
using Featherguard.Feedback;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class StrongholdScreen
    {
        const float ToastHold = 1.1f;
        const float ToastFade = 0.4f;

        static readonly Color BuildingColor = new(0.17f, 0.13f, 0.1f, 0.95f);
        static readonly Color ComingSoonColor = new(0.75f, 0.72f, 0.66f);
        static readonly Vector2 BuildingSize = new(380f, 230f);

        static readonly Place[] Buildings =
        {
            new("Forge", "Sword", new Vector2(-660f, 80f)),
            new("Wise Woman's Hut", "Heal", new Vector2(-250f, 300f)),
            new("Sacred Oak", "Clover", new Vector2(250f, 300f)),
            new("Tavern", "Card", new Vector2(660f, 80f)),
            new("Shop", "Shop", new Vector2(-660f, -260f)),
        };

        readonly BattleHud hud;
        Overlay overlay;
        Text toast;
        Coroutine toastRoutine;
        bool embarked;

        StrongholdScreen(BattleHud hud) => this.hud = hud;

        public static IEnumerator Show(BattleHud hud)
        {
            var screen = new StrongholdScreen(hud);
            hud.SetHudVisible(false);
            screen.Build();
            yield return new WaitUntil(() => screen.embarked);
            screen.Close();
            hud.SetHudVisible(true);
        }

        void Build()
        {
            overlay = hud.OpenOverlay("Stronghold", 0.82f);
            var root = overlay.Content;

            foreach (var building in Buildings)
                Location(root, building, BuildingSize);
            Location(root, new Place("Camp", "Campfire", new Vector2(0f, -170f)), new Vector2(420f, 300f));

            var settings = Ui.Rect("Settings", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -24f), new Vector2(240f, 84f));
            Ui.Button(settings, Ui.NeutralColor, () => ComingSoon("Settings"));
            Ui.Label(Ui.Stretch("Label", settings), "Settings", 34, TextAnchor.MiddleCenter, Color.white);

            var embark = Ui.Rect("Embark", root, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-40f, 40f), new Vector2(460f, 140f));
            Ui.Button(embark, Ui.GoColor, Embark);
            Ui.Icon(embark, "Flag", new Vector2(80f, 0f), 88f, anchor: new Vector2(0f, 0.5f));
            Ui.Label(Ui.Stretch("Label", embark), "Embark", 56, TextAnchor.MiddleCenter, Color.white).rectTransform.offsetMin = new Vector2(110f, 0f);
            Pulser.Attach(embark, 5f, 0.04f);

            toast = Ui.Label(Ui.Rect("Toast", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -40f), new Vector2(1200f, 70f)),
                "", 46, TextAnchor.MiddleCenter, Ui.GoldColor);
        }

        void Location(RectTransform root, Place place, Vector2 size)
        {
            var rect = Ui.Centered(place.Name, root, place.Position, size);
            Ui.Button(rect, BuildingColor, () => ComingSoon(place.Name));
            Ui.Icon(rect, place.Icon, new Vector2(0f, 30f), size.y * 0.45f, ComingSoonColor);
            Ui.Label(Ui.Rect("Name", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 52f), new Vector2(size.x, 46f)),
                place.Name, 36, TextAnchor.MiddleCenter, Color.white);
            Ui.Label(Ui.Rect("State", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(size.x, 32f)),
                "Coming soon", 24, TextAnchor.MiddleCenter, ComingSoonColor);
        }

        void ComingSoon(string place)
        {
            FeedbackHub.Ui(SfxId.Click);
            if (toastRoutine != null) hud.StopCoroutine(toastRoutine);
            toastRoutine = hud.StartCoroutine(ShowToast($"{place}: coming soon"));
        }

        IEnumerator ShowToast(string text)
        {
            toast.text = text;
            toast.color = Ui.GoldColor;
            yield return new WaitForSecondsRealtime(ToastHold);
            yield return Tweens.Fade(toast, ToastFade);
        }

        void Embark()
        {
            FeedbackHub.Ui(SfxId.Rage);
            embarked = true;
        }

        void Close()
        {
            if (toastRoutine != null) hud.StopCoroutine(toastRoutine);
            overlay.Close();
        }

        readonly struct Place
        {
            public readonly string Name;
            public readonly string Icon;
            public readonly Vector2 Position;

            public Place(string name, string icon, Vector2 position)
            {
                Name = name;
                Icon = icon;
                Position = position;
            }
        }
    }
}
