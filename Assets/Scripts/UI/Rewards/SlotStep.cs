using System.Collections;
using Featherguard.Feel;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class SlotStep : IRewardStep
    {
        static readonly Color Win = new(0.6f, 1f, 0.5f);
        static readonly Color Loss = new(1f, 0.4f, 0.35f);
        static readonly Color JackpotFrame = new(1f, 0.85f, 0.2f);
        static readonly Color PairFrame = new(0.6f, 1f, 0.6f);

        public IEnumerator Run(RewardContext context)
        {
            var session = context.Session;
            var config = session.Definition.slots;
            var screen = context.Screen;
            string reward = $"+{session.CurrentGoldReward} zł{(session.CurrentElite ? " (elita x2)" : "")}";

            screen.Open("WYGRANA!", $"{reward}  ·  Zakręć automatem!");
            var machine = SlotMachineView.Create(screen.Root, new Vector2(0f, 30f), SlotRules.Available(config, session));
            var spin = screen.Button("ZAKRĘĆ!", new Vector2(0f, -370f), Ui.RiskColor, 0, "Dice", new Vector2(520f, 130f));
            Pulser.Attach(spin);
            yield return screen.WaitChoice();
            Object.Destroy(spin.gameObject);

            var reels = SlotRules.Roll(config, session, session.Random);
            yield return machine.Spin(reels);
            var outcome = SlotRules.Evaluate(config, reels);
            if (outcome.Matches >= 2)
                for (int i = 0; i < reels.Length; i++)
                    if (reels[i] == outcome.Symbol)
                        machine.Highlight(i, outcome.Matches >= 3 ? JackpotFrame : PairFrame);

            outcome.Reward?.Grant(session, context);
            screen.RefreshHeader("WYGRANA!", reward);
            string text = outcome.Reward?.text ?? "";
            if (outcome.IsJackpot)
            {
                bool mega = outcome.Symbol.megaJackpot;
                yield return screen.Jackpot((mega ? config.megaJackpotPrefix : config.jackpotPrefix) + text, mega, -300f);
            }
            else
            {
                bool positive = outcome.Reward == null || outcome.Reward.Positive;
                FeedbackHub.Ui(positive ? SfxId.Coin : SfxId.Lose);
                screen.BigText(text, positive ? Win : Loss, -300f);
                if (!positive) FeedbackHub.Instance.Shake(0.12f, 0.3f);
            }
            yield return screen.Continue(-420f);
        }
    }
}
