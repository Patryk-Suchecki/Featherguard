namespace Featherguard.Battle.Presentation
{
    public static class CommandIntent
    {
        public static bool TryResolve(BattleUnit hero, BattleUnit droppedOn, bool moved, bool specialArmed, out AbilityDefinition ability, out BattleUnit target)
        {
            ability = null;
            target = null;
            if (hero == null) return false;

            bool tap = !moved && (droppedOn == null || droppedOn == hero);
            var definition = hero.Definition;
            if (specialArmed)
                ability = definition.special;
            else if (tap)
                ability = definition.support;
            else if (droppedOn != null)
                ability = droppedOn.Team == hero.Team ? definition.support : definition.attack;
            if (ability == null) return false;

            if (ability.TargetsAll)
                return tap || droppedOn != null && OnSide(ability, hero, droppedOn);

            target = tap ? hero : droppedOn;
            if (target != null && OnSide(ability, hero, target)) return true;
            target = null;
            return false;
        }

        static bool OnSide(AbilityDefinition ability, BattleUnit hero, BattleUnit unit) => ability.TargetsEnemies == (unit.Team != hero.Team);
    }
}
