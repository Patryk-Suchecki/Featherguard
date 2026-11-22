using System.Collections;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class PotionPlayback
    {
        const string RevivedText = "Revived!";
        static readonly Color PotionColor = new(0.6f, 1f, 0.7f);

        readonly BattlePresentationConfig config;
        readonly IBattleView view;
        readonly ICombatFeedback feedback;
        readonly UnitStage stage;

        public PotionPlayback(BattlePresentationConfig config, IBattleView view, ICombatFeedback feedback, UnitStage stage)
        {
            this.config = config;
            this.view = view;
            this.feedback = feedback;
            this.stage = stage;
        }

        public void Register(BattleEventRouter router)
        {
            router.On<PotionUsed>(Use);
            router.OnInstant<Revived>(Revive);
        }

        IEnumerator Use(PotionUsed e)
        {
            view.Callout(e.Potion.displayName);
            feedback.Play(SfxId.Buff);
            foreach (var target in e.Targets)
            {
                var unit = stage.ViewOf(target);
                if (unit != null) feedback.Rise(unit.transform.position, PotionColor, unit.Radius);
            }
            yield return BattleEventRouter.Wait(config.afterAction);
        }

        void Revive(Revived e)
        {
            var unit = stage.ViewOf(e.Unit);
            if (unit == null) return;
            unit.Animator.Play("Idle", 0.25f);
            feedback.Play(SfxId.Heal);
            view.Popup(unit.Top, RevivedText, PopupKind.Heal);
        }
    }
}
