using System.Linq;

namespace Featherguard.Battle.Presentation
{
    public sealed class TargetHighlighter
    {
        readonly BattleModel model;
        readonly BattlePresenter presenter;
        readonly BattlePresentationConfig config;

        public TargetHighlighter(BattleModel model, BattlePresenter presenter, BattlePresentationConfig config)
        {
            this.model = model;
            this.presenter = presenter;
            this.config = config;
        }

        public void Clear()
        {
            foreach (var unit in presenter.Views)
                unit.Ring.Show(null);
        }

        public void Show(BattleUnit selected, bool specialArmed)
        {
            Clear();
            var definition = selected.Definition;
            var abilities = specialArmed ? new[] { definition.special } : new[] { definition.attack, definition.support };
            foreach (var ability in abilities.Where(a => a != null))
                foreach (var target in model.Candidates(selected, ability))
                    presenter.ViewOf(target)?.Ring.Show(ability.TargetsEnemies ? config.enemyRing : config.allyRing);
            presenter.ViewOf(selected)?.Ring.Show(config.selectedRing);
        }
    }
}
