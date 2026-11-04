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

    public sealed class RewardContext
    {
        public RewardRound Round { get; }
        public RunSession Session => Round.Session;
        public EconomyConfig Economy => Round.Economy;
        public RewardScreen Screen { get; }

        public RewardContext(RewardRound round, RewardScreen screen)
        {
            Round = round;
            Screen = screen;
        }
    }

    public static class RewardSequence
    {
        public static IEnumerator Play(BattleHud hud, RunSession session)
        {
            hud.SetHudVisible(false);
            var context = new RewardContext(new RewardRound(session), new RewardScreen(hud, session, hud));
            foreach (var step in Steps(context.Round))
                yield return step.Run(context);
            while (context.Round.TryTakeCard(out var minimum))
                yield return new CardPickStep(minimum, "DODATKOWA KARTA!").Run(context);
            yield return new PathStep().Run(context);
            context.Screen.Close();
        }

        static IEnumerable<IRewardStep> Steps(RewardRound round)
        {
            yield return new SlotStep();
            yield return new GambleStep();
            yield return new CardPickStep(round.FirstCardRarity, "WYBIERZ KARTĘ");
        }
    }
}
