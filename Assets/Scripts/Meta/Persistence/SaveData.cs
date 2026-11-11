using System;
using System.Collections.Generic;

namespace Featherguard.Meta
{
    [Serializable]
    public class PerkLevel
    {
        public string id;
        public int level;
    }

    [Serializable]
    public class SaveData
    {
        public int feathers;
        public int runs;
        public int wins;
        public int bestBattles;
        public List<PerkLevel> perks = new();
    }
}
