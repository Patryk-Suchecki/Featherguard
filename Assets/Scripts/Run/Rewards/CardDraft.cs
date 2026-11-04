using System.Collections.Generic;

namespace Featherguard.Run
{
    public sealed class CardDraft
    {
        readonly RewardRound round;
        readonly Rarity minimum;

        public IReadOnlyList<UpgradeOffer> Offers { get; private set; }
        public int RerollCost => round.Economy.RerollCost(round.Rerolls);
        public bool CanReroll => round.Session.Wallet.CanAfford(RerollCost);
        public bool IsEmpty => Offers.Count == 0;

        public CardDraft(RewardRound round, Rarity minimum)
        {
            this.round = round;
            this.minimum = minimum;
            Offers = Draw();
        }

        public bool Reroll()
        {
            if (!round.Session.Wallet.TrySpend(RerollCost)) return false;
            round.Rerolls++;
            Offers = Draw();
            return true;
        }

        public bool Pick(int index)
        {
            if (index < 0 || index >= Offers.Count) return false;
            round.Session.Apply(Offers[index]);
            return true;
        }

        List<UpgradeOffer> Draw() => round.Session.DrawOffers(round.Economy.cardChoices, minimum);
    }
}
