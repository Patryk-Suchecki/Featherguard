namespace Featherguard.Battle
{
    public readonly struct EnemyDecision
    {
        public readonly AbilityDefinition Ability;
        public readonly BattleUnit Target;

        public EnemyDecision(AbilityDefinition ability, BattleUnit target)
        {
            Ability = ability;
            Target = target;
        }
    }

    public interface IEnemyBrain
    {
        EnemyDecision Decide(BattleUnit enemy, BattleState state);
    }
}
