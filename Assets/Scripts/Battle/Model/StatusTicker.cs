using System.Linq;

namespace Featherguard.Battle
{
    public sealed class StatusTicker
    {
        readonly Combat combat;

        public StatusTicker(Combat combat) => this.combat = combat;

        public void BeginTurn(Squad squad)
        {
            foreach (var unit in squad.Living.ToList())
            {
                foreach (var effect in unit.ActiveStatuses.Where(e => e.Definition.damagePerTurn).ToList())
                {
                    combat.DamageOverTime(unit, effect);
                    if (!unit.IsAlive) break;
                }
                if (unit.IsAlive && unit.SkipsTurn)
                    combat.Log.Emit(new Stunned { Unit = unit });
            }
        }

        public void EndTurn(Squad squad)
        {
            foreach (var unit in squad.Living.ToList())
                unit.Statuses.EndTurn();
        }
    }
}
