using System.Collections;
using Featherguard.Feel;
using UnityEngine;

namespace Featherguard.UI
{
    public sealed class CampfireStep : IRewardStep
    {
        public IEnumerator Run(RewardContext context)
        {
            float heal = context.Economy.campfireHeal;
            context.Session.Party.HealAll(heal);
            var screen = context.Screen;
            screen.Open("OGNISKO", $"Drużyna odpoczywa: +{Mathf.RoundToInt(heal * 100f)}% HP");
            Ui.Icon(screen.Root, "Campfire", new Vector2(0f, 20f), 300f);
            FeedbackHub.Ui(SfxId.Heal);
            yield return screen.Continue(-330f);
        }
    }
}
