using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public enum SlotSymbol { Coin, Heart, Card, Rage, Skull, Gem }

    public class SlotMachine : MonoBehaviour
    {
        const float CellHeight = 140f;
        const float ReelWidth = 230f;
        const int StripLength = 34;

        readonly List<RectTransform> strips = new();
        readonly List<Image> frames = new();

        public static string Name(SlotSymbol symbol) => symbol switch
        {
            SlotSymbol.Coin => "ZŁOTO",
            SlotSymbol.Heart => "SERCE",
            SlotSymbol.Card => "KARTA",
            SlotSymbol.Rage => "FURIA",
            SlotSymbol.Skull => "PECH",
            _ => "KLEJNOT",
        };

        public static Color Tint(SlotSymbol symbol) => symbol switch
        {
            SlotSymbol.Coin => new Color(1f, 0.78f, 0.15f),
            SlotSymbol.Heart => new Color(0.95f, 0.3f, 0.35f),
            SlotSymbol.Card => new Color(0.3f, 0.55f, 1f),
            SlotSymbol.Rage => new Color(1f, 0.45f, 0.12f),
            SlotSymbol.Skull => new Color(0.35f, 0.3f, 0.4f),
            _ => new Color(0.3f, 0.95f, 0.95f),
        };

        public static SlotMachine Create(RectTransform parent, Vector2 position)
        {
            var root = Ui.Rect("SlotMachine", parent, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(ReelWidth * 3f + 100f, CellHeight * 3f + 60f));
            Ui.Panel(root, new Color(0.55f, 0.16f, 0.1f, 1f));
            var inner = Ui.Stretch("Inner", root, 12f);
            Ui.Panel(inner, new Color(0.08f, 0.05f, 0.06f, 1f));
            var machine = root.gameObject.AddComponent<SlotMachine>();
            for (int i = 0; i < 3; i++)
            {
                var window = Ui.Rect($"Reel{i}", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2((i - 1) * (ReelWidth + 20f), 0f), new Vector2(ReelWidth, CellHeight * 3f));
                machine.frames.Add(Ui.Image(window, new Color(0.2f, 0.17f, 0.2f, 1f)));
                machine.frames[i].sprite = Ui.Sprite("RoundRect");
                machine.frames[i].type = Image.Type.Sliced;
                window.gameObject.AddComponent<RectMask2D>();
                var strip = Ui.Rect("Strip", window, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ReelWidth, CellHeight));
                machine.strips.Add(strip);
                machine.Fill(strip, Enumerable.Range(0, StripLength).Select(_ => RandomSymbol()).ToArray());
                SetOffset(strip, 1f);
            }
            var line = Ui.Rect("Payline", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(ReelWidth * 3f + 70f, CellHeight + 8f));
            var lineImage = Ui.Image(line, new Color(1f, 0.85f, 0.2f, 0.18f));
            lineImage.raycastTarget = false;
            return machine;
        }

        static SlotSymbol RandomSymbol() => (SlotSymbol)Random.Range(0, RunState.GemsUnlocked ? 6 : 5);

        void Fill(RectTransform strip, SlotSymbol[] symbols)
        {
            for (int i = strip.childCount - 1; i >= 0; i--)
                Destroy(strip.GetChild(i).gameObject);
            strip.DetachChildren();
            for (int i = 0; i < symbols.Length; i++)
            {
                var cell = Ui.Rect(symbols[i].ToString(), strip, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -i * CellHeight), new Vector2(ReelWidth - 20f, CellHeight - 14f));
                var tint = Tint(symbols[i]);
                Ui.Panel(cell, new Color(tint.r * 0.35f + 0.08f, tint.g * 0.35f + 0.08f, tint.b * 0.35f + 0.1f, 1f));
                Ui.Icon(cell, symbols[i].ToString(), new Vector2(0f, 12f), 96f);
                Ui.Label(Ui.Rect("Label", cell, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 4f), new Vector2(ReelWidth - 20f, 26f)),
                    Name(symbols[i]), 22, TextAnchor.LowerCenter, Color.white);
            }
        }

        static void SetOffset(RectTransform strip, float cells) => strip.anchoredPosition = new Vector2(0f, cells * CellHeight);

        public static SlotSymbol[] Roll()
        {
            float match = Mathf.Min(0.6f, 0.3f + RunState.Luck * 0.04f);
            var weights = new Dictionary<SlotSymbol, float>
            {
                [SlotSymbol.Coin] = 30f,
                [SlotSymbol.Heart] = 20f,
                [SlotSymbol.Card] = 18f + RunState.Luck,
                [SlotSymbol.Rage] = 14f,
                [SlotSymbol.Skull] = Mathf.Max(3f, 13f - RunState.Luck * 2f),
            };
            if (RunState.GemsUnlocked)
                weights[SlotSymbol.Gem] = 6f + RunState.Luck;

            SlotSymbol Pick()
            {
                float roll = Random.value * weights.Values.Sum();
                foreach (var pair in weights)
                {
                    roll -= pair.Value;
                    if (roll <= 0f) return pair.Key;
                }
                return SlotSymbol.Coin;
            }

            var first = Pick();
            var second = Random.value < match ? first : Pick();
            var third = Random.value < match ? second : Pick();
            return new[] { first, second, third };
        }

        public IEnumerator Spin(SlotSymbol[] result)
        {
            bool tension = result[0] == result[1];
            var routines = new List<Coroutine>();
            for (int i = 0; i < 3; i++)
            {
                var symbols = Enumerable.Range(0, StripLength).Select(_ => RandomSymbol()).ToArray();
                int target = StripLength - 3;
                symbols[target] = result[i];
                if (i == 2 && tension && result[2] != result[0])
                    symbols[target + 1] = result[0];
                var strip = strips[i];
                var visible = strip.childCount > 1 ? CurrentSymbol(strip) : symbols[0];
                symbols[1] = visible;
                Fill(strip, symbols);
                SetOffset(strip, 1f);
                float duration = 1.1f + i * 0.45f + (i == 2 && tension ? 1.2f : 0f);
                routines.Add(StartCoroutine(SpinReel(i, target, duration, i == 2 && tension)));
            }
            foreach (var routine in routines)
                yield return routine;
        }

        SlotSymbol CurrentSymbol(RectTransform strip)
        {
            int index = Mathf.Clamp(Mathf.RoundToInt(strip.anchoredPosition.y / CellHeight), 0, strip.childCount - 1);
            return System.Enum.Parse<SlotSymbol>(strip.GetChild(index).name);
        }

        IEnumerator SpinReel(int index, int target, float duration, bool tension)
        {
            var strip = strips[index];
            float start = 1f;
            float overshoot = target + 0.22f;
            int lastCell = 1;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                float eased = tension ? 1f - Mathf.Pow(1f - t, 5f) : 1f - Mathf.Pow(1f - t, 3.5f);
                float offset = Mathf.Lerp(start, overshoot, eased);
                SetOffset(strip, offset);
                int cell = Mathf.FloorToInt(offset);
                if (cell != lastCell)
                {
                    lastCell = cell;
                    HitFeedback.Instance.Play(SfxId.Tick, tension && t > 0.6f ? 0.9f : 0.45f, 0.15f);
                }
                if (tension)
                    frames[index].color = Color.Lerp(new Color(0.2f, 0.17f, 0.2f), new Color(1f, 0.55f, 0.2f), Mathf.PingPong(Time.unscaledTime * 6f, 1f));
                yield return null;
            }
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.14f)
            {
                SetOffset(strip, Mathf.Lerp(overshoot, target, Mathf.SmoothStep(0f, 1f, t)));
                yield return null;
            }
            SetOffset(strip, target);
            frames[index].color = new Color(0.2f, 0.17f, 0.2f);
            HitFeedback.Instance.Play(SfxId.Click, 1f, 0.05f);
            StartCoroutine(Punch(strip.parent as RectTransform));
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
