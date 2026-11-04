using System.Collections;
using Featherguard.Feel;
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
            var session = context.Session;
            var economy = context.Economy;
            var wallet = session.Wallet;
            var screen = context.Screen;
            var offers = session.DrawOffers(economy.shopOffers);
            var sold = new bool[offers.Count];

            while (true)
            {
                screen.Open("SKLEP", "Złoto albo krew — wybór należy do ciebie");
                float start = -(offers.Count - 1) * Spacing * 0.5f;
                for (int i = 0; i < offers.Count; i++)
                {
                    float x = start + i * Spacing;
                    var card = screen.Card(offers[i], new Vector2(x, 30f), sold[i] ? SoldLabel : "", -1);
                    card.localScale = Vector3.one * 0.86f;
                    if (sold[i])
                    {
                        card.gameObject.AddComponent<CanvasGroup>().alpha = 0.35f;
                        continue;
                    }
                    var rarity = offers[i].Upgrade.rarity;
                    int price = economy.GoldPrice(rarity);
                    var gold = screen.Button(price.ToString(), new Vector2(x - 98f, -272f), wallet.CanAfford(price) ? Ui.GoColor : Ui.NeutralColor, i, "Coin", new Vector2(186f, 110f));
                    gold.GetComponent<Button>().interactable = wallet.CanAfford(price);
                    screen.Button($"-{Mathf.RoundToInt(economy.BloodPrice(rarity) * 100f)}%", new Vector2(x + 98f, -272f), Ui.RiskColor, BloodOffset + i, "Lifesteal", new Vector2(186f, 110f));
                }
                var heal = screen.Button($"Leczenie ({economy.shopHealCost})", new Vector2(-260f, -410f), wallet.CanAfford(economy.shopHealCost) ? Ui.GoColor : Ui.NeutralColor, HealChoice, "Heal");
                heal.GetComponent<Button>().interactable = wallet.CanAfford(economy.shopHealCost);
                screen.Button("Wyjdź", new Vector2(260f, -410f), Ui.NeutralColor, ExitChoice, "Flag");
                yield return screen.WaitChoice();

                int choice = screen.Choice;
                if (choice == ExitChoice) yield break;
                if (choice == HealChoice)
                {
                    if (wallet.TrySpend(economy.shopHealCost))
                    {
                        session.Party.HealAll(economy.shopHealFraction);
                        FeedbackHub.Ui(SfxId.Heal);
                    }
                    continue;
                }

                bool blood = choice >= BloodOffset;
                int index = blood ? choice - BloodOffset : choice;
                if (index < 0 || index >= offers.Count || sold[index]) continue;
                var tier = offers[index].Upgrade.rarity;
                if (blood)
                {
                    session.Party.HurtAll(economy.BloodPrice(tier));
                    FeedbackHub.Ui(SfxId.Hit);
                    FeedbackHub.Instance.Shake(0.1f, 0.25f);
                }
                else if (wallet.TrySpend(economy.GoldPrice(tier)))
                {
                    FeedbackHub.Ui(SfxId.Coin);
                }
                else continue;
                session.Apply(offers[index]);
                sold[index] = true;
            }
        }
    }
}
