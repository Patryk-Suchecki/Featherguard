using System.Collections.Generic;

namespace Featherguard.Run
{
    public sealed class Shop
    {
        readonly RunSession session;
        readonly bool[] sold;

        public IReadOnlyList<UpgradeOffer> Offers { get; }
        public EconomyConfig Economy => session.Economy;
        public int HealCost => Economy.shopHealCost;
        public bool CanAffordHeal => session.Wallet.CanAfford(HealCost);

        public Shop(RunSession session)
        {
            this.session = session;
            Offers = session.DrawOffers(session.Economy.shopOffers);
            sold = new bool[Offers.Count];
        }

        public bool IsSold(int index) => sold[index];
        public int GoldPrice(int index) => Economy.GoldPrice(Offers[index].Upgrade.rarity);
        public float BloodPrice(int index) => Economy.BloodPrice(Offers[index].Upgrade.rarity);
        public bool CanAfford(int index) => session.Wallet.CanAfford(GoldPrice(index));

        public bool BuyWithGold(int index)
        {
            if (!IsAvailable(index) || !session.Wallet.TrySpend(GoldPrice(index))) return false;
            Deliver(index);
            return true;
        }

        public bool BuyWithBlood(int index)
        {
            if (!IsAvailable(index)) return false;
            session.Party.HurtAll(BloodPrice(index));
            Deliver(index);
            return true;
        }

        public bool BuyHeal()
        {
            if (!session.Wallet.TrySpend(HealCost)) return false;
            session.Party.HealAll(Economy.shopHealFraction);
            return true;
        }

        bool IsAvailable(int index) => index >= 0 && index < Offers.Count && !sold[index];

        void Deliver(int index)
        {
            session.Apply(Offers[index]);
            sold[index] = true;
        }
    }
}
