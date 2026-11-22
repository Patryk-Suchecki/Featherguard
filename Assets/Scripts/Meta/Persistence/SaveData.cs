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
        public List<string> party = new();
        public WorldSave world = new();
        public List<MaterialCount> materials = new();
    }

    [Serializable]
    public class WorldSave
    {
        public int storyWon;
        public string chosen = "";
        public List<LandOffer> offers = new();
    }

    [Serializable]
    public class MaterialCount
    {
        public string id;
        public int count;
    }

    [Serializable]
    public class LandOffer
    {
        public string land;
        public List<string> activities = new();
    }
}
