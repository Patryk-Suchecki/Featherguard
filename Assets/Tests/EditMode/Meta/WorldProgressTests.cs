using System.Linq;
using Featherguard.Meta;
using NUnit.Framework;
using UnityEngine;

namespace Featherguard.Tests
{
    public class WorldProgressTests
    {
        LandDefinition fields;
        LandDefinition marshes;
        LandDefinition mountain;
        ActivityDefinition[] story;
        WorldDefinition world;
        MemorySaveStore store;
        MetaProgression meta;

        static LandDefinition Land(string name)
        {
            var land = ScriptableObject.CreateInstance<LandDefinition>();
            land.name = name;
            return land;
        }

        static ActivityDefinition Activity(string name, LandDefinition land, ActivityType type = ActivityType.SingleFight, int threat = 1, int availableAfter = 0)
        {
            var activity = ScriptableObject.CreateInstance<ActivityDefinition>();
            activity.name = name;
            activity.land = land;
            activity.type = type;
            activity.threat = threat;
            activity.availableAfter = availableAfter;
            return activity;
        }

        [SetUp]
        public void SetUp()
        {
            fields = Land("Fields");
            marshes = Land("Marshes");
            mountain = Land("Mountain");
            story = new[]
            {
                Activity("S1", fields), Activity("S2", fields), Activity("S3", marshes),
                Activity("S4", fields, ActivityType.BossFight), Activity("S5", mountain),
            };
            fields.sideActivities = new[]
            {
                Activity("F1", fields), Activity("F2", fields, ActivityType.WavesFight), Activity("F3", fields, ActivityType.EliteFight),
                Activity("FLate", fields, availableAfter: 3), Activity("FBoss", fields, ActivityType.BossFight),
            };
            marshes.sideActivities = new[] { Activity("M1", marshes), Activity("M2", marshes) };
            mountain.sideActivities = new[] { Activity("Mt1", mountain) };
            world = ScriptableObject.CreateInstance<WorldDefinition>();
            world.lands = new[] { fields, marshes, mountain };
            world.storyMissions = story;
            store = new MemorySaveStore();
            meta = new MetaProgression(ScriptableObject.CreateInstance<MetaConfig>(), store);
        }

        WorldProgress Create(params float[] rolls) => meta.CreateWorld(world, new FakeRandom(0f, rolls));

        [Test]
        public void NewWorld_HasOnlyTheFirstLandOpenAndTheFirstStoryMissionNext()
        {
            var progress = Create();

            Assert.AreSame(story[0], progress.NextStory);
            Assert.IsTrue(progress.IsOpen(fields));
            Assert.IsFalse(progress.IsOpen(marshes));
            Assert.IsFalse(progress.IsOpen(mountain));
        }

        [Test]
        public void WinningTheNextStoryMission_AdvancesTheStory()
        {
            var progress = Create();

            progress.Win(story[0]);

            Assert.AreSame(story[1], progress.NextStory);
            Assert.AreEqual(1, progress.StoryWon);
        }

        [Test]
        public void WinningASideActivity_DoesNotAdvanceTheStory()
        {
            var progress = Create();

            progress.Win(fields.sideActivities[0]);

            Assert.AreSame(story[0], progress.NextStory);
        }

        [Test]
        public void ALand_OpensWhenTheNextStoryMissionIsInItAndStaysOpen()
        {
            var progress = Create();
            progress.Win(story[0]);
            Assert.IsFalse(progress.IsOpen(marshes));

            progress.Win(story[1]);
            Assert.IsTrue(progress.IsOpen(marshes));

            progress.Win(story[2]);
            Assert.IsTrue(progress.IsOpen(marshes));
        }

        [Test]
        public void ALandWithTheStoryMission_CountsItAsOneOfItsPlaces()
        {
            var progress = Create(0.99f);

            var activities = progress.ActivitiesIn(fields);

            Assert.AreEqual(3, activities.Count);
            Assert.AreSame(story[0], activities[0]);
        }

        [Test]
        public void ALandShowsAtLeastOneActivity()
        {
            var progress = Create(0f);

            Assert.AreEqual(1, progress.ActivitiesIn(fields).Count);
        }

