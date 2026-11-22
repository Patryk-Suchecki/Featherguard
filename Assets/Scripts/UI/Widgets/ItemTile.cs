using UnityEngine;

namespace Featherguard.UI
{
    public static class ItemTile
    {
        static readonly Color BadgeColor = new(0.05f, 0.04f, 0.05f, 0.9f);

        public static RectTransform Create(RectTransform parent, string sprite, int count, Vector2 position, float size, Vector2? anchor = null)
        {
            var a = anchor ?? new Vector2(0.5f, 0.5f);
            var tile = Ui.Rect(sprite, parent, a, new Vector2(0.5f, 0.5f), position, Vector2.one * size);
            var icon = Ui.Image(Ui.Stretch("Icon", tile), Color.white);
            icon.sprite = Ui.Sprite(sprite);
            icon.preserveAspect = true;
            if (count > 0)
            {
                var badge = Ui.Rect("Count", tile, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(4f, -4f), new Vector2(size * 0.5f, size * 0.3f));
                Ui.Panel(badge, BadgeColor);
                Ui.Label(Ui.Stretch("Label", badge), $"×{count}", Mathf.RoundToInt(size * 0.2f), TextAnchor.MiddleCenter, Color.white);
            }
            return tile;
        }
    }
}
