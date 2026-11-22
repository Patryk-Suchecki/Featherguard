using System;
using System.Collections.Generic;

namespace Featherguard.Meta
{
    [Serializable]
    public class SaveData
    {
        public List<string> party = new();
        public WorldSave world = new();
    }

    [Serializable]
    public class WorldSave
    {
        public int storyWon;
        public string chosen = "";
        public List<LandOffer> offers = new();
    }

    [Serializable]
    public class LandOffer
    {
        public string land;
        public List<string> activities = new();
    }
}
