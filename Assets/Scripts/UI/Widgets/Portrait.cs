using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public static class Portrait
    {
        static readonly Color Backdrop = new(0.25f, 0.4f, 0.6f, 1f);

        public static RectTransform Create(RectTransform parent, Sprite sprite, Vector2 position, float size, Vector2 anchor)
        {
            var frame = Ui.Rect("Portrait", parent, anchor, anchor, position, Vector2.one * size);
            Ui.Image(frame, Backdrop).sprite = Ui.Sprite("Circle");
            frame.gameObject.AddComponent<Mask>();
            Ui.Image(Ui.Stretch("Image", frame), Color.white).sprite = sprite;
            return frame;
        }
    }
}
