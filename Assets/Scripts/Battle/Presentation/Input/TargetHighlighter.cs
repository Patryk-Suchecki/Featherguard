using System.Collections.Generic;

namespace Featherguard.Battle.Presentation
{
    public sealed class TargetHighlighter
    {
        readonly BattlePresenter presenter;
        readonly BattlePresentationConfig config;

        public TargetHighlighter(BattlePresenter presenter, BattlePresentationConfig config)
        {
            this.presenter = presenter;
            this.config = config;
        }

        public void Clear()
        {
            foreach (var unit in presenter.Views)
                unit.Ring.Show(null);
        }

        public void ShowBases(IEnumerable<BattleUnit> readyHeroes)
        {
            var ready = new HashSet<BattleUnit>(readyHeroes);
            foreach (var unit in presenter.Views)
                if (unit.IsAlive)
                    unit.Ring.ShowBase(unit.Team == Team.Enemies || ready.Contains(unit.Unit));
        }

        public void ShowHovered(BattleUnit hovered)
        {
            Clear();
            if (hovered != null)
                presenter.ViewOf(hovered)?.Ring.Show(config.hoverRing, true);
        }
    }
}
