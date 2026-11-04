using System.Linq;
using Featherguard.Core;

namespace Featherguard.Run
{
    public readonly struct SlotOutcome
    {
        public readonly SlotSymbol[] Reels;
        public readonly SlotSymbol Symbol;
        public readonly int Matches;
        public readonly SlotReward Reward;

        public SlotOutcome(SlotSymbol[] reels, SlotSymbol symbol, int matches, SlotReward reward)
        {
            Reels = reels;
            Symbol = symbol;
            Matches = matches;
            Reward = reward;
        }

        public bool IsJackpot => Matches >= 3 && Reward != null && Reward.Positive;
    }

    public static class SlotRules
    {
        public static SlotSymbol[] Available(SlotConfig config, RunSession session) =>
            config.symbols.Where(s => s.IsAvailable(session)).ToArray();

        public static SlotSymbol[] Roll(SlotConfig config, RunSession session, IRandomSource random)
        {
            var symbols = Available(config, session);
            float match = config.MatchChance(session.Luck);
            SlotSymbol Pick() => WeightedRandom.Pick(symbols, s => s.WeightFor(session.Luck), random);

            var first = Pick();
            var second = random.Value < match ? first : Pick();
            var third = random.Value < match ? second : Pick();
            return new[] { first, second, third };
        }

        public static SlotOutcome Play(SlotConfig config, RunSession session, IRewardQueue queue)
        {
            var outcome = Evaluate(config, Roll(config, session, session.Random));
            outcome.Reward?.Grant(session, queue);
            return outcome;
        }

        public static SlotOutcome Evaluate(SlotConfig config, SlotSymbol[] reels)
        {
            var best = reels.GroupBy(s => s).OrderByDescending(g => g.Count()).First();
            int matches = best.Count();
            var reward = matches >= 3 ? best.Key.triple : matches == 2 ? best.Key.pair : config.noMatch;
            return new SlotOutcome(reels, best.Key, matches, reward);
        }
    }
}
