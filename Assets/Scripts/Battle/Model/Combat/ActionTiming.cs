namespace Featherguard.Battle
{
    public enum TimingResult { Miss, Good, Perfect }

    public readonly struct ActionTiming
    {
        public static readonly ActionTiming None = default;

        public readonly TimingResult Attack;
        public readonly TimingResult Block;

        public ActionTiming(TimingResult attack, TimingResult block)
        {
            Attack = attack;
            Block = block;
        }

        public bool AnyPerfect => Attack == TimingResult.Perfect || Block == TimingResult.Perfect;

        public static ActionTiming ForAttack(TimingResult result) => new(result, TimingResult.Miss);
        public static ActionTiming ForBlock(TimingResult result) => new(TimingResult.Miss, result);

        public float DamageMultiplier(BattleRules rules)
        {
            float bonus = Attack switch
            {
                TimingResult.Perfect => rules.perfectAttackBonus,
                TimingResult.Good => rules.goodAttackBonus,
                _ => 0f,
            };
            float reduction = Block switch
            {
                TimingResult.Perfect => rules.perfectBlockReduction,
                TimingResult.Good => rules.goodBlockReduction,
                _ => 0f,
            };
            return (1f + bonus) * (1f - reduction);
        }
    }
}
