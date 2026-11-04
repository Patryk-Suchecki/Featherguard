using System.Collections;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class GambleStep : IRewardStep
    {
        const int Keep = 0;
        const int Risk = 1;

        static readonly Color Win = new(0.6f, 1f, 0.5f);
        static readonly Color Loss = new(1f, 0.4f, 0.35f);

        public IEnumerator Run(RewardContext context)
        {
            var session = context.Session;
            var wallet = session.Wallet;
            int stake = session.CurrentGoldReward;
            if (stake <= 0 || !wallet.CanAfford(stake)) yield break;

            float chance = context.Economy.GambleChance(session.Luck);
            var screen = context.Screen;
            screen.Open("PODWÓJNIE ALBO NIC?", $"Stawka: {stake} zł  ·  Szansa: {Mathf.RoundToInt(chance * 100f)}%");
            var box = Ui.Centered("Wheel", screen.Root, new Vector2(0f, 40f), new Vector2(760f, 380f));
            Ui.Panel(box, Ui.PanelColor);
            Ui.Icon(box, "Dice", new Vector2(0f, 90f), 150f);
            var wheel = Ui.Label(Ui.Centered("Label", box, new Vector2(0f, -80f), new Vector2(740f, 160f)), $"{stake} zł", 120, TextAnchor.MiddleCenter, Color.white);
            var risk = screen.Button("Ryzykuję!", new Vector2(-260f, -330f), Ui.RiskColor, Risk, "Dice");
            var keep = screen.Button("Zabieram", new Vector2(260f, -330f), Ui.NeutralColor, Keep, "Coin");
            yield return screen.WaitChoice();
            Object.Destroy(risk.gameObject);
            Object.Destroy(keep.gameObject);
            if (screen.Choice == Keep) yield break;

            bool win = session.Random.Value < chance;
            yield return SpinWheel(wheel, stake, win);
            if (win)
            {
                wallet.Deposit(stake);
                screen.RefreshHeader("PODWOJONE!", "");
                yield return screen.Jackpot($"+{stake} zł", false, -260f);
            }
            else
            {
                wallet.Lose(stake);
                screen.RefreshHeader("PRZEPADŁO", "");
                FeedbackHub.Ui(SfxId.Lose);
                FeedbackHub.Instance.Shake(0.15f, 0.35f);
                screen.BigText($"-{stake} zł", Loss, -260f);
            }
            yield return screen.Continue();
        }

        static IEnumerator SpinWheel(UnityEngine.UI.Text wheel, int stake, bool win)
        {
            int flips = 22 + Random.Range(0, 2);
            if ((flips % 2 == 0) != win) flips++;
            for (int i = 0; i < flips; i++)
            {
                bool showWin = i % 2 == 0;
                wheel.text = showWin ? $"x2 = {stake * 2}" : "0 zł";
                wheel.color = showWin ? Win : Loss;
                FeedbackHub.Ui(SfxId.Tick, 0.8f);
                yield return new WaitForSecondsRealtime(0.04f + Mathf.Pow(i / (float)flips, 3f) * 0.35f);
            }
        }
    }
}
