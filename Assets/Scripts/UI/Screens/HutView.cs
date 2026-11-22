using System;
using System.Linq;
using Featherguard.Feedback;
using Featherguard.Meta;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class HutView
    {
        const int Columns = 5;
        const float Tile = 118f;
        const float CellWidth = 156f;
        const float CellHeight = 196f;
        const float GridLeft = -660f;
        const float GridTop = 190f;
        const float DetailsX = 430f;

        static readonly Color PanelColor = new(0.09f, 0.08f, 0.1f, 1f);
        static readonly Color CellColor = new(0.14f, 0.13f, 0.16f, 1f);
        static readonly Color SelectedColor = new(0.55f, 0.45f, 0.2f, 1f);
        static readonly Color LockedIconColor = new(0.1f, 0.1f, 0.12f, 1f);
        static readonly Color ButtonColor = new(0.16f, 0.17f, 0.22f, 1f);
        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color MutedColor = new(0.66f, 0.66f, 0.7f);
        static readonly Color ShortColor = new(1f, 0.45f, 0.38f);

        readonly RectTransform panel;
        readonly RecipeBook book;
        readonly PotionBag bag;
        readonly MaterialStorage storage;
        readonly PotionBrewer brewer;
        RectTransform body;
        RecipeDefinition selected;
        int count = 1;

        HutView(RectTransform panel, RecipeBook book, PotionBag bag, MaterialStorage storage, PotionBrewer brewer)
        {
            this.panel = panel;
            this.book = book;
            this.bag = bag;
            this.storage = storage;
            this.brewer = brewer;
            selected = book.All.FirstOrDefault();
        }

        public static HutView Build(RectTransform content, RecipeBook book, PotionBag bag, MaterialStorage storage, PotionBrewer brewer, Action onClose)
        {
            var panel = Ui.Centered("Hut", content, Vector2.zero, new Vector2(1640f, 920f));
            Ui.Panel(panel, PanelColor, true);
            Ui.Label(Ui.Rect("Title", panel, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -28f), new Vector2(900f, 70f)),
                "Wise Woman's Hut", 60, TextAnchor.MiddleCenter, Ui.GoldColor);
            var back = Ui.Rect("Back", panel, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(28f, -28f), new Vector2(200f, 76f));
            Ui.Button(back, ButtonColor, () => onClose());
            Ui.Label(Ui.Stretch("Label", back), "Back", 32, TextAnchor.MiddleCenter, Color.white);
            var view = new HutView(panel, book, bag, storage, brewer);
            view.Redraw();
            return view;
        }

        void Redraw()
        {
            if (body != null) UnityEngine.Object.Destroy(body.gameObject);
            body = Ui.Stretch("Body", panel);
            for (int i = 0; i < book.All.Count; i++)
                Cell(book.All[i], new Vector2(GridLeft + i % Columns * CellWidth, GridTop - i / Columns * CellHeight));
            if (selected != null) Details(selected);
        }

        void Cell(RecipeDefinition recipe, Vector2 position)
        {
            bool known = book.Knows(recipe);
            var cell = Ui.Centered(recipe.name, body, position, new Vector2(CellWidth - 10f, CellHeight - 10f));
            Ui.Button(cell, recipe == selected ? SelectedColor : CellColor, () => Select(recipe));
            var tile = ItemTile.Create(cell, recipe.potion.name, bag.Count(recipe.potion), new Vector2(0f, 22f), Tile);
            if (!known) tile.GetComponentInChildren<Image>().color = LockedIconColor;
            Ui.Wrapped(Ui.Centered("Name", cell, new Vector2(0f, -68f), new Vector2(CellWidth - 14f, 50f)),
                known ? recipe.potion.displayName : "Locked", 20, TextAnchor.MiddleCenter, known ? TextColor : MutedColor);
        }

        void Details(RecipeDefinition recipe)
        {
            var potion = recipe.potion;
            bool known = book.Knows(recipe);
            Text(known ? potion.displayName : "Unknown recipe", 46, Ui.GoldColor, 300f, 60f);
            if (!known)
            {
                Text("Win its schematic fight on the world map to learn this recipe.", 30, MutedColor, 200f, 110f);
                return;
            }
            Text(potion.description, 28, Color.white, 222f, 96f);
            Text($"Owned: {bag.Count(potion)}", 30, TextColor, 150f, 44f);

            int brewing = Mathf.Max(1, count);
            for (int i = 0; i < recipe.ingredients.Length; i++)
            {
                var ingredient = recipe.ingredients[i];
                float y = 70f - i * 96f;
                MaterialTile.Create(body, ingredient.material, 0, new Vector2(DetailsX - 250f, y), 84f);
                int owned = storage.Count(ingredient.material);
                int needed = ingredient.amount * brewing;
                Ui.Label(Ui.Centered("Ingredient", body, new Vector2(DetailsX + 60f, y), new Vector2(520f, 50f)),
                    $"{ingredient.material.displayName}   {owned} / {needed}", 30, TextAnchor.MiddleLeft, owned >= needed ? TextColor : ShortColor);
            }

            float row = -320f;
            Step("−", new Vector2(DetailsX - 260f, row), () => SetCount(count - 1));
            Ui.Label(Ui.Centered("Count", body, new Vector2(DetailsX - 170f, row), new Vector2(90f, 80f)), count.ToString(), 44, TextAnchor.MiddleCenter, Color.white);
            Step("+", new Vector2(DetailsX - 80f, row), () => SetCount(count + 1));
            var brew = Ui.Centered("Brew", body, new Vector2(DetailsX + 190f, row), new Vector2(280f, 96f));
            Ui.Button(brew, Ui.GoColor, () => Brew(recipe)).interactable = brewer.CanBrew(recipe, count);
            Ui.Label(Ui.Stretch("Label", brew), "Brew", 42, TextAnchor.MiddleCenter, Color.white);
        }

        void Text(string text, int size, Color color, float y, float height) =>
            Ui.Wrapped(Ui.Centered("Text", body, new Vector2(DetailsX, y), new Vector2(680f, height)), text, size, TextAnchor.MiddleLeft, color);

        void Step(string label, Vector2 position, Action onClick)
        {
            var rect = Ui.Centered(label, body, position, new Vector2(84f, 84f));
            Ui.Button(rect, ButtonColor, () => onClick());
            Ui.Label(Ui.Stretch("Label", rect), label, 48, TextAnchor.MiddleCenter, Color.white);
        }

        void Select(RecipeDefinition recipe)
        {
            FeedbackHub.Ui(SfxId.Click);
            selected = recipe;
            count = 1;
            Redraw();
        }

        void SetCount(int value)
        {
            FeedbackHub.Ui(SfxId.Click);
            count = Mathf.Clamp(value, 1, brewer.MaxBrew);
            Redraw();
        }

        void Brew(RecipeDefinition recipe)
        {
            if (!brewer.Brew(recipe, count)) return;
            FeedbackHub.Ui(SfxId.Buff);
            count = 1;
            Redraw();
        }
    }
}
