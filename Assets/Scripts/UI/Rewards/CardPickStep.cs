using System.Collections;
using System.Collections.Generic;
using Featherguard.Feel;
using Featherguard.Run;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class CardPickStep : IRewardStep
    {
        const int RerollChoice = 100;
        const float Spacing = 420f;

        readonly Rarity minimum;
        readonly string title;

        public CardPickStep(Rarity minimum, string title)
        {
            this.minimum = minimum;
            this.title = title;
        }

        public IEnumerator Run(RewardContext context)
        {
            var session = context.Session;
            var screen = context.Screen;
            var offers = session.DrawOffers(context.Economy.cardChoices, minimum);
            while (true)
            {
                int cost = context.Economy.RerollCost(context.Rerolls);
                screen.Open(title, "Dotknij karty, aby ją zabrać");
                yield return Reveal(screen, offers, _ => "", 0f, true);
                bool affordable = session.Wallet.CanAfford(cost);
                var reroll = screen.Button($"Losuj ({cost} zł)", new Vector2(0f, -400f), affordable ? Ui.RiskColor : Ui.NeutralColor, RerollChoice, "Dice");
                reroll.GetComponent<Button>().interactable = affordable;
                yield return screen.WaitChoice();

                if (screen.Choice == RerollChoice)
                {
                    if (!session.Wallet.TrySpend(cost)) continue;
                    context.Rerolls++;
                    offers = session.DrawOffers(context.Economy.cardChoices, minimum);
                    continue;
                }
                session.Apply(offers[screen.Choice]);
                FeedbackHub.Ui(SfxId.Buff);
                yield return new WaitForSecondsRealtime(0.25f);
                yield break;
            }
        }

        public static IEnumerator Reveal(RewardScreen screen, List<UpgradeOffer> offers, System.Func<UpgradeOffer, string> footer, float y, bool selectable)
        {
            float start = -(offers.Count - 1) * Spacing * 0.5f;
            for (int i = 0; i < offers.Count; i++)
            {
                var offer = offers[i];
                var card = screen.Card(offer, new Vector2(start + i * Spacing, y), footer(offer), selectable ? i : -1);
                screen.Run(Tweens.Flip(card));
                var rarity = offer.Upgrade.rarity;
                FeedbackHub.Ui(rarity == Rarity.Legendary ? SfxId.Jackpot : rarity >= Rarity.Epic ? SfxId.Rage : SfxId.Whoosh, rarity >= Rarity.Epic ? 0.8f : 0.5f);
                if (rarity == Rarity.Legendary)
                    Pulser.Attach(card);
                yield return new WaitForSecondsRealtime(0.18f);
            }
        }
    }
}
