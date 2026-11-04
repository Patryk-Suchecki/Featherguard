using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
            screen.Open("PRZEKLĘTA SKRZYNIA", "Szepcze obietnice. Otworzysz?");
            Pulser.Attach(Ui.Icon(screen.Root, "Chest", new Vector2(0f, 40f), 300f));
            screen.Button("Otwórz", new Vector2(-260f, -330f), Ui.RiskColor, Open, "Chest");
            screen.Button("Odejdź", new Vector2(260f, -330f), Ui.NeutralColor, Leave, "Flag");
            yield return screen.WaitChoice();
            if (screen.Choice == Leave) yield break;

            var shown = new List<UpgradeOffer>();
            var reward = session.DrawOffers(1, context.Economy.chestRarity).FirstOrDefault();
            if (reward.IsValid)
            {
                session.Apply(reward);
                shown.Add(reward);
            }
            var curse = session.DrawCurse();
            if (curse.IsValid)
            {
                session.Apply(curse);
                shown.Add(curse);
            }

            screen.Open("SKRZYNIA OTWARTA", "Nagroda i cena");
            FeedbackHub.Ui(SfxId.Jackpot, 0.8f);
            yield return CardPickStep.Reveal(screen, shown, o => o.Upgrade.curse ? "KLĄTWA" : "ZDOBYTE!", 20f, false);
            yield return screen.Continue();
        }
    }
}
