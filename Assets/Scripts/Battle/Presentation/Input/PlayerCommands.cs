using System.Linq;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public interface IPlayerCommands
    {
        BattleUnit Selected { get; }
        bool SpecialArmed { get; }
        bool AcceptingInput { get; }
        bool SpecialAvailable { get; }
        bool Dragging { get; }
        Vector3 DragOrigin { get; }
        Vector2 DragPointer { get; }
        BattleUnit DragTarget { get; }
        PotionDefinition HeldPotion { get; }
        Vector2? DragScreenOrigin { get; }
        void ToggleSpecial();
    }

    public interface IPotionCommands
    {
        bool CanUsePotions { get; }
        void GrabPotion(PotionDefinition potion, Vector2 screen);
        void MovePotion(Vector2 screen);
        void DropPotion(Vector2 screen);
    }

    public readonly struct PotionCommand
    {
        public readonly PotionDefinition Potion;
        public readonly BattleUnit Target;

        public PotionCommand(PotionDefinition potion, BattleUnit target)
        {
            Potion = potion;
            Target = target;
        }
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

    public sealed class PlayerCommands : IPlayerCommands, IPotionCommands
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
        public PotionCommand? ChosenPotion { get; private set; }
        public PotionDefinition HeldPotion { get; private set; }
        public Vector2? DragScreenOrigin => HeldPotion != null ? potionOrigin : null;
        public bool CanUsePotions => CanCommand && Selected == null;
        public bool Dragging => Selected != null && moved || HeldPotion != null;
        public Vector3 DragOrigin => Selected != null && presenter.ViewOf(Selected) != null ? presenter.ViewOf(Selected).Center : Vector3.zero;
        public Vector2 DragPointer { get; private set; }
        public BattleUnit DragTarget { get; private set; }

        bool moved;
        Vector2 potionOrigin;

        public PlayerCommands(BattleModel model, BattlePresenter presenter, IUnitPicker picker, TargetHighlighter highlighter, ISoundPlayer sounds, IPopupView view)
        {
            this.model = model;
            this.presenter = presenter;
            this.picker = picker;
            this.highlighter = highlighter;
            this.sounds = sounds;
            this.view = view;
        }

        public bool SpecialAvailable => CanCommand && model.Rage.Ready && model.Heroes.Any(h => model.CanCommand(h) && h.Definition.special != null);

        public void Open()
        {
            AcceptingInput = model.AwaitingInput;
            SpecialArmed = false;
            Selected = null;
            HeldPotion = null;
            DragTarget = null;
            RefreshHighlights();
        }

        public void Close()
        {
            AcceptingInput = false;
            SpecialArmed = false;
            Selected = null;
            HeldPotion = null;
            DragTarget = null;
            RefreshHighlights();
        }

        public PlayerCommand TakeChosen()
        {
            var command = Chosen.Value;
            Chosen = null;
            AcceptingInput = false;
            RefreshHighlights(command.Hero);
            return command;
        }

        public PotionCommand TakeChosenPotion()
        {
            var command = ChosenPotion.Value;
            ChosenPotion = null;
            AcceptingInput = false;
            RefreshHighlights();
            return command;
        }

        public void GrabPotion(PotionDefinition potion, Vector2 screen)
        {
            if (!CanUsePotions || model.Potions.Count(potion) <= 0) return;
            HeldPotion = potion;
            potionOrigin = screen;
            DragPointer = screen;
            DragTarget = null;
            sounds.Play(SfxId.Click);
            RefreshHighlights();
        }

        public void MovePotion(Vector2 screen)
        {
            if (HeldPotion == null) return;
            DragPointer = screen;
            var target = PotionTargetAt(screen);
            if (target == DragTarget) return;
            DragTarget = target;
            RefreshHighlights();
        }

        public void DropPotion(Vector2 screen)
        {
            if (HeldPotion == null) return;
            var potion = HeldPotion;
            var target = PotionTargetAt(screen);
            HeldPotion = null;
            DragTarget = null;
            if (target != null) ChosenPotion = new PotionCommand(potion, target);
            RefreshHighlights();
        }

        BattleUnit PotionTargetAt(Vector2 screen)
        {
            var candidates = model.PotionCandidates(HeldPotion).Select(presenter.ViewOf).Where(v => v != null);
            var target = picker.At(screen, candidates)?.Unit;
            return model.CanUsePotion(HeldPotion, target) ? target : null;
        }

        public void Tick()
        {
            if (!CanCommand || HeldPotion != null) return;
            var pointer = picker.Read(presenter.Views);
            if (pointer.Pressed)
            {
                var hero = pointer.Unit?.Unit;
                if (hero == null || !model.CanCommand(hero) || SpecialArmed && hero.Definition.special == null) return;
                Selected = hero;
                moved = false;
                DragTarget = null;
                DragPointer = pointer.Screen;
                sounds.Play(SfxId.Click);
                RefreshHighlights();
            }
            else if (pointer.Held && Selected != null)
            {
                moved = pointer.Moved;
                DragPointer = pointer.Screen;
                var over = pointer.Unit?.Unit;
                var target = moved && IsValidDrop(Selected, over) ? over : null;
                if (target == DragTarget) return;
                DragTarget = target;
                RefreshHighlights();
            }
            else if (pointer.Released && Selected != null)
            {
                var hero = Selected;
                Selected = null;
                moved = false;
                DragTarget = null;
                Drop(hero, pointer.Unit?.Unit, pointer.Moved);
                RefreshHighlights();
            }
        }

        bool IsValidDrop(BattleUnit hero, BattleUnit over) =>
            over != null
            && CommandIntent.TryResolve(hero, over, true, SpecialArmed, out var ability, out var target)
            && model.CanUse(hero, ability, target)
            && !model.IsBlockedByTaunt(hero, ability, target);

        public void ToggleSpecial()
        {
            if (!SpecialAvailable) return;
            SpecialArmed = !SpecialArmed;
            sounds.Play(SfxId.Click);
            RefreshHighlights();
        }

        bool CanCommand => AcceptingInput && Chosen == null && ChosenPotion == null;

        void Drop(BattleUnit hero, BattleUnit droppedOn, bool moved)
        {
            if (!CommandIntent.TryResolve(hero, droppedOn, moved, SpecialArmed, out var ability, out var target)) return;
            if (model.IsBlockedByTaunt(hero, ability, target))
            {
                var taunted = presenter.ViewOf(target);
                if (taunted != null) view.Popup(taunted.Top, TauntText, PopupKind.Buff);
                return;
            }
            if (!model.CanUse(hero, ability, target)) return;
            SpecialArmed = false;
            Chosen = new PlayerCommand(hero, ability, target);
        }

        void RefreshHighlights(BattleUnit acting = null)
        {
            highlighter.ShowBases(model.Heroes.Where(h => model.CanCommand(h) && h != acting));
            if (!CanCommand) highlighter.Clear();
            else if (HeldPotion != null) highlighter.ShowCandidates(model.PotionCandidates(HeldPotion), DragTarget);
            else if (Selected != null) highlighter.ShowHovered(DragTarget);
            else highlighter.Clear();
        }
    }
}
