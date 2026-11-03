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

        static readonly Color Panel = new(0.06f, 0.07f, 0.11f, 0.96f);
        static readonly Color Go = new(0.2f, 0.55f, 0.25f, 1f);
        static readonly Color Risk = new(0.75f, 0.2f, 0.12f, 1f);
        static readonly Color Neutral = new(0.25f, 0.3f, 0.4f, 1f);

        BattleHud hud;
        BattleManager battle;
        RectTransform root;
        Text partyLine;
        int choice = -1;
        int extraPicks;
        Rarity extraMinimum = Rarity.Common;
        int rerolls;

        public static void Play(BattleHud hud, BattleManager battle)
        {
            var flow = hud.gameObject.AddComponent<RewardFlow>();
            flow.hud = hud;
            flow.battle = battle;
            flow.StartCoroutine(flow.Sequence());
        }

        IEnumerator Sequence()
        {
            yield return Slots();
            yield return DoubleOrNothing();
            yield return CardPick(RunState.CurrentElite ? Rarity.Rare : Rarity.Common, "Wybierz ulepszenie");
            while (extraPicks > 0)
            {
                extraPicks--;
                yield return CardPick(extraMinimum, "Dodatkowa karta!");
            }
            yield return PathChoice();
            battle.ContinueRun();
        }

        RectTransform Screen(string title, string subtitle)
        {
            if (root != null) Destroy(root.gameObject);
            root = Ui.Stretch("Rewards", hud.Canvas);
            Ui.Image(root, new Color(0f, 0f, 0f, 0.72f), true);
            Ui.Label(Ui.Rect("Title", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -50f), new Vector2(1700f, 110f)),
                title, 84, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.25f));
            Ui.Label(Ui.Rect("Subtitle", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -160f), new Vector2(1700f, 50f)),
                subtitle, 32, TextAnchor.MiddleCenter, Color.white);
            partyLine = Ui.Label(Ui.Rect("Party", root, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 24f), new Vector2(1800f, 40f)),
                "", 26, TextAnchor.MiddleCenter, new Color(0.8f, 0.9f, 1f));
            RefreshParty();
            choice = -1;
            return root;
        }

        void RefreshParty()
        {
            var heroes = string.Join("   ", RunState.Heroes.Select(h => $"{h.displayName} {RunState.HealthOf(h)}/{RunState.MaxHealthOf(h)}"));
            partyLine.text = $"{heroes}   ·   {RunState.Gold} zł   ·   Szczęście {RunState.Luck}";
            hud.RefreshGold();
        }

        RectTransform Button(string label, Vector2 position, Color color, int value, float width = 420f)
        {
            var rect = BattleHud.AddButton(root, label, position, color, () =>
            {
                HitFeedback.Instance.Play(SfxId.Click);
                choice = value;
            });
            rect.sizeDelta = new Vector2(width, 90f);
            return rect;
        }

        IEnumerator WaitChoice()
        {
            choice = -1;
            yield return new WaitUntil(() => choice >= 0);
        }

        Text BigText(string text, Color color, float y)
        {
            var label = Ui.Label(Ui.Rect("Result", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, y), new Vector2(1700f, 120f)),
                text, 76, TextAnchor.MiddleCenter, color);
            StartCoroutine(Pop(label.rectTransform, 0.25f, 1.8f));
            return label;
        }

        IEnumerator Slots()
        {
            string elite = RunState.CurrentElite ? " (elita x2)" : "";
            Screen("WYGRANA!", $"Nagroda: {RunState.CurrentGoldReward} zł{elite}  ·  Zakręć automatem!");
            var machine = SlotMachine.Create(root, new Vector2(0f, 60f));
            var spin = Button("ZAKRĘĆ!", new Vector2(0f, -300f), Risk, 0, 460f);
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
            RefreshParty();
            if (count == 3 && good)
                yield return Jackpot(text, symbol == SlotSymbol.Gem);
            else
            {
                HitFeedback.Instance.Play(good ? SfxId.Coin : SfxId.Lose, 1f, 0f);
                BigText(text, good ? new Color(0.6f, 1f, 0.5f) : new Color(1f, 0.4f, 0.35f), -280f);
                if (!good) HitFeedback.Instance.Shake(0.12f, 0.3f);
            }
            yield return new WaitForSecondsRealtime(0.6f);
            Button("Dalej", new Vector2(0f, -400f), Go, 0);
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
                    return (triple ? "JACKPOT! Pełna Furia na start" : "Furia +40 na start", true);
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

        IEnumerator Jackpot(string text, bool mega)
        {
            HitFeedback.Instance.Play(SfxId.Jackpot, 1f, 0f);
            HitFeedback.Instance.Shake(mega ? 0.3f : 0.2f, 0.6f);
            var flash = Ui.Stretch("Flash", root);
            var flashImage = Ui.Image(flash, new Color(1f, 0.95f, 0.7f, 0.85f));
            var label = BigText(text, new Color(1f, 0.85f, 0.15f), -280f);
            label.fontSize = mega ? 96 : 84;
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
            var layer = root;
            for (int i = 0; i < count; i++)
            {
                if (layer == null) yield break;
                var coin = Ui.Rect("Coin", layer, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(Random.Range(-200f, 200f), 60f), Vector2.one * Random.Range(26f, 44f));
                Ui.Image(coin, new Color(1f, Random.Range(0.72f, 0.9f), 0.15f));
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
            Screen("PODWÓJNIE ALBO NIC?", $"Postaw nagrodę za walkę: {stake} zł  ·  Szansa: {Mathf.RoundToInt(chance * 100f)}%");
            var wheel = Ui.Label(Ui.Rect("Wheel", root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, 80f), new Vector2(900f, 220f)),
                $"{stake} zł", 150, TextAnchor.MiddleCenter, Color.white);
            var risk = Button("Ryzykuję!", new Vector2(-240f, -200f), Risk, 1);
            var keep = Button("Zabieram", new Vector2(240f, -200f), Neutral, 0);
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
                wheel.text = showWin ? $"x2 = {stake * 2} zł" : "0 zł";
                wheel.color = showWin ? new Color(0.6f, 1f, 0.5f) : new Color(1f, 0.4f, 0.35f);
                HitFeedback.Instance.Play(SfxId.Tick, 0.8f, 0.1f);
                yield return new WaitForSecondsRealtime(0.04f + Mathf.Pow(i / (float)flips, 3f) * 0.35f);
            }
            if (win)
            {
                RunState.AddGold(stake);
                yield return Jackpot($"PODWOJONE! +{stake} zł", false);
            }
            else
            {
                RunState.AddGold(-stake);
                HitFeedback.Instance.Play(SfxId.Lose, 1f, 0f);
                HitFeedback.Instance.Shake(0.15f, 0.35f);
                BigText($"Przepadło -{stake} zł", new Color(1f, 0.4f, 0.35f), -150f);
            }
            RefreshParty();
            yield return new WaitForSecondsRealtime(0.6f);
            Button("Dalej", new Vector2(0f, -330f), Go, 0);
            yield return WaitChoice();
        }

        IEnumerator CardPick(Rarity minimum, string title)
        {
            var offers = RunState.Offer(3, minimum);
            while (true)
            {
                int rerollCost = 10 + rerolls * 5;
                Screen(title, "Kliknij kartę, aby ją zabrać");
                yield return ShowCards(offers, (_, _) => "Weź");
                var reroll = Button($"Losuj ponownie ({rerollCost} zł)", new Vector2(0f, -400f), RunState.Gold >= rerollCost ? Risk : Neutral, 100, 520f);
                reroll.GetComponent<Button>().interactable = RunState.Gold >= rerollCost;
                yield return WaitChoice();
                if (choice == 100)
                {
                    RunState.AddGold(-rerollCost);
                    rerolls++;
                    offers = RunState.Offer(3, minimum);
                    continue;
                }
                RunState.Apply(offers[choice]);
                RefreshParty();
                HitFeedback.Instance.Play(SfxId.Buff, 1f, 0f);
                yield return new WaitForSecondsRealtime(0.25f);
                yield break;
            }
        }

        IEnumerator ShowCards(List<UpgradeOffer> offers, System.Func<UpgradeOffer, int, string> footer)
        {
            float spacing = 440f;
            float start = -(offers.Count - 1) * spacing * 0.5f;
            for (int i = 0; i < offers.Count; i++)
            {
                var card = Card(offers[i], new Vector2(start + i * spacing, 30f), footer(offers[i], i), i);
                StartCoroutine(Flip(card));
                bool rare = offers[i].Upgrade.rarity >= Rarity.Epic;
                HitFeedback.Instance.Play(offers[i].Upgrade.rarity == Rarity.Legendary ? SfxId.Jackpot : rare ? SfxId.Rage : SfxId.Whoosh, rare ? 0.8f : 0.5f, 0.05f);
                if (offers[i].Upgrade.rarity == Rarity.Legendary)
                    StartCoroutine(Pulse(card));
                yield return new WaitForSecondsRealtime(0.18f);
            }
        }

        RectTransform Card(UpgradeOffer offer, Vector2 position, string footer, int value)
        {
            var upgrade = offer.Upgrade;
            var tint = upgrade.curse ? new Color(0.5f, 0.1f, 0.15f) : UpgradeDefinition.Tint(upgrade.rarity);
            var rect = Ui.Rect(upgrade.displayName, root, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), position, new Vector2(400f, 500f));
            var button = Ui.Button(rect, tint, () =>
            {
                HitFeedback.Instance.Play(SfxId.Click);
                choice = value;
            });
            button.interactable = value >= 0;
            var body = Ui.Stretch("Body", rect, 8f);
            Ui.Image(body, Panel);
            Ui.Label(Ui.Rect("Rarity", body, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -14f), new Vector2(380f, 34f)),
                upgrade.curse ? "KLĄTWA" : UpgradeDefinition.Label(upgrade.rarity), 26, TextAnchor.MiddleCenter, tint);
            Ui.Label(Ui.Rect("Name", body, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -60f), new Vector2(370f, 60f)),
                upgrade.displayName, 38, TextAnchor.MiddleCenter, Color.white);
            Ui.Label(Ui.Rect("Target", body, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -122f), new Vector2(370f, 34f)),
                offer.Target, 26, TextAnchor.MiddleCenter, new Color(1f, 0.85f, 0.45f));
            var description = Ui.Label(Ui.Rect("Description", body, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(340f, 200f)),
                upgrade.description, 30, TextAnchor.MiddleCenter, new Color(0.88f, 0.92f, 1f));
            description.horizontalOverflow = HorizontalWrapMode.Wrap;
            if (!string.IsNullOrEmpty(footer))
                Ui.Label(Ui.Rect("Footer", body, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 16f), new Vector2(370f, 40f)),
                    footer, 28, TextAnchor.MiddleCenter, new Color(0.6f, 1f, 0.6f));
            return rect;
        }

        IEnumerator PathChoice()
        {
            var nodes = RunState.NextIsFinal ? new List<Node> { Node.Shop, Node.Campfire } : RollNodes();
            Screen(RunState.NextIsFinal ? "PRZED BOSSEM" : "WYBIERZ ŚCIEŻKĘ", RunState.NextIsFinal ? "Herszt czeka. Przygotuj się." : "Dokąd teraz?");
            for (int i = 0; i < nodes.Count; i++)
            {
                var (title, description, color) = Describe(nodes[i]);
                var rect = Button(title, new Vector2((i - 0.5f) * 560f, 40f), color, i, 500f);
                rect.sizeDelta = new Vector2(500f, 300f);
                var label = rect.GetComponentInChildren<Text>();
                label.rectTransform.offsetMin = new Vector2(0f, 120f);
                label.fontSize = 46;
                var text = Ui.Label(Ui.Rect("Description", rect, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(0f, 20f), new Vector2(440f, 150f)),
                    description, 28, TextAnchor.MiddleCenter, Color.white);
                text.horizontalOverflow = HorizontalWrapMode.Wrap;
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
                    HitFeedback.Instance.Play(SfxId.Heal, 1f, 0f);
                    Button("Dalej", new Vector2(0f, -200f), Go, 0);
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

        static (string, string, Color) Describe(Node node) => node switch
        {
            Node.Elite => ("Elita", "Silniejsi wrogowie (+35%), podwójne złoto i co najmniej rzadka karta", new Color(0.6f, 0.15f, 0.15f, 1f)),
            Node.Shop => ("Sklep", "Kup karty za złoto albo zapłać krwią", new Color(0.55f, 0.45f, 0.1f, 1f)),
            Node.Chest => ("Przeklęta skrzynia", "Epicka nagroda... i klątwa w pakiecie", new Color(0.4f, 0.15f, 0.5f, 1f)),
            Node.Campfire => ("Ognisko", "Drużyna leczy 35% HP", new Color(0.2f, 0.45f, 0.25f, 1f)),
            _ => ("Ścieżka", "Zwykła walka", Neutral),
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
                float spacing = 440f;
                float start = -(offers.Count - 1) * spacing * 0.5f;
                for (int i = 0; i < offers.Count; i++)
                {
                    float x = start + i * spacing;
                    var card = Card(offers[i], new Vector2(x, 70f), sold[i] ? "SPRZEDANE" : "", -1);
                    if (sold[i])
                    {
                        card.gameObject.AddComponent<CanvasGroup>().alpha = 0.35f;
                        continue;
                    }
                    int price = Price(offers[i].Upgrade.rarity);
                    var gold = Button($"{price} zł", new Vector2(x - 100f, -250f), RunState.Gold >= price ? Go : Neutral, i, 190f);
                    gold.GetComponent<Button>().interactable = RunState.Gold >= price;
                    Button($"Krew -{Mathf.RoundToInt(BloodPrice(offers[i].Upgrade.rarity) * 100f)}%", new Vector2(x + 100f, -250f), Risk, 10 + i, 190f);
                }
                var heal = Button($"Leczenie 30% ({healCost} zł)", new Vector2(-280f, -390f), RunState.Gold >= healCost ? Go : Neutral, 50, 500f);
                heal.GetComponent<Button>().interactable = RunState.Gold >= healCost;
                Button("Wyjdź", new Vector2(280f, -390f), Neutral, 99, 500f);
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
            Button("Otwórz", new Vector2(-240f, -100f), Risk, 1);
            Button("Odejdź", new Vector2(240f, -100f), Neutral, 0);
            yield return WaitChoice();
            if (choice == 0) yield break;

            var reward = RunState.Offer(1, Rarity.Epic).FirstOrDefault();
            var curse = RunState.RandomCurse();
            Screen("SKRZYNIA OTWARTA", "Nagroda i cena");
            var shown = new List<UpgradeOffer>();
            if (reward.Upgrade != null)
            {
                RunState.Apply(reward);
                shown.Add(reward);
            }
            RunState.Apply(curse);
            shown.Add(curse);
            RefreshParty();
            HitFeedback.Instance.Play(SfxId.Jackpot, 0.8f, 0f);
            yield return ShowCards(shown, (o, _) => o.Upgrade.curse ? "Klątwa" : "Zdobyte!");
            foreach (var button in root.GetComponentsInChildren<Button>())
                button.interactable = false;
            yield return new WaitForSecondsRealtime(0.5f);
            Button("Dalej", new Vector2(0f, -400f), Go, 0);
            yield return WaitChoice();
        }

        static IEnumerator Flip(RectTransform rect)
        {
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / 0.22f)
            {
                if (rect == null) yield break;
                rect.localScale = new Vector3(Mathf.SmoothStep(0f, 1f, t), 1f, 1f);
                yield return null;
            }
            if (rect != null) rect.localScale = Vector3.one;
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
