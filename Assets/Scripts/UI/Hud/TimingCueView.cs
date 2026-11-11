using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class TimingCueView
    {
        const float TargetSize = 170f;
        const float RingGrowth = 2.2f;
        static readonly Color TargetColor = new(1f, 0.78f, 0.2f, 0.95f);
        static readonly Color RingColor = new(1f, 1f, 1f, 0.95f);
        static readonly Color BlockColor = new(0.55f, 0.85f, 1f);

        readonly Camera view;
        readonly RectTransform root;
        readonly RectTransform ring;
        readonly GameObject attack;
        readonly GameObject block;

        public TimingCueView(RectTransform parent, Camera view)
        {
            this.view = view;
            root = Ui.Centered("TimingCue", parent, Vector2.zero, Vector2.one * TargetSize);

            attack = Ui.Stretch("Attack", root).gameObject;
            Ui.Image(Ui.Stretch("Target", attack.transform), TargetColor).sprite = Ui.Sprite("Ring");
            ring = Ui.Stretch("Ring", attack.transform);
            Ui.Image(ring, RingColor).sprite = Ui.Sprite("Ring");

            var blockRect = Ui.Centered("Block", root, Vector2.zero, new Vector2(520f, 140f));
            Ui.Label(blockRect, "BLOCK!", 96, TextAnchor.MiddleCenter, BlockColor);
            block = blockRect.gameObject;

            Hide();
        }

        public void ShowAttackRing(Vector3 world, float closing)
        {
            Place(world);
            attack.SetActive(true);
            block.SetActive(false);
            ring.localScale = Vector3.one * (1f + RingGrowth * closing);
        }

        public void ShowBlockCue(Vector3 world)
        {
            Place(world);
            attack.SetActive(false);
            block.SetActive(true);
        }

        public void Hide() => root.gameObject.SetActive(false);

        void Place(Vector3 world)
        {
            root.gameObject.SetActive(true);
            root.position = view.WorldToScreenPoint(world);
        }
    }
}
