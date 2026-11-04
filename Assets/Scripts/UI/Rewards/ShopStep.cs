using System.Collections;
using Featherguard.Feel;
using Featherguard.Run;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class ShopStep : IRewardStep
    {
        const int HealChoice = 50;
        const int ExitChoice = 99;
        const int BloodOffset = 10;
        const float Spacing = 420f;
        const string SoldLabel = "SPRZEDANE";

        public IEnumerator Run(RewardContext context)
        {
            var screen = context.Screen;
            var shop = new Shop(context.Session);

            while (true)
            {
                screen.Open("SKLEP", "Złoto albo krew — wybór należy do ciebie");
                float start = -(shop.Offers.Count - 1) * Spacing * 0.5f;
                for (int i = 0; i < shop.Offers.Count; i++)
                {
                    float x = start + i * Spacing;
                    var card = screen.Card(shop.Offers[i], new Vector2(x, 30f), shop.IsSold(i) ? SoldLabel : "", -1);
                    card.localScale = Vector3.one * 0.86f;
                    if (shop.IsSold(i))
                    {
                        card.gameObject.AddComponent<CanvasGroup>().alpha = 0.35f;
                        continue;
                    }
                    bool affordable = shop.CanAfford(i);
                    var gold = screen.Button(shop.GoldPrice(i).ToString(), new Vector2(x - 98f, -272f), affordable ? Ui.GoColor : Ui.NeutralColor, i, "Coin", new Vector2(186f, 110f));
                    gold.GetComponent<Button>().interactable = affordable;
                    screen.Button($"-{Mathf.RoundToInt(shop.BloodPrice(i) * 100f)}%", new Vector2(x + 98f, -272f), Ui.RiskColor, BloodOffset + i, "Lifesteal", new Vector2(186f, 110f));
                }
                var heal = screen.Button($"Leczenie ({shop.HealCost})", new Vector2(-260f, -410f), shop.CanAffordHeal ? Ui.GoColor : Ui.NeutralColor, HealChoice, "Heal");
                heal.GetComponent<Button>().interactable = shop.CanAffordHeal;
                screen.Button("Wyjdź", new Vector2(260f, -410f), Ui.NeutralColor, ExitChoice, "Flag");
                yield return screen.WaitChoice();

                int choice = screen.Choice;
                if (choice == ExitChoice) yield break;
                if (choice == HealChoice)
                {
                    if (shop.BuyHeal()) FeedbackHub.Ui(SfxId.Heal);
                }
                else if (choice >= BloodOffset)
                {
                    if (!shop.BuyWithBlood(choice - BloodOffset)) continue;
                    FeedbackHub.Ui(SfxId.Hit);
                    FeedbackHub.Instance.Shake(0.1f, 0.25f);
                }
                else if (shop.BuyWithGold(choice))
                {
                    FeedbackHub.Ui(SfxId.Coin);
                }
            }
        }
    }
}
