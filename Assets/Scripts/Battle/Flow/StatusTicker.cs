using System.Collections;
using System.Linq;
using UnityEngine;

namespace Featherguard.Battle
{
    public sealed class StatusTicker
    {
        const string StunnedText = "Ogłuszony";

        readonly Combat combat;

        public StatusTicker(Combat combat) => this.combat = combat;

        public IEnumerator BeginTurn(Squad squad)
        {
            foreach (var unit in squad.Living.ToList())
            {
                foreach (var effect in unit.ActiveStatuses.Where(e => e.Definition.damagePerTurn).ToList())
                {
                    combat.DamageOverTime(unit, effect.Potency, effect.Definition.tint);
                    yield return new WaitForSeconds(0.35f);
                    if (!unit.IsAlive) break;
                }
                if (unit.IsAlive && unit.SkipsTurn)
                {
                    combat.View.Popup(unit.Top, StunnedText, PopupKind.Buff);
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }

        public void EndTurn(Squad squad)
        {
            foreach (var unit in squad.Living)
                unit.Statuses.EndTurn();
        }
    }
}
