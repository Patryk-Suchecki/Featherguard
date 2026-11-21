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
        readonly BattleVisuals visuals;
        EnemyWindup windup;
        IReadOnlyList<BattleEvent> forcedEnd;

        public BattleModel Model { get; }
        public BattlePresenter Presenter { get; }
        public IPlayerCommands Commands => commands;
        public BattleResult Result => Model.Result;

        public BattleController(BattleModel model, BattleServices services)
        {
            Model = model;
            visuals = services.Visuals;
            Presenter = new BattlePresenter(services.Config, services.Factory, services.Visuals, services.View, services.Feedback, services.Host, model.Rage);
            var highlighter = new TargetHighlighter(Presenter, services.Config);
            commands = new PlayerCommands(model, Presenter, services.UnitPicker, highlighter, services.Feedback, services.View);
            if (services.Config.timingPrompts)
                prompt = new TimingPrompt(services.Config, new TimingJudge(model.State.Rules), services.TimingCues, services.View, services.Feedback, services.TimingInput, services.Random);
        }

        public void Tick() => commands.Tick();

        public void End(bool victory)
        {
            if (!Model.IsFinished) forcedEnd = Model.End(victory);
        }

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
                while (commands.Chosen == null && forcedEnd == null)
                    yield return null;
                if (forcedEnd != null) break;
                var command = commands.TakeChosen();
                var timing = TimingResult.Miss;
                if (prompt != null && Model.IsTimed(command.Hero, command.Ability))
                {
                    var gesture = visuals.Abilities.Of(command.Ability).gesture;
                    var squad = command.Ability.TargetsEnemies ? Model.Enemies : Model.Heroes;
                    yield return prompt.Perform(gesture, PointOf(command.Target, squad), result => timing = result);
                }
                yield return Play(Model.Act(command.Hero, command.Ability, command.Target, timing));
            }
            commands.Close();
            if (forcedEnd != null) yield return Play(forcedEnd);
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
