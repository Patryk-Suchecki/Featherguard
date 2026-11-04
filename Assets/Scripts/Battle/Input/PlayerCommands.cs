using System.Collections;
using System.Linq;
using Featherguard.Feel;

namespace Featherguard.Battle
{
    public interface IPlayerCommands
    {
        BattleUnit Selected { get; }
        bool SpecialArmed { get; }
        bool AcceptingInput { get; }
        void Select(BattleUnit hero);
        void ToggleSpecial();
    }

    public sealed class PlayerCommands : IPlayerCommands
    {
        const string TauntText = "Prowokacja!";

        readonly Combat combat;
        readonly ActionExecutor executor;
        readonly IUnitPicker picker;

        public BattleUnit Selected { get; private set; }
        public bool SpecialArmed { get; private set; }
        public bool AcceptingInput { get; private set; }
        public IEnumerator Pending { get; private set; }

        public PlayerCommands(Combat combat, ActionExecutor executor, IUnitPicker picker)
        {
            this.combat = combat;
            this.executor = executor;
            this.picker = picker;
        }

        public void Open()
        {
            AcceptingInput = true;
            SelectFirstReady();
        }

        public void Close()
        {
            AcceptingInput = false;
            SetSelection(null);
        }

        public void Suspend()
        {
            AcceptingInput = false;
            RefreshHighlights();
        }

        public void ClearPending() => Pending = null;

        public void SelectFirstReady() => SetSelection(combat.Heroes.FirstOrDefault(IsReady));

        public void Tick()
        {
            if (!CanCommand || GameClock.Paused) return;
            if (picker.TryPick(out var unit))
                OnUnitTapped(unit);
        }

        public void Select(BattleUnit hero)
        {
            if (!CanCommand || !IsReady(hero)) return;
            combat.Feedback.Play(SfxId.Click);
            SetSelection(hero);
        }

        public void ToggleSpecial()
        {
            if (!CanCommand || !combat.Rage.Ready || Selected == null) return;
            var special = Selected.Definition.special;
            if (special.TargetsAll)
            {
                SpecialArmed = true;
                Use(Selected, special, null);
                return;
            }
            SpecialArmed = !SpecialArmed;
            combat.Feedback.Play(SfxId.Click);
            RefreshHighlights();
        }

        bool CanCommand => AcceptingInput && Pending == null;

        static bool IsReady(BattleUnit unit) => unit != null && unit.IsAlive && !unit.HasActed;

        void OnUnitTapped(BattleUnit unit)
        {
            if (Selected == null)
            {
                if (unit.Team == Team.Heroes && IsReady(unit))
                    SetSelection(unit);
                return;
            }

            var definition = Selected.Definition;
            var ability = SpecialArmed ? definition.special : unit.Team == Team.Enemies ? definition.attack : definition.support;
            if (ability == null || ability.TargetsEnemies != (unit.Team == Team.Enemies)) return;
            if (!Targeting.IsValid(ability, unit, combat.Heroes, combat.Enemies))
            {
                combat.View.Popup(unit.Top, TauntText, PopupKind.Buff);
                return;
            }
            Use(Selected, ability, unit);
        }

        void Use(BattleUnit actor, AbilityDefinition ability, BattleUnit target)
        {
            if (SpecialArmed)
            {
                combat.Rage.Consume();
                SpecialArmed = false;
            }
            actor.HasActed = true;
            Pending = executor.Execute(actor, ability, target);
        }

        void SetSelection(BattleUnit hero)
        {
            Selected = hero;
            SpecialArmed = false;
            RefreshHighlights();
        }

        void RefreshHighlights()
        {
            foreach (var unit in combat.Heroes.Concat(combat.Enemies))
                unit.Ring.Show(null);
            if (Selected == null || !AcceptingInput) return;

            var rules = combat.Rules;
            var definition = Selected.Definition;
            var abilities = SpecialArmed ? new[] { definition.special } : new[] { definition.attack, definition.support };
            foreach (var ability in abilities.Where(a => a != null))
                foreach (var target in Targeting.Candidates(ability, combat.Heroes, combat.Enemies))
                    target.Ring.Show(ability.TargetsEnemies ? rules.enemyRing : rules.allyRing);
            Selected.Ring.Show(rules.selectedRing);
        }
    }
}
