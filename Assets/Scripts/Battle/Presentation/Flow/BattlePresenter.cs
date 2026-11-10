using System;
using System.Collections;
using System.Collections.Generic;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class BattlePresenter
    {
        readonly BattleEventRouter router;
        readonly UnitStage stage;

        public RageMeter Rage { get; }
        public IEnumerable<UnitView> Views => stage.Views;

        public event Action<UnitView> UnitDefeated
        {
            add => stage.UnitDefeated += value;
            remove => stage.UnitDefeated -= value;
        }

        public BattlePresenter(BattlePresentationConfig config, UnitViewFactory factory, AbilityVisualsLibrary visuals, IBattleView view, ICombatFeedback feedback, MonoBehaviour host, RageMeter rage)
        {
            Rage = new RageMeter(rage.Max, rage.Value);
            router = new BattleEventRouter(host);
            stage = new UnitStage(config, factory, view, feedback, host);
            stage.Register(router);
            new BattleAnnouncer(config, view, feedback, stage, Rage).Register(router);
            new ActionPlayback(config, visuals, view, feedback, host, stage, router).Register();
        }

        public UnitView ViewOf(BattleUnit unit) => stage.ViewOf(unit);

        public IEnumerator Play(IReadOnlyList<BattleEvent> events) => router.Play(events);
    }
}
