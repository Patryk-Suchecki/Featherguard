using System.Linq;

namespace Featherguard.Battle
{
    public sealed class StatusTicker
    {
        readonly BattleState state;

        public StatusTicker(BattleState state) => this.state = state;

        public void BeginTurn(Squad squad)
        {
            foreach (var unit in squad.Living.ToList())
            {
                foreach (var status in unit.ActiveStatuses.Where(s => s.Definition.damagePerTurn).ToList())
                {
                    state.DamageOverTime(unit, status);
                    if (!unit.IsAlive) break;
                }
                if (unit.IsAlive && unit.SkipsTurn)
                    state.Log.Emit(new Stunned { Unit = unit });
            }
        }

        public void EndTurn(Squad squad)
        {
            foreach (var unit in squad.Living.ToList())
                unit.Statuses.EndTurn();
        }
    }
}
