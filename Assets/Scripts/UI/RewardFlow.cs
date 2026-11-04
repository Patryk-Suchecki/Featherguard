using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Battle;
using Featherguard.Feel;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public class RewardFlow : MonoBehaviour
    {
        enum Node { Normal, Elite, Shop, Chest, Campfire }

        static readonly Vector2 ButtonSize = new(460f, 120f);

        BattleHud hud;
        BattleManager battle;
        RectTransform dim;
        RectTransform root;
        int choice = -1;
        int extraPicks;
        Rarity extraMinimum = Rarity.Common;
        int rerolls;

        public static void Play(BattleHud hud, BattleManager battle)
        {
            var flow = hud.gameObject.AddComponent<RewardFlow>();
            flow.hud = hud;
            flow.battle = battle;
            hud.SetHudVisible(false);
            flow.StartCoroutine(flow.Sequence());
        }

        IEnumerator Sequence()
        {
            yield return Slots();
            yield return DoubleOrNothing();
            yield return CardPick(RunState.CurrentElite ? Rarity.Rare : Rarity.Common, "WYBIERZ KARTĘ");
            while (extraPicks > 0)
            {
                extraPicks--;
                yield return CardPick(extraMinimum, "DODATKOWA KARTA!");
            }
            yield return PathChoice();
            battle.ContinueRun();
        }

        RectTransform Screen(string title, string subtitle)
        {
            if (dim != null) Destroy(dim.gameObject);
            root = hud.Overlay("Rewards", out dim);
            dim.SetSiblingIndex(hud.Safe.GetSiblingIndex() + 1);
            BattleHud.Title(root, title, subtitle);
            BattleHud.PartyStrip(root);
            hud.RefreshGold();
            choice = -1;
            return root;
        }

        void Refresh(string title, string subtitle)
        {
            var keep = root.Cast<Transform>().Where(t => t.name != "Party" && t.name != "Stats" && t.name != "Title" && t.name != "Subtitle").ToList();
            foreach (var t in root.Cast<Transform>().Except(keep).ToList())
                Destroy(t.gameObject);
            BattleHud.Title(root, title, subtitle);
            BattleHud.PartyStrip(root);
            hud.RefreshGold();
        }

        RectTransform Button(string label, Vector2 position, Color color, int value, string icon = null, Vector2? size = null)
        {
            return Ui.TextButton(root, label, position, size ?? ButtonSize, color, () =>
            {
                HitFeedback.Instance.Play(SfxId.Click);
                choice = value;
            }, icon);
        }

        IEnumerator WaitChoice()
        {
            choice = -1;
            yield return new WaitUntil(() => choice >= 0);
        }

        Text BigText(string text, Color color, float y)
        {
            var label = Ui.Label(Ui.Centered("Result", root, new Vector2(0f, y), new Vector2(1400f, 110f)), text, 70, TextAnchor.MiddleCenter, color);
            StartCoroutine(Pop(label.rectTransform, 0.25f, 1.8f));
            return label;
        }

        IEnumerator Slots()
        {
            string elite = RunState.CurrentElite ? " (elita x2)" : "";
            Screen("WYGRANA!", $"+{RunState.CurrentGoldReward} zł{elite}  ·  Zakręć automatem!");
            var machine = SlotMachine.Create(root, new Vector2(0f, 30f));
            var spin = Button("ZAKRĘĆ!", new Vector2(0f, -370f), Ui.RiskColor, 0, "Dice", new Vector2(520f, 130f));
            StartCoroutine(Pulse(spin));
            yield return WaitChoice();
            Destroy(spin.gameObject);

            var result = SlotMachine.Roll();
            yield return machine.Spin(result);

            var groups = result.GroupBy(s => s).OrderByDescending(g => g.Count()).First();
            int count = groups.Count();
            var symbol = groups.Key;
            if (count >= 2)
                for (int i = 0; i < 3; i++)
                    if (result[i] == symbol)
                        machine.Highlight(i, count == 3 ? new Color(1f, 0.85f, 0.2f) : new Color(0.6f, 1f, 0.6f));

            var (text, good) = Payout(symbol, count);
            Refresh("WYGRANA!", $"+{RunState.CurrentGoldReward} zł{elite}");
            if (count == 3 && good)
                yield return Jackpot(text, symbol == SlotSymbol.Gem, -300f);
            else
            {
                HitFeedback.Instance.Play(good ? SfxId.Coin : SfxId.Lose, 1f, 0f);
                BigText(text, good ? new Color(0.6f, 1f, 0.5f) : new Color(1f, 0.4f, 0.35f), -300f);
                if (!good) HitFeedback.Instance.Shake(0.12f, 0.3f);
            }
            yield return new WaitForSecondsRealtime(0.6f);
            Button("Dalej", new Vector2(0f, -420f), Ui.GoColor, 0, "Flag");
            yield return WaitChoice();
        }

        (string, bool) Payout(SlotSymbol symbol, int count)
        {
            bool triple = count == 3;
            if (count < 2)
            {
                RunState.AddGold(5);
                return ("Nic... +5 zł", true);
            }
            switch (symbol)
            {
                case SlotSymbol.Coin:
                    RunState.AddGold(triple ? 60 : 20);
                    return (triple ? "JACKPOT! +60 zł" : "+20 zł", true);
                case SlotSymbol.Heart:
                    RunState.HealParty(triple ? 1f : 0.25f);
                    return (triple ? "JACKPOT! Pełne leczenie" : "Leczenie +25%", true);
                case SlotSymbol.Card:
                    extraPicks++;
                    extraMinimum = triple ? Rarity.Epic : Rarity.Common;
                    return (triple ? "JACKPOT! Epicka karta" : "Dodatkowa karta", true);
                case SlotSymbol.Rage:
                    RunState.StartRage = triple ? 100f : 40f;
                    return (triple ? "JACKPOT! Pełna Furia" : "Furia +40 na start", true);
                case SlotSymbol.Gem:
                    RunState.AddGold(triple ? 150 : 50);
                    return (triple ? "MEGA JACKPOT! +150 zł" : "+50 zł", true);
                default:
                    if (triple)
                    {
                        RunState.HurtParty(0.15f);
                        RunState.AddGold(-20);
                        return ("PECH! -15% HP, -20 zł", false);
                    }
                    RunState.AddGold(-10);
                    return ("Pech... -10 zł", false);
            }
        }

        IEnumerator Jackpot(string text, bool mega, float y)
        {
            HitFeedback.Instance.Play(SfxId.Jackpot, 1f, 0f);
            HitFeedback.Instance.Shake(mega ? 0.3f : 0.2f, 0.6f);
            var flash = Ui.Stretch("Flash", dim);
            var flashImage = Ui.Image(flash, new Color(1f, 0.95f, 0.7f, 0.85f));
            var label = BigText(text, new Color(1f, 0.85f, 0.15f), y);
            label.fontSize = mega ? 92 : 80;
            StartCoroutine(Pulse(label.rectTransform));
            StartCoroutine(CoinShower(mega ? 90 : 50));
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.5f)
            {
                flashImage.color = new Color(1f, 0.95f, 0.7f, 0.85f * (1f - t));
                yield return null;
            }
            Destroy(flash.gameObject);
        }

        IEnumerator CoinShower(int count)
        {
            var layer = dim;
            for (int i = 0; i < count; i++)
            {
                if (layer == null) yield break;
                var coin = Ui.Icon(layer, "Coin", new Vector2(Random.Range(-200f, 200f), 60f), Random.Range(44f, 70f));
                coin.gameObject.AddComponent<FallingCoin>().Launch(new Vector2(Random.Range(-900f, 900f), Random.Range(500f, 1300f)));
                if (i % 6 == 0) HitFeedback.Instance.Play(SfxId.Coin, 0.35f, 0.25f);
                yield return new WaitForSecondsRealtime(0.015f);
            }
        }

        IEnumerator DoubleOrNothing()
        {
            int stake = RunState.CurrentGoldReward;
            if (RunState.Gold < stake || stake <= 0) yield break;
            float chance = Mathf.Min(0.65f, 0.5f + RunState.Luck * 0.03f);
            Screen("PODWÓJNIE ALBO NIC?", $"Stawka: {stake} zł  ·  Szansa: {Mathf.RoundToInt(chance * 100f)}%");
            var box = Ui.Centered("Wheel", root, new Vector2(0f, 40f), new Vector2(760f, 380f));
            Ui.Panel(box, Ui.PanelColor);
            Ui.Icon(box, "Dice", new Vector2(0f, 90f), 150f);
            var wheel = Ui.Label(Ui.Centered("Label", box, new Vector2(0f, -80f), new Vector2(740f, 160f)), $"{stake} zł", 120, TextAnchor.MiddleCenter, Color.white);
            var risk = Button("Ryzykuję!", new Vector2(-260f, -330f), Ui.RiskColor, 1, "Dice");
            var keep = Button("Zabieram", new Vector2(260f, -330f), Ui.NeutralColor, 0, "Coin");
            yield return WaitChoice();
            Destroy(risk.gameObject);
            Destroy(keep.gameObject);
            if (choice == 0) yield break;

            bool win = Random.value < chance;
            int flips = 22 + Random.Range(0, 2);
            if ((flips % 2 == 0) != win) flips++;
            for (int i = 0; i < flips; i++)
            {
                bool showWin = i % 2 == 0;
                wheel.text = showWin ? $"x2 = {stake * 2}" : "0 zł";
                wheel.color = showWin ? new Color(0.6f, 1f, 0.5f) : new Color(1f, 0.4f, 0.35f);
                HitFeedback.Instance.Play(SfxId.Tick, 0.8f, 0.1f);
                yield return new WaitForSecondsRealtime(0.04f + Mathf.Pow(i / (float)flips, 3f) * 0.35f);
            }
            if (win)
            {
                RunState.AddGold(stake);
                Refresh("PODWOJONE!", "");
                yield return Jackpot($"+{stake} zł", false, -260f);
            }
            else
            {
                RunState.AddGold(-stake);
                Refresh("PRZEPADŁO", "");
                HitFeedback.Instance.Play(SfxId.Lose, 1f, 0f);
                HitFeedback.Instance.Shake(0.15f, 0.35f);
                BigText($"-{stake} zł", new Color(1f, 0.4f, 0.35f), -260f);
            }
            yield return new WaitForSecondsRealtime(0.6f);
            Button("Dalej", new Vector2(0f, -400f), Ui.GoColor, 0, "Flag");
            yield return WaitChoice();
        }

        IEnumerator CardPick(Rarity minimum, string title)
        {
            var offers = RunState.Offer(3, minimum);
            while (true)
            {
                int rerollCost = 10 + rerolls * 5;
                Screen(title, "Dotknij karty, aby ją zabrać");
                yield return ShowCards(offers, (_, _) => "", 0f);
                var reroll = Button($"Losuj ({rerollCost} zł)", new Vector2(0f, -400f), RunState.Gold >= rerollCost ? Ui.RiskColor : Ui.NeutralColor, 100, "Dice");
                reroll.GetComponent<Button>().interactable = RunState.Gold >= rerollCost;
                yield return WaitChoice();
                if (choice == 100)
                {
                    if (RunState.Gold < rerollCost) continue;
                    RunState.AddGold(-rerollCost);
                    rerolls++;
                    offers = RunState.Offer(3, minimum);
                    continue;
                }
                RunState.Apply(offers[choice]);
                HitFeedback.Instance.Play(SfxId.Buff, 1f, 0f);
                yield return new WaitForSecondsRealtime(0.25f);
                yield break;
            }
        }

        IEnumerator ShowCards(List<UpgradeOffer> offers, System.Func<UpgradeOffer, int, string> footer, float y, bool interactable = true)
        {
            const float spacing = 420f;
            float start = -(offers.Count - 1) * spacing * 0.5f;
            for (int i = 0; i < offers.Count; i++)
            {
                var card = Card(offers[i], new Vector2(start + i * spacing, y), footer(offers[i], i), interactable ? i : -1);
                StartCoroutine(Flip(card));
                bool rare = offers[i].Upgrade.rarity >= Rarity.Epic;
                HitFeedback.Instance.Play(offers[i].Upgrade.rarity == Rarity.Legendary ? SfxId.Jackpot : rare ? SfxId.Rage : SfxId.Whoosh, rare ? 0.8f : 0.5f, 0.05f);
                if (offers[i].Upgrade.rarity == Rarity.Legendary)
                    StartCoroutine(Pulse(card));
                yield return new WaitForSecondsRealtime(0.18f);
            }
        }

        public static string IconFor(UpgradeDefinition upgrade)
        {
            if (upgrade.curse) return "Skull";
            if (upgrade.runBonus == RunBonus.Luck) return "Clover";
            if (upgrade.runBonus == RunBonus.GoldGain) return "Coin";
            if (upgrade.modifiers.Length == 0) return "Card";
            return upgrade.modifiers[0].stat switch
            {
                StatType.Damage => "Sword",
                StatType.CritChance or StatType.CritDamage => "Crit",
                StatType.MaxHealth => "Heart",
                StatType.Healing => "Heal",
                StatType.DamageTaken => "Defense",
                StatType.Lifesteal => "Lifesteal",
                StatType.RageGain => "Rage",
                _ => "Card",
            };
        }

        RectTransform Card(UpgradeOffer offer, Vector2 position, string footer, int value)
        {
            var upgrade = offer.Upgrade;
            var tint = upgrade.curse ? new Color(0.6f, 0.12f, 0.18f) : UpgradeDefinition.Tint(upgrade.rarity);
            var rect = Ui.Centered(upgrade.displayName, root, position, new Vector2(390f, 520f));
            var button = Ui.Button(rect, Ui.PanelColor, () =>
            {
                HitFeedback.Instance.Play(SfxId.Click);
                choice = value;
            });
            button.interactable = value >= 0;
            var colors = button.colors;
            colors.disabledColor = Color.white;
            button.colors = colors;
            Ui.Sliced(Ui.Stretch("Frame", rect, -2f), "Frame", tint);

            var glow = Ui.Icon(rect, "Circle", new Vector2(0f, 140f), 190f, new Color(tint.r, tint.g, tint.b, 0.25f));
            glow.raycastTarget = false;
            Ui.Icon(rect, IconFor(upgrade), new Vector2(0f, 140f), 150f);
            Ui.Wrapped(Ui.Centered("Name", rect, new Vector2(0f, 20f), new Vector2(360f, 90f)), upgrade.displayName, 38, TextAnchor.MiddleCenter, Color.white);
            Ui.Label(Ui.Centered("Target", rect, new Vector2(0f, -40f), new Vector2(360f, 36f)), offer.Target, 28, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.45f));
            Ui.Wrapped(Ui.Centered("Description", rect, new Vector2(0f, -120f), new Vector2(340f, 110f)), upgrade.description, 30, TextAnchor.MiddleCenter, new Color(0.88f, 0.92f, 1f));
            var tag = Ui.Centered("Rarity", rect, new Vector2(0f, -222f), new Vector2(240f, 48f));
            Ui.Sliced(tag, "Pill", tint);
            Ui.Label(Ui.Stretch("Label", tag), string.IsNullOrEmpty(footer) ? (upgrade.curse ? "KLĄTWA" : UpgradeDefinition.Label(upgrade.rarity)) : footer, 26, TextAnchor.MiddleCenter, Color.white);
            return rect;
        }

        IEnumerator PathChoice()
        {
            var nodes = RunState.NextIsFinal ? new List<Node> { Node.Shop, Node.Campfire } : RollNodes();
            Screen(RunState.NextIsFinal ? "PRZED BOSSEM" : "WYBIERZ ŚCIEŻKĘ", RunState.NextIsFinal ? "Herszt czeka. Przygotuj się." : "Dokąd teraz?");
            for (int i = 0; i < nodes.Count; i++)
            {
                var (title, description, color, icon) = Describe(nodes[i]);
                int value = i;
                var rect = Ui.Centered(title, root, new Vector2((i - 0.5f) * 580f, -40f), new Vector2(540f, 520f));
                Ui.Button(rect, color, () =>
                {
                    HitFeedback.Instance.Play(SfxId.Click);
                    choice = value;
                });
                Ui.Icon(rect, icon, new Vector2(0f, 120f), 200f);
                Ui.Label(Ui.Centered("Title", rect, new Vector2(0f, -30f), new Vector2(500f, 70f)), title, 54, TextAnchor.MiddleCenter, Color.white);
                Ui.Wrapped(Ui.Centered("Description", rect, new Vector2(0f, -160f), new Vector2(460f, 150f)), description, 32, TextAnchor.MiddleCenter, new Color(0.92f, 0.94f, 1f));
                StartCoroutine(Flip(rect));
            }
            yield return WaitChoice();
            switch (nodes[choice])
            {
                case Node.Elite:
                    RunState.NextElite = true;
                    break;
                case Node.Shop:
                    yield return Shop();
                    break;
                case Node.Chest:
                    yield return Chest();
                    break;
                case Node.Campfire:
                    RunState.HealParty(0.35f);
                    Screen("OGNISKO", "Drużyna odpoczywa: +35% HP");
                    Ui.Icon(root, "Campfire", new Vector2(0f, 20f), 300f);
                    HitFeedback.Instance.Play(SfxId.Heal, 1f, 0f);
                    Button("Dalej", new Vector2(0f, -330f), Ui.GoColor, 0, "Flag");
                    yield return WaitChoice();
                    break;
            }
        }

        static List<Node> RollNodes()
        {
            var pool = new List<Node> { Node.Normal, Node.Elite, Node.Shop, Node.Chest, Node.Campfire };
            var picked = new List<Node>();
            while (picked.Count < 2)
            {
                var node = pool[Random.Range(0, pool.Count)];
                pool.Remove(node);
                picked.Add(node);
            }
            return picked;
        }

        static (string, string, Color, string) Describe(Node node) => node switch
        {
            Node.Elite => ("Elita", "Silniejsi wrogowie, podwójne złoto i lepsza karta", new Color(0.55f, 0.14f, 0.14f, 1f), "Elite"),
            Node.Shop => ("Sklep", "Kup karty za złoto albo zapłać krwią", new Color(0.5f, 0.4f, 0.1f, 1f), "Shop"),
            Node.Chest => ("Skrzynia", "Epicka nagroda... i klątwa w pakiecie", new Color(0.38f, 0.14f, 0.48f, 1f), "Chest"),
            Node.Campfire => ("Ognisko", "Drużyna leczy 35% HP", new Color(0.18f, 0.42f, 0.24f, 1f), "Campfire"),
            _ => ("Ścieżka", "Zwykła walka", Ui.NeutralColor, "Sword"),
        };

        static int Price(Rarity rarity) => rarity switch
        {
            Rarity.Rare => 45,
            Rarity.Epic => 80,
            Rarity.Legendary => 130,
            _ => 25,
        };

        static float BloodPrice(Rarity rarity) => rarity switch
        {
            Rarity.Rare => 0.18f,
            Rarity.Epic => 0.28f,
            Rarity.Legendary => 0.4f,
            _ => 0.1f,
        };

        IEnumerator Shop()
        {
            var offers = RunState.Offer(3);
            var sold = new bool[offers.Count];
            const int healCost = 20;
            while (true)
            {
                Screen("SKLEP", "Złoto albo krew — wybór należy do ciebie");
                const float spacing = 420f;
                float start = -(offers.Count - 1) * spacing * 0.5f;
                for (int i = 0; i < offers.Count; i++)
                {
                    float x = start + i * spacing;
                    var card = Card(offers[i], new Vector2(x, 30f), sold[i] ? "SPRZEDANE" : "", -1);
                    card.localScale = Vector3.one * 0.86f;
                    if (sold[i])
                    {
                        card.gameObject.AddComponent<CanvasGroup>().alpha = 0.35f;
                        continue;
                    }
                    int price = Price(offers[i].Upgrade.rarity);
                    var gold = Button(price.ToString(), new Vector2(x - 98f, -272f), RunState.Gold >= price ? Ui.GoColor : Ui.NeutralColor, i, "Coin", new Vector2(186f, 110f));
                    gold.GetComponent<Button>().interactable = RunState.Gold >= price;
                    Button($"-{Mathf.RoundToInt(BloodPrice(offers[i].Upgrade.rarity) * 100f)}%", new Vector2(x + 98f, -272f), Ui.RiskColor, 10 + i, "Lifesteal", new Vector2(186f, 110f));
                }
                var heal = Button($"Leczenie ({healCost})", new Vector2(-260f, -410f), RunState.Gold >= healCost ? Ui.GoColor : Ui.NeutralColor, 50, "Heal");
                heal.GetComponent<Button>().interactable = RunState.Gold >= healCost;
                Button("Wyjdź", new Vector2(260f, -410f), Ui.NeutralColor, 99, "Flag");
                yield return WaitChoice();

                if (choice == 99) yield break;
                if (choice == 50)
                {
                    if (RunState.Gold < healCost) continue;
                    RunState.AddGold(-healCost);
                    RunState.HealParty(0.3f);
                    HitFeedback.Instance.Play(SfxId.Heal, 1f, 0f);
                    continue;
                }
                int index = choice % 10;
                if (index >= offers.Count || sold[index] || (choice < 10 && RunState.Gold < Price(offers[index].Upgrade.rarity))) continue;
                if (choice >= 10)
                {
                    RunState.HurtParty(BloodPrice(offers[index].Upgrade.rarity));
                    HitFeedback.Instance.Play(SfxId.Hit, 1f, 0f);
                    HitFeedback.Instance.Shake(0.1f, 0.25f);
                }
                else
                {
                    RunState.AddGold(-Price(offers[index].Upgrade.rarity));
                    HitFeedback.Instance.Play(SfxId.Coin, 1f, 0f);
                }
                RunState.Apply(offers[index]);
                sold[index] = true;
            }
        }

        IEnumerator Chest()
        {
            Screen("PRZEKLĘTA SKRZYNIA", "Szepcze obietnice. Otworzysz?");
            var icon = Ui.Icon(root, "Chest", new Vector2(0f, 40f), 300f);
            StartCoroutine(Pulse(icon.rectTransform));
            Button("Otwórz", new Vector2(-260f, -330f), Ui.RiskColor, 1, "Chest");
            Button("Odejdź", new Vector2(260f, -330f), Ui.NeutralColor, 0, "Flag");
            yield return WaitChoice();
            if (choice == 0) yield break;

            var reward = RunState.Offer(1, Rarity.Epic).FirstOrDefault();
            var curse = RunState.RandomCurse();
            var shown = new List<UpgradeOffer>();
            if (reward.Upgrade != null)
            {
                RunState.Apply(reward);
                shown.Add(reward);
            }
            RunState.Apply(curse);
            shown.Add(curse);
            Screen("SKRZYNIA OTWARTA", "Nagroda i cena");
            HitFeedback.Instance.Play(SfxId.Jackpot, 0.8f, 0f);
            yield return ShowCards(shown, (o, _) => o.Upgrade.curse ? "KLĄTWA" : "ZDOBYTE!", 20f, false);
            yield return new WaitForSecondsRealtime(0.5f);
            Button("Dalej", new Vector2(0f, -400f), Ui.GoColor, 0, "Flag");
            yield return WaitChoice();
        }

        static IEnumerator Flip(RectTransform rect)
        {
            var baseScale = rect.localScale;
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.22f)
            {
                if (rect == null) yield break;
                rect.localScale = new Vector3(Mathf.SmoothStep(0f, 1f, t) * baseScale.x, baseScale.y, 1f);
                yield return null;
            }
            if (rect != null) rect.localScale = baseScale;
        }

        static IEnumerator Pop(RectTransform rect, float duration, float from)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                if (rect == null) yield break;
                rect.localScale = Vector3.one * Mathf.Lerp(from, 1f, 1f - Mathf.Pow(1f - t, 3f));
                yield return null;
            }
            if (rect != null) rect.localScale = Vector3.one;
        }

        static IEnumerator Pulse(RectTransform rect)
        {
            while (rect != null)
            {
                rect.localScale = Vector3.one * (1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.05f);
                yield return null;
            }
        }
    }

    public class FallingCoin : MonoBehaviour
    {
        Vector2 velocity;
        float spin;
        float age;

        public void Launch(Vector2 initial)
        {
            velocity = initial;
            spin = Random.Range(-720f, 720f);
        }

        void Update()
        {
            age += Time.unscaledDeltaTime;
            velocity += Vector2.down * 2600f * Time.unscaledDeltaTime;
            var rect = (RectTransform)transform;
            rect.anchoredPosition += velocity * Time.unscaledDeltaTime;
            rect.Rotate(0f, 0f, spin * Time.unscaledDeltaTime);
            rect.localScale = new Vector3(Mathf.Abs(Mathf.Cos(age * 9f)) * 0.8f + 0.2f, 1f, 1f);
            if (age > 2.5f) Destroy(gameObject);
        }
    }
}
