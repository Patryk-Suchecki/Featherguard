using System;
using UnityEngine;
using UnityEngine.Scripting.APIUpdating;

namespace Featherguard.Run
{
    public interface IRewardQueue
    {
        void QueueCard(Rarity minimum);
    }

    [Serializable]
    public abstract class SlotReward
    {
        public string text;
        public virtual bool Positive => true;
        public abstract void Grant(RunSession session, IRewardQueue queue);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class GoldReward : SlotReward
    {
        public int amount = 20;

        public override void Grant(RunSession session, IRewardQueue queue) => session.Wallet.Earn(amount);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class HealReward : SlotReward
    {
        [Range(0f, 1f)] public float fraction = 0.25f;

        public override void Grant(RunSession session, IRewardQueue queue) => session.Party.HealAll(fraction);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class CardReward : SlotReward
    {
        public Rarity minimum;

        public override void Grant(RunSession session, IRewardQueue queue) => queue.QueueCard(minimum);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class StartRageReward : SlotReward
    {
        public float amount = 40f;

        public override void Grant(RunSession session, IRewardQueue queue) => session.StartRage = Mathf.Max(session.StartRage, amount);
    }

    [Serializable, MovedFrom(true, sourceAssembly: "Featherguard")]
    public sealed class PenaltyReward : SlotReward
    {
        [Range(0f, 1f)] public float healthLoss;
        public int goldLoss = 10;

        public override bool Positive => false;

        public override void Grant(RunSession session, IRewardQueue queue)
        {
            if (healthLoss > 0f) session.Party.HurtAll(healthLoss);
            session.Wallet.Lose(goldLoss);
        }
    }
}
