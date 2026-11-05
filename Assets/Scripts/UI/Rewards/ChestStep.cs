using System.Collections;
using Featherguard.Feel;
using Featherguard.Run;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class ChestStep : IRewardStep
    {
        const int Leave = 0;
        const int Open = 1;

        public IEnumerator Run(RewardContext context)
        {
            var session = context.Session;
            var screen = context.Screen;
            screen.Open("CURSED CHEST", "It whispers promises. Open it?");
            Pulser.Attach(Ui.Icon(screen.Root, "Chest", new Vector2(0f, 40f), 300f));
            screen.Button("Open", new Vector2(-260f, -330f), Ui.RiskColor, Open, "Chest");
            screen.Button("Leave", new Vector2(260f, -330f), Ui.NeutralColor, Leave, "Flag");
            yield return screen.WaitChoice();
            if (screen.Choice == Leave) yield break;

            var shown = PathRewards.OpenChest(session);

            screen.Open("CHEST OPENED", "The reward and the price");
            FeedbackHub.Ui(SfxId.Jackpot, 0.8f);
            yield return CardPickStep.Reveal(screen, shown, o => o.Upgrade.curse ? "CURSE" : "GAINED!", 20f, false);
            yield return screen.Continue();
        }
    }
}