        [Test]
        public void SideActivities_RespectAvailableAfterAndTheBossRule()
        {
            var progress = meta.CreateWorld(world, new FakeRandom(0.99f));
            for (int i = 0; i < 20; i++)
            {
                progress.Reroll();
                var drawn = progress.ActivitiesIn(fields).ToList();
                CollectionAssert.DoesNotContain(drawn, fields.sideActivities[3]);
                CollectionAssert.DoesNotContain(drawn, fields.sideActivities[4]);
            }
        }

        [Test]
        public void ABeatenBoss_CanReturnAsASideActivity()
        {
            var progress = meta.CreateWorld(world, new FakeRandom(0.99f));
            foreach (var mission in story.Take(4))
                progress.Win(mission);

            Assert.IsTrue(progress.IsBossBeaten(fields));
            var drawable = Enumerable.Range(0, 30).SelectMany(_ => { progress.Reroll(); return progress.ActivitiesIn(fields); }).Distinct().ToList();
            CollectionAssert.Contains(drawable, fields.sideActivities[4]);
            CollectionAssert.Contains(drawable, fields.sideActivities[3]);
        }

        [Test]
        public void SideActivities_NeverRepeatOnTheMap()
        {
            var progress = Create(0.99f);
            progress.Win(story[0]);
            progress.Win(story[1]);

            var all = progress.Lands.SelectMany(progress.ActivitiesIn).ToList();

            Assert.AreEqual(all.Count, all.Distinct().Count());
        }

        [Test]
        public void Winning_RerollsTheSideActivities()
        {
            var progress = Create(0.99f, 0f, 0f, 0.99f, 0.99f, 0.99f);
            var before = progress.ActivitiesIn(fields).ToList();

            progress.Win(fields.sideActivities[0]);

            CollectionAssert.AreNotEqual(before, progress.ActivitiesIn(fields));
        }

        [Test]
        public void ThreatRating_FollowsTheThresholds()
        {
            var progress = Create();
            var activity = Activity("A", fields, threat: 5);

            Assert.AreEqual(ThreatRating.Easy, progress.Rate(activity, 7f));
            Assert.AreEqual(ThreatRating.Even, progress.Rate(activity, 4f));
            Assert.AreEqual(ThreatRating.Even, progress.Rate(activity, 6f));
            Assert.AreEqual(ThreatRating.Dangerous, progress.Rate(activity, 3f));
            Assert.AreEqual(ThreatRating.Madness, progress.Rate(activity, 1f));
        }

        [Test]
        public void Daring_AppliesOnlyAtDangerousOrMadness()
        {
            var progress = Create();

            Assert.AreEqual(1f, progress.DaringMultiplier(ThreatRating.Even));
            Assert.AreEqual(1.5f, progress.DaringMultiplier(ThreatRating.Dangerous));
            Assert.AreEqual(1.5f, progress.DaringMultiplier(ThreatRating.Madness));
        }

        [Test]
        public void ProgressAndSideActivities_AreSavedAndRestored()
        {
            var progress = Create(0.99f);
            progress.Win(story[0]);
            progress.Win(story[1]);
            var shown = progress.Lands.ToDictionary(l => l, l => progress.ActivitiesIn(l).ToList());

            var reloaded = new MetaProgression(meta.Config, store).CreateWorld(world, new FakeRandom(0f));

            Assert.AreEqual(2, reloaded.StoryWon);
            foreach (var land in progress.Lands)
                CollectionAssert.AreEqual(shown[land], reloaded.ActivitiesIn(land));
        }

        [Test]
        public void SavedSideActivitiesThatNoLongerExist_AreDrawnAgain()
        {
            var progress = Create(0.99f);
            meta.Data.world.offers[0].activities.Add("Removed");
            store.Save(meta.Data);

            var reloaded = new MetaProgression(meta.Config, store).CreateWorld(world, new FakeRandom(0f));

            Assert.IsTrue(reloaded.ActivitiesIn(fields).All(a => a != null));
            Assert.AreEqual(1, reloaded.ActivitiesIn(fields).Count);
        }
    }
}
