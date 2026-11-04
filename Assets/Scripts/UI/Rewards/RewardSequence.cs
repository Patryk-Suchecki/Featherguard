using System.Collections;
using System.Collections.Generic;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.UI
{
    public interface IRewardStep
    {
        IEnumerator Run(RewardContext context);
    }

    public sealed class RewardContext : IRewardQueue
    {
        readonly Queue<Rarity> pendingCards = new();

        public RunSession Session { get; }
        public EconomyConfig Economy => Session.Economy;
        public RewardScreen Screen { get; }
        public int Rerolls { get; set; }

        public RewardContext(RunSession session, RewardScreen screen)
        {
            Session = session;
            Screen = screen;
        }

        public void QueueCard(Rarity minimum) => pendingCards.Enqueue(minimum);
        public bool TryTakeCard(out Rarity minimum) => pendingCards.TryDequeue(out minimum);
    }

    public static class RewardSequence
    {
        public static IEnumerator Play(BattleHud hud, RunSession session)
        {
            hud.SetHudVisible(false);
            var context = new RewardContext(session, new RewardScreen(hud, session, hud));
            foreach (var step in Steps(session))
                yield return step.Run(context);
            while (context.TryTakeCard(out var minimum))
                yield return new CardPickStep(minimum, "DODATKOWA KARTA!").Run(context);
            yield return new PathStep().Run(context);
            context.Screen.Close();
        }

        static IEnumerable<IRewardStep> Steps(RunSession session)
        {
            yield return new SlotStep();
            yield return new GambleStep();
            yield return new CardPickStep(session.CurrentElite ? session.Economy.eliteCardRarity : Rarity.Common, "WYBIERZ KARTĘ");
        }
    }
}
