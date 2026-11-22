using System;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Core;

namespace Featherguard.Meta
{
    public sealed class WorldProgress
    {
        readonly WorldDefinition world;
        readonly SaveData data;
        readonly ISaveStore store;
        readonly IRandomSource random;
        readonly Dictionary<LandDefinition, List<ActivityDefinition>> sideActivities = new();

        public WorldProgress(WorldDefinition world, SaveData data, ISaveStore store, IRandomSource random)
        {
            this.world = world;
            this.data = data;
            this.store = store;
            this.random = random;
            data.world ??= new WorldSave();
            data.world.storyWon = Math.Clamp(data.world.storyWon, 0, world.storyMissions.Length);
            string chosen = data.world.chosen;
            if (!RestoreSideActivities())
                Reroll();
            Chosen = OnMap(chosen);
        }

        public IReadOnlyList<LandDefinition> Lands => world.lands;
        public int StoryWon => data.world.storyWon;
        public int StoryCount => world.storyMissions.Length;
        public ActivityDefinition NextStory => StoryWon < StoryCount ? world.storyMissions[StoryWon] : null;
        public bool IsStoryComplete => NextStory == null;
        public ActivityDefinition Chosen { get; private set; }

        public bool IsOpen(LandDefinition land) => world.storyMissions.Take(Math.Min(StoryWon + 1, StoryCount)).Any(m => m.land == land);

        public int StoryWonIn(LandDefinition land) => world.storyMissions.Take(StoryWon).Count(m => m.land == land);

        public int StoryCountIn(LandDefinition land) => world.storyMissions.Count(m => m.land == land);

        public bool IsBossBeaten(LandDefinition land)
        {
            int boss = Array.FindLastIndex(world.storyMissions, m => m.land == land);
            return boss >= 0 && StoryWon > boss;
        }

        public IReadOnlyList<ActivityDefinition> ActivitiesIn(LandDefinition land)
        {
            var activities = new List<ActivityDefinition>();
            if (NextStory != null && NextStory.land == land) activities.Add(NextStory);
            if (sideActivities.TryGetValue(land, out var sides)) activities.AddRange(sides);
            return activities;
        }

        public bool IsStoryMission(ActivityDefinition activity) => activity != null && activity == NextStory;

        public ThreatRating Rate(ActivityDefinition activity) => Rate(activity, world.partyStrength);

        public ThreatRating Rate(ActivityDefinition activity, float partyStrength) => world.Rate(activity.threat - partyStrength);

        public bool IsDaring(ThreatRating rating) => rating >= ThreatRating.Dangerous;

        public float DaringMultiplier(ThreatRating rating) => IsDaring(rating) ? 1f + world.daringBonus : 1f;

        public void Win(ActivityDefinition activity)
        {
            if (IsStoryMission(activity)) data.world.storyWon++;
            Chosen = null;
            Reroll();
        }

        public bool Choose(ActivityDefinition activity)
        {
            if (activity == null || !ActivitiesIn(activity.land).Contains(activity)) return false;
            Chosen = activity;
            Save();
            return true;
        }

        public void ClearChoice()
        {
            Chosen = null;
            Save();
        }

        public void Reroll()
        {
            sideActivities.Clear();
            var drawn = new HashSet<ActivityDefinition>();
            foreach (var land in world.lands.Where(IsOpen))
            {
                int places = random.Range(world.minActivitiesPerLand, world.maxActivitiesPerLand + 1);
                if (NextStory != null && NextStory.land == land) places--;
                var pool = Drawable(land).Where(a => !drawn.Contains(a)).ToList();
                var picked = new List<ActivityDefinition>();
                while (picked.Count < places && pool.Count > 0)
                {
                    int index = random.Range(0, pool.Count);
                    picked.Add(pool[index]);
                    drawn.Add(pool[index]);
                    pool.RemoveAt(index);
                }
                sideActivities[land] = picked;
            }
            Save();
        }

        ActivityDefinition OnMap(string id) =>
            string.IsNullOrEmpty(id) ? null : world.lands.Where(IsOpen).SelectMany(ActivitiesIn).FirstOrDefault(a => a.name == id);

        IEnumerable<ActivityDefinition> Drawable(LandDefinition land) =>
            land.sideActivities.Where(a => a != null && a.availableAfter <= StoryWon && (a.type != ActivityType.BossFight || IsBossBeaten(land)) && !IsSchematicLearned(a));

        bool IsSchematicLearned(ActivityDefinition activity) =>
            activity.reward == RewardKind.Schematic && (activity.schematic == null || RecipeBook.IsKnown(activity.schematic, data));

        bool RestoreSideActivities()
        {
            var openLands = world.lands.Where(IsOpen).ToList();
            foreach (var offer in data.world.offers)
            {
                var land = openLands.FirstOrDefault(l => l.name == offer.land);
                if (land == null) return false;
                var drawable = Drawable(land).ToList();
                var activities = offer.activities.Select(id => drawable.FirstOrDefault(a => a.name == id)).ToList();
                if (activities.Any(a => a == null)) return false;
                sideActivities[land] = activities;
            }
            return openLands.All(sideActivities.ContainsKey);
        }

        void Save()
        {
            data.world.offers = sideActivities
                .Select(pair => new LandOffer { land = pair.Key.name, activities = pair.Value.Select(a => a.name).ToList() })
                .ToList();
            data.world.chosen = Chosen != null ? Chosen.name : "";
            store.Save(data);
        }
    }
}
