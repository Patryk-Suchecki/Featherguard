using System.Linq;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using UnityEngine;
using UnityEngine.UI;

namespace Featherguard.UI
{
    public sealed class BattleInfoView
    {
        const float Width = 1080f;

        static readonly Color PanelColor = new(0.05f, 0.05f, 0.08f, 0.94f);
        static readonly Color HeroColor = new(1f, 0.85f, 0.45f);
        static readonly Color EnemyColor = new(1f, 0.52f, 0.44f);
        static readonly Color TextColor = new(0.9f, 0.86f, 0.78f);
        static readonly Color MutedColor = new(0.66f, 0.66f, 0.72f);

        readonly RectTransform parent;
        RectTransform panel;
        BattleUnit shownUnit;
        PotionDefinition shownPotion;

        public BattleInfoView(RectTransform parent) => this.parent = parent;

        public void Update(IPlayerCommands commands, PotionPouch pouch)
        {
            var potion = commands.InspectedPotion;
            var unit = potion == null ? commands.Inspected : null;
            if (unit == shownUnit && potion == shownPotion) return;
            shownUnit = unit;
            shownPotion = potion;
            if (panel != null) Object.Destroy(panel.gameObject);
            panel = null;
            if (potion != null) ShowPotion(potion, pouch != null ? pouch.Count(potion) : 0);
            else if (unit != null) ShowUnit(unit);
        }

        void ShowUnit(BattleUnit unit)
        {
            bool hero = unit.Team == Team.Heroes;
            Open();
            Line(unit.Definition.displayName, 46, hero ? HeroColor : EnemyColor, true);
            Line($"Health {unit.Health} / {unit.MaxHealth}", 30, Color.white);
            if (unit.ActiveStatuses.Count > 0)
                Line("Statuses: " + string.Join(", ", unit.ActiveStatuses.Select(StatusText)), 28, TextColor);
            var definition = unit.Definition;
            Ability("Attack", definition.attack);
            Ability("Skill", definition.support);
            Ability(hero ? "Rage" : "Special", definition.special);
        }

        void ShowPotion(PotionDefinition potion, int left)
        {
            Open();
            Line(potion.displayName, 46, Ui.GoldColor, true);
            Line(potion.description, 30, Color.white);
            Line($"Target: {potion.TargetText}   ·   Left: {left}", 28, MutedColor);
        }

        void Ability(string kind, AbilityDefinition ability)
        {
            if (ability == null) return;
            Line($"<color=#A8A8B8>{kind}</color>  <b>{ability.displayName}</b>  {ability.Summary}", 28, TextColor);
        }

        static string StatusText(ActiveStatus status)
        {
            string name = status.Stacks > 1 ? $"{status.Definition.displayName} ×{status.Stacks}" : status.Definition.displayName;
            return $"{name} ({status.Turns} {(status.Turns == 1 ? "turn" : "turns")})";
        }

        void Open()
        {
            panel = Ui.Rect("Info", parent, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -104f), new Vector2(Width, 0f));
            Ui.Panel(panel, PanelColor);
            panel.SetAsLastSibling();
            var layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(40, 40, 26, 30);
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        void Line(string text, int size, Color color, bool bold = false)
        {
            var label = Ui.Wrapped(Ui.Rect("Line", panel, Vector2.zero, Vector2.zero, Vector2.zero, Vector2.zero), text, size, TextAnchor.UpperLeft, color);
            label.supportRichText = true;
            label.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
        }
    }
}
