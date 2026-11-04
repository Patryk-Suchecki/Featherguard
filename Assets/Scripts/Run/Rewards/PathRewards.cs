using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;

namespace Featherguard.Run
{
    public static class PathRewards
    {
        public static List<PathNode> Choices(RunSession session)
        {
            var definition = session.Definition;
            if (session.NextIsFinal) return definition.beforeBossNodes.ToList();
            var remaining = definition.pathNodes.ToList();
            var picked = new List<PathNode>();
            while (picked.Count < definition.pathChoices && remaining.Count > 0)
            {
                var node = WeightedRandom.Pick(remaining, n => n.weight, session.Random);
                remaining.Remove(node);
                picked.Add(node);
            }
            return picked;
        }

        public static void ChooseElite(RunSession session) => session.NextElite = true;

        public static float Rest(RunSession session)
        {
            float heal = session.Economy.campfireHeal;
            session.Party.HealAll(heal);
            return heal;
        }

        public static List<UpgradeOffer> OpenChest(RunSession session)
        {
            var gained = new List<UpgradeOffer>();
            var reward = session.DrawOffers(1, session.Economy.chestRarity).FirstOrDefault();
            if (reward.IsValid) gained.Add(reward);
            var curse = session.DrawCurse();
            if (curse.IsValid) gained.Add(curse);
            foreach (var offer in gained)
                session.Apply(offer);
            return gained;
        }
    }
}
