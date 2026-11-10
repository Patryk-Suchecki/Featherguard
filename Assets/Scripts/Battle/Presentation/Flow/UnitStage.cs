using System;
using System.Collections;
using System.Collections.Generic;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class UnitStage
    {
        readonly BattlePresentationConfig config;
        readonly UnitViewFactory factory;
        readonly IUnitListView view;
        readonly ISoundPlayer sounds;
        readonly MonoBehaviour host;
        readonly Dictionary<BattleUnit, UnitView> views = new();

        public IEnumerable<UnitView> Views => views.Values;

        public event Action<UnitView> UnitDefeated;

        public UnitStage(BattlePresentationConfig config, UnitViewFactory factory, IUnitListView view, ISoundPlayer sounds, MonoBehaviour host)
        {
            this.config = config;
            this.factory = factory;
            this.view = view;
            this.sounds = sounds;
            this.host = host;
        }

        public UnitView ViewOf(BattleUnit unit) => unit != null && views.TryGetValue(unit, out var unitView) ? unitView : null;

        public void Register(BattleEventRouter router)
        {
            router.On<UnitSpawned>(Spawn);
            router.OnInstant<UnitRemoved>(Remove);
            router.OnInstant<UnitChanged>(e => ViewOf(e.Unit)?.Show(e.State));
            router.OnInstant<Died>(ShowDeath);
        }

        IEnumerator Spawn(UnitSpawned e)
        {
            var home = config.SlotPosition(e.Unit.Team, e.Unit.Slot);
            bool hero = e.Unit.Team == Team.Heroes;
            var unitView = factory.Create(e.Unit, e.State, hero ? home : home + Vector3.right * config.spawnOffset);
            unitView.HomePosition = home;
            views[e.Unit] = unitView;
            view.UnitAdded(unitView);
            if (hero) yield break;
            sounds.Play(SfxId.Whoosh, 0.5f);
            host.StartCoroutine(UnitMotion.HopTo(unitView.transform, home, 0.55f, 1.1f));
            yield return BattleEventRouter.Wait(e.Summoned ? config.summonInterval : config.spawnInterval);
        }

        void Remove(UnitRemoved e)
        {
            var unitView = ViewOf(e.Unit);
            if (unitView == null) return;
            views.Remove(e.Unit);
            view.UnitRemoved(unitView);
            host.StartCoroutine(UnitMotion.SinkAndDestroy(unitView.transform));
        }

        void ShowDeath(Died e)
        {
            var unit = ViewOf(e.Unit);
            if (unit == null) return;
            unit.Animator.Play("Death", 0.04f);
            unit.Ring.Show(null);
            sounds.Play(SfxId.Death);
            UnitDefeated?.Invoke(unit);
        }
    }
}
