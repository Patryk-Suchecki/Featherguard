using System.Collections.Generic;

namespace Featherguard.Run
{
    public sealed class RewardRound : IRewardQueue
    {
        readonly Queue<Rarity> pendingCards = new();

        public RunSession Session { get; }
        public EconomyConfig Economy => Session.Economy;
        public int Rerolls { get; internal set; }

        public RewardRound(RunSession session) => Session = session;

        public Rarity FirstCardRarity => Session.CurrentElite ? Economy.eliteCardRarity : Rarity.Common;

        public void QueueCard(Rarity minimum) => pendingCards.Enqueue(minimum);
        public bool TryTakeCard(out Rarity minimum) => pendingCards.TryDequeue(out minimum);
    }
}
