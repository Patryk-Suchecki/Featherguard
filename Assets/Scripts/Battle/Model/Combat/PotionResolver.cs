namespace Featherguard.Battle
{
    public sealed class PotionResolver
    {
        readonly BattleState state;

        public PotionResolver(BattleState state) => this.state = state;

        public void Use(PotionDefinition potion, BattleUnit chosen)
        {
            var targets = PotionTargeting.Resolve(potion, chosen, state.Heroes, state.Enemies);
            state.Log.Emit(new PotionUsed { Potion = potion, Targets = targets });
            foreach (var effect in potion.effects)
            {
                if (effect == null) continue;
                if (effect.Once)
                {
                    effect.Apply(new PotionContext(state, chosen, potion));
                    continue;
                }
                foreach (var target in targets)
                    effect.Apply(new PotionContext(state, target, potion));
            }
        }
    }
}
