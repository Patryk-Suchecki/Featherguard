namespace Featherguard.Run
{
    public sealed class Gamble
    {
        readonly RunSession session;

        public int Stake { get; }
        public float Chance { get; }
        public bool Available => Stake > 0 && session.Wallet.CanAfford(Stake);

        public Gamble(RunSession session)
        {
            this.session = session;
            Stake = session.CurrentGoldReward;
            Chance = session.Economy.GambleChance(session.Luck);
        }

        public bool Resolve()
        {
            bool win = session.Random.Value < Chance;
            if (win) session.Wallet.Deposit(Stake);
            else session.Wallet.Lose(Stake);
            return win;
        }
    }
}
