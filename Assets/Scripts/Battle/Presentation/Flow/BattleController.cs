using System.Collections;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class BattleServices
    {
        public BattlePresentationConfig Config;
        public UnitViewFactory Factory;
        public AbilityVisualsLibrary Visuals;
        public IBattleView View;
        public ICombatFeedback Feedback;
        public MonoBehaviour Host;
        public IUnitPicker UnitPicker;
    }

    public sealed class BattleController
    {
        readonly PlayerCommands commands;

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
        }

        public void Tick() => commands.Tick();

        public IEnumerator Run()
        {
            yield return Presenter.Play(Model.Start());
            while (!Model.IsFinished)
            {
                commands.Open();
                while (commands.Submitted == null)
                    yield return null;
                yield return Presenter.Play(commands.TakeSubmitted());
            }
            commands.Close();
        }
    }
}
