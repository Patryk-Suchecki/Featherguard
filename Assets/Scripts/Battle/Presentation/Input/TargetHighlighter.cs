using System.Collections.Generic;

namespace Featherguard.Battle.Presentation
{
    public sealed class TargetHighlighter
    {
        const float CandidateAlpha = 0.45f;

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

        public void ShowCandidates(IEnumerable<BattleUnit> candidates, BattleUnit hovered)
        {
            Clear();
            var dim = config.hoverRing;
            dim.a *= CandidateAlpha;
            foreach (var unit in candidates)
                presenter.ViewOf(unit)?.Ring.Show(dim);
            if (hovered != null)
                presenter.ViewOf(hovered)?.Ring.Show(config.hoverRing, true);
        }

        public void ShowHovered(BattleUnit hovered)
        {
            Clear();
            if (hovered != null)
                presenter.ViewOf(hovered)?.Ring.Show(config.hoverRing, true);
        }
    }
}
