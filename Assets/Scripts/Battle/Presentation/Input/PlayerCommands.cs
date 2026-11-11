using System.Collections.Generic;
using System.Linq;
using Featherguard.Feedback;

namespace Featherguard.Battle.Presentation
{
    public interface IPlayerCommands
    {
        BattleUnit Selected { get; }
        bool SpecialArmed { get; }
        bool AcceptingInput { get; }
        bool SpecialAvailable { get; }
        bool CanSelect(BattleUnit hero);
        void Select(BattleUnit hero);
        void ToggleSpecial();
    }

    public readonly struct PlayerCommand
    {
        public readonly BattleUnit Hero;
        public readonly AbilityDefinition Ability;
        public readonly BattleUnit Target;

        public PlayerCommand(BattleUnit hero, AbilityDefinition ability, BattleUnit target)
        {
            Hero = hero;
            Ability = ability;
            Target = target;
        }
    }

    public sealed class PlayerCommands : IPlayerCommands
    {
        const string TauntText = "Taunted!";

        readonly BattleModel model;
        readonly BattlePresenter presenter;
        readonly IUnitPicker picker;
        readonly TargetHighlighter highlighter;
        readonly ISoundPlayer sounds;
        readonly IPopupView view;

        public BattleUnit Selected { get; private set; }
        public bool SpecialArmed { get; private set; }
        public bool AcceptingInput { get; private set; }
        public PlayerCommand? Chosen { get; private set; }

        public PlayerCommands(BattleModel model, BattlePresenter presenter, IUnitPicker picker, TargetHighlighter highlighter, ISoundPlayer sounds, IPopupView view)
        {
            this.model = model;
            this.presenter = presenter;
            this.picker = picker;
            this.highlighter = highlighter;
            this.sounds = sounds;
            this.view = view;
        }

        public bool SpecialAvailable => CanCommand && model.Rage.Ready && Selected?.Definition.special != null;

        public bool CanSelect(BattleUnit hero) => model.CanCommand(hero);

        public void Open()
        {
            AcceptingInput = model.AwaitingInput;
            SetSelection(AcceptingInput ? model.Heroes.FirstOrDefault(model.CanCommand) : null);
        }

        public void Close()
        {
            AcceptingInput = false;
            SetSelection(null);
        }

        public PlayerCommand TakeChosen()
        {
            var command = Chosen.Value;
            Chosen = null;
            AcceptingInput = false;
            RefreshHighlights();
            return command;
        }

        public void Tick()
        {
            if (!CanCommand) return;
            if (picker.TryPick(presenter.Views, out var unit))
                Tap(unit.Unit);
        }

        public void Select(BattleUnit hero)
        {
            if (!CanCommand || !model.CanCommand(hero)) return;
            sounds.Play(SfxId.Click);
            SetSelection(hero);
        }

        public void ToggleSpecial()
        {
            if (!SpecialAvailable) return;
            var special = Selected.Definition.special;
            if (special.TargetsAll)
            {
                Submit(special, null);
                return;
            }
            SpecialArmed = !SpecialArmed;
            sounds.Play(SfxId.Click);
            RefreshHighlights();
        }

        public void Tap(BattleUnit unit)
        {
            if (!CanCommand || unit == null) return;
            if (Selected == null)
            {
                if (model.CanCommand(unit))
                    SetSelection(unit);
                return;
            }

            var definition = Selected.Definition;
            bool enemy = unit.Team == Team.Enemies;
            var ability = SpecialArmed ? definition.special : enemy ? definition.attack : definition.support;
            if (ability == null || ability.TargetsEnemies != enemy) return;
            if (model.IsBlockedByTaunt(Selected, ability, unit))
            {
                var target = presenter.ViewOf(unit);
                if (target != null) view.Popup(target.Top, TauntText, PopupKind.Buff);
                return;
            }
            Submit(ability, unit);
        }

        bool CanCommand => AcceptingInput && Chosen == null;

        void Submit(AbilityDefinition ability, BattleUnit target)
        {
            if (!model.CanUse(Selected, ability, target)) return;
            SpecialArmed = false;
            Chosen = new PlayerCommand(Selected, ability, target);
        }

        void SetSelection(BattleUnit hero)
        {
            Selected = hero;
            SpecialArmed = false;
            RefreshHighlights();
        }

        void RefreshHighlights()
        {
            if (Selected == null || !CanCommand) highlighter.Clear();
            else highlighter.Show(Selected, SpecialArmed);
        }
    }
}
