using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class BattleServices
    {
        public BattlePresentationConfig Config;
        public UnitViewFactory Factory;
        public BattleVisuals Visuals;
        public IBattleView View;
        public ITimingCueView TimingCues;
        public ITimingInput TimingInput;
        public IRandomSource Random;
        public ICombatFeedback Feedback;
        public MonoBehaviour Host;
        public IUnitPicker UnitPicker;
    }

    public sealed class BattleController
    {
        readonly PlayerCommands commands;
        readonly TimingPrompt prompt;
        EnemyWindup windup;

        public BattleModel Model { get; }
        public BattlePresenter Presenter { get; }
        public IPlayerCommands Commands => commands;
        public BattleResult Result => Model.Result;

        public BattleController(BattleModel model, BattleServices services)
        {
            Model = model;
            Presenter = new BattlePresenter(services.Config, services.Factory, services.Visuals, services.View, services.Feedback, services.Host, model.Rage);
            var highlighter = new TargetHighlighter(model, Presenter, services.Config);
            commands = new PlayerCommands(model, Presenter, services.UnitPicker, highlighter, services.Feedback, services.View);
            if (services.Config.timingPrompts)
                prompt = new TimingPrompt(services.Config, new TimingJudge(model.State.Rules), services.TimingCues, services.View, services.Feedback, services.TimingInput, services.Random);
        }

        public void Tick() => commands.Tick();

        public IEnumerator Run()
        {
            yield return Play(Model.Start());
            while (!Model.IsFinished)
            {
                if (Model.AwaitingBlock)
                {
                    yield return Block();
                    continue;
                }

                commands.Open();
                while (commands.Chosen == null)
                    yield return null;
                var command = commands.TakeChosen();
                var timing = TimingResult.Miss;
                if (prompt != null && Model.IsTimed(command.Hero, command.Ability))
                    yield return prompt.Attack(PointOf(command.Target, Model.Enemies), result => timing = result);
                yield return Play(Model.Act(command.Hero, command.Ability, command.Target, timing));
            }
            commands.Close();
        }

        IEnumerator Block()
        {
            var timing = TimingResult.Miss;
            if (prompt != null)
                yield return prompt.Block(PointOf(windup?.Target, Model.Heroes), result => timing = result);
            yield return Play(Model.Block(timing));
        }

        IEnumerator Play(IReadOnlyList<BattleEvent> events)
        {
            windup = events.OfType<EnemyWindup>().LastOrDefault() ?? windup;
            return Presenter.Play(events);
        }

        Vector3 PointOf(BattleUnit unit, Squad squad)
        {
            var target = unit != null && unit.IsAlive ? unit : squad.Living.FirstOrDefault();
            var view = target != null ? Presenter.ViewOf(target) : null;
            return view != null ? view.Center : Vector3.zero;
        }
    }
}
