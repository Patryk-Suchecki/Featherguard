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
            var screen = context.Screen;
            var draft = new CardDraft(context.Round, minimum);
            while (!draft.IsEmpty)
            {
                screen.Open(title, "Tap a card to take it");
                yield return Reveal(screen, draft.Offers, _ => "", 0f, true);
                bool affordable = draft.CanReroll;
                var reroll = screen.Button($"Reroll ({draft.RerollCost} gold)", new Vector2(0f, -400f), affordable ? Ui.RiskColor : Ui.NeutralColor, RerollChoice, "Dice");
                reroll.GetComponent<Button>().interactable = affordable;
                yield return screen.WaitChoice();

                if (screen.Choice == RerollChoice)
                {
                    draft.Reroll();
                    continue;
                }
                if (!draft.Pick(screen.Choice)) continue;
                FeedbackHub.Ui(SfxId.Buff);
                yield return new WaitForSecondsRealtime(0.25f);
                yield break;
            }
        }

        public static IEnumerator Reveal(RewardScreen screen, IReadOnlyList<UpgradeOffer> offers, System.Func<UpgradeOffer, string> footer, float y, bool selectable)
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
