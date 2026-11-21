using System;
using System.Collections.Generic;
using Featherguard.Meta;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class MapLand
    {
        public string Name;
        public string Law;
        public bool Open;
        public int StoryWon;
        public int StoryTotal;
        public IReadOnlyList<MapActivity> Activities;
    }

    public sealed class MapActivity
    {
        public string Name;
        public ActivityType Type;
        public int Waves;
        public bool Story;
        public ThreatRating Rating;
        public bool Daring;
        public RewardKind Reward;
        public Action Select;
    }

    public class WorldMapHud : MonoBehaviour
    {
        const float ColumnWidth = 340f;
        const float ColumnGap = 20f;
        const float ColumnTop = -170f;
        const float ColumnHeight = 770f;
        const float TileHeight = 180f;
        const float TileGap = 12f;
        const float TilesTop = -200f;

        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color MutedColor = new(0.62f, 0.63f, 0.68f);
        static readonly Color LockedColor = new(0.45f, 0.46f, 0.5f);
        static readonly Color StoryColor = new(1f, 0.8f, 0.42f);
        static readonly Color BackdropColor = new(0.03f, 0.035f, 0.055f, 1f);
        static readonly Color ColumnColor = new(0.07f, 0.075f, 0.11f, 0.95f);
        static readonly Color LockedColumnColor = new(0.05f, 0.05f, 0.07f, 0.95f);
        static readonly Color TileColor = new(0.13f, 0.14f, 0.19f, 1f);
        static readonly Color StoryTileColor = new(0.3f, 0.22f, 0.08f, 1f);
        static readonly Color ButtonColor = new(0.1f, 0.1f, 0.13f, 0.92f);

        public void Init(IReadOnlyList<MapLand> lands, int storyWon, int storyCount, Action onBack, Action onPrototype)
        {
            var root = ScreenCanvas.Create("WorldMapHUD");
            Ui.Image(Ui.Stretch("Backdrop", root), BackdropColor);
            var safe = SafeArea.Create(root);

            Ui.Label(Ui.Rect("Title", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -20f), new Vector2(900f, 70f)),
                "World Map", 60, TextAnchor.MiddleCenter, StoryColor);
            Ui.Label(Ui.Rect("Story", safe, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -92f), new Vector2(900f, 44f)),
                storyWon >= storyCount ? "The story is complete" : $"Story missions {storyWon} / {storyCount}", 32, TextAnchor.MiddleCenter, Color.white);

            float first = -(lands.Count - 1) * (ColumnWidth + ColumnGap) / 2f;
            for (int i = 0; i < lands.Count; i++)
                Column(safe, lands[i], first + i * (ColumnWidth + ColumnGap));

            Button(safe, "Back", new Vector2(0f, 1f), new Vector2(24f, -24f), onBack);
            Button(safe, "Prototype run", new Vector2(0f, 0f), new Vector2(24f, 24f), onPrototype, 320f);
        }

        static void Column(RectTransform parent, MapLand land, float x)
        {
            var column = Ui.Rect(land.Name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(x, ColumnTop), new Vector2(ColumnWidth, ColumnHeight));
            Ui.Panel(column, land.Open ? ColumnColor : LockedColumnColor);
            Text(column, "Name", land.Name, 36, land.Open ? TextColor : LockedColor, -18f, 46f);

            if (!land.Open)
            {
                Text(column, "State", "Locked", 28, LockedColor, -74f, 36f);
                Ui.Wrapped(Ui.Rect("Hint", column, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -124f), new Vector2(ColumnWidth - 40f, 80f)),
                    "Not reached by the story yet", 22, TextAnchor.UpperCenter, LockedColor);
                return;
            }

            Text(column, "State", $"Story {land.StoryWon} / {land.StoryTotal}", 24, StoryColor, -66f, 32f);
            Ui.Wrapped(Ui.Rect("Law", column, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(ColumnWidth - 36f, 120f)),
                string.IsNullOrEmpty(land.Law) ? "No Law" : land.Law, 19, TextAnchor.UpperCenter, MutedColor);

            if (land.Activities.Count == 0)
            {
                Text(column, "Empty", "Nothing here for now", 22, MutedColor, TilesTop - 20f, 32f);
                return;
            }
            for (int i = 0; i < land.Activities.Count; i++)
                Tile(column, land.Activities[i], TilesTop - i * (TileHeight + TileGap));
        }

        static void Tile(RectTransform column, MapActivity activity, float y)
        {
            var tile = Ui.Rect(activity.Name, column, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(ColumnWidth - 32f, TileHeight));
            Ui.Button(tile, activity.Story ? StoryTileColor : TileColor, () => activity.Select());
            Text(tile, "Kind", activity.Story ? "Main quest" : ActivityText.Describe(activity.Type, activity.Waves), 20, activity.Story ? StoryColor : MutedColor, -10f, 26f);
            Ui.Wrapped(Ui.Rect("Name", tile, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -36f), new Vector2(ColumnWidth - 56f, 56f)),
                activity.Name, 27, TextAnchor.MiddleCenter, activity.Story ? StoryColor : Color.white);
            Text(tile, "Rating", activity.Daring ? $"{activity.Rating}   ·   Daring +50%" : activity.Rating.ToString(), 22,
                ActivityText.RatingColor(activity.Rating), -94f, 28f);
            if (activity.Reward != RewardKind.None) Reward(tile, activity.Reward, -132f);
        }

        static void Reward(RectTransform tile, RewardKind reward, float y)
        {
            var row = Ui.Rect("Reward", tile, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(200f, 34f));
            Ui.Icon(row, ActivityText.RewardIcon(reward), new Vector2(16f, 0f), 30f, anchor: new Vector2(0f, 0.5f));
            Ui.Label(Ui.Rect("Name", row, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(40f, 0f), new Vector2(160f, 34f)),
                ActivityText.RewardName(reward), 24, TextAnchor.MiddleLeft, Ui.GoldColor);
        }

        static void Text(RectTransform parent, string name, string text, int size, Color color, float y, float height) =>
            Ui.Label(Ui.Rect(name, parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, y), new Vector2(parent.sizeDelta.x - 24f, height)),
                text, size, TextAnchor.MiddleCenter, color);

        static void Button(RectTransform parent, string label, Vector2 anchor, Vector2 position, Action onClick, float width = 240f)
        {
            var rect = Ui.Rect(label, parent, anchor, anchor, position, new Vector2(width, 84f));
            Ui.Button(rect, ButtonColor, () => onClick());
            Ui.Label(Ui.Stretch("Label", rect), label, 34, TextAnchor.MiddleCenter, TextColor);
        }
    }
}
