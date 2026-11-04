using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Feel;
using Featherguard.Run;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class SlotMachineView : MonoBehaviour
    {
        const float CellHeight = 140f;
        const float ReelWidth = 230f;
        const int StripLength = 34;

        static readonly Color FrameColor = new(0.2f, 0.17f, 0.2f, 1f);
        static readonly Color TensionColor = new(1f, 0.55f, 0.2f, 1f);

        readonly List<RectTransform> strips = new();
        readonly List<Image> frames = new();
        readonly List<SlotSymbol[]> contents = new();
        SlotSymbol[] symbols;

        public static SlotMachineView Create(RectTransform parent, Vector2 position, SlotSymbol[] available)
        {
            var root = Ui.Centered("SlotMachine", parent, position, new Vector2(ReelWidth * 3f + 100f, CellHeight * 3f + 60f));
            Ui.Panel(root, new Color(0.55f, 0.16f, 0.1f, 1f));
            Ui.Panel(Ui.Stretch("Inner", root, 12f), new Color(0.08f, 0.05f, 0.06f, 1f));
            var machine = root.gameObject.AddComponent<SlotMachineView>();
            machine.symbols = available;
            for (int i = 0; i < 3; i++)
            {
                var window = Ui.Centered($"Reel{i}", root, new Vector2((i - 1) * (ReelWidth + 20f), 0f), new Vector2(ReelWidth, CellHeight * 3f));
                machine.frames.Add(Ui.Panel(window, FrameColor));
                window.gameObject.AddComponent<RectMask2D>();
                var strip = Ui.Centered("Strip", window, Vector2.zero, new Vector2(ReelWidth, CellHeight));
                machine.strips.Add(strip);
                machine.contents.Add(null);
                machine.Fill(i, machine.RandomStrip());
                SetOffset(strip, 1f);
            }
            var line = Ui.Centered("Payline", root, Vector2.zero, new Vector2(ReelWidth * 3f + 70f, CellHeight + 8f));
            Ui.Image(line, new Color(1f, 0.85f, 0.2f, 0.18f));
            return machine;
        }

        SlotSymbol[] RandomStrip() => Enumerable.Range(0, StripLength).Select(_ => symbols[Random.Range(0, symbols.Length)]).ToArray();

        void Fill(int reel, SlotSymbol[] strip)
        {
            var parent = strips[reel];
            for (int i = parent.childCount - 1; i >= 0; i--)
                Destroy(parent.GetChild(i).gameObject);
            parent.DetachChildren();
            contents[reel] = strip;
            for (int i = 0; i < strip.Length; i++)
            {
                var symbol = strip[i];
                var cell = Ui.Centered(symbol.id, parent, new Vector2(0f, -i * CellHeight), new Vector2(ReelWidth - 20f, CellHeight - 14f));
                Ui.Panel(cell, new Color(symbol.tint.r * 0.35f + 0.08f, symbol.tint.g * 0.35f + 0.08f, symbol.tint.b * 0.35f + 0.1f, 1f));
                var icon = Ui.Icon(cell, "Card", new Vector2(0f, 12f), 96f);
                icon.sprite = symbol.icon;
                Ui.Label(Ui.Rect("Label", cell, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(ReelWidth - 20f, 26f)),
                    symbol.label, 22, TextAnchor.LowerCenter, Color.white);
            }
        }

        static void SetOffset(RectTransform strip, float cells) => strip.anchoredPosition = new Vector2(0f, cells * CellHeight);

        SlotSymbol Visible(int reel)
        {
            var strip = contents[reel];
            int index = Mathf.Clamp(Mathf.RoundToInt(strips[reel].anchoredPosition.y / CellHeight), 0, strip.Length - 1);
            return strip[index];
        }

        public IEnumerator Spin(SlotSymbol[] result)
        {
            bool tension = result[0] == result[1];
            var routines = new List<Coroutine>();
            for (int i = 0; i < 3; i++)
            {
                var strip = RandomStrip();
                int target = StripLength - 3;
                strip[target] = result[i];
                if (i == 2 && tension && result[2] != result[0])
                    strip[target + 1] = result[0];
                strip[1] = Visible(i);
                Fill(i, strip);
                SetOffset(strips[i], 1f);
                float duration = 1.1f + i * 0.45f + (i == 2 && tension ? 1.2f : 0f);
                routines.Add(StartCoroutine(SpinReel(i, target, duration, i == 2 && tension)));
            }
            foreach (var routine in routines)
                yield return routine;
        }

        IEnumerator SpinReel(int index, int target, float duration, bool tension)
        {
            var strip = strips[index];
            float overshoot = target + 0.22f;
            int lastCell = 1;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                float eased = 1f - Mathf.Pow(1f - t, tension ? 5f : 3.5f);
                float offset = Mathf.Lerp(1f, overshoot, eased);
                SetOffset(strip, offset);
                int cell = Mathf.FloorToInt(offset);
                if (cell != lastCell)
                {
                    lastCell = cell;
                    FeedbackHub.Ui(SfxId.Tick, tension && t > 0.6f ? 0.9f : 0.45f);
                }
                if (tension)
                    frames[index].color = Color.Lerp(FrameColor, TensionColor, Mathf.PingPong(Time.unscaledTime * 6f, 1f));
                yield return null;
            }
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.14f)
            {
                SetOffset(strip, Mathf.Lerp(overshoot, target, Mathf.SmoothStep(0f, 1f, t)));
                yield return null;
            }
            SetOffset(strip, target);
            frames[index].color = FrameColor;
            FeedbackHub.Ui(SfxId.Click);
            StartCoroutine(Punch((RectTransform)strip.parent));
        }

        public void Highlight(int index, Color color) => frames[index].color = color;

        static IEnumerator Punch(RectTransform rect)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.18f)
            {
                rect.localScale = Vector3.one * (1f + Mathf.Sin(t * Mathf.PI) * 0.06f);
                yield return null;
            }
            rect.localScale = Vector3.one;
        }
    }
}
