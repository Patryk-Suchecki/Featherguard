using System.Collections;
using Featherguard.Battle;
using Featherguard.Core;
using Featherguard.Meta;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public sealed class ActivityFlow
    {
        readonly BattleHud hud;
        readonly WorldProgress world;
        readonly ActivityDefinition activity;
        readonly float afterVictory;
        readonly MaterialStorage storage;
        readonly MaterialConfig materials;
        readonly IRandomSource random;

        public ActivityFlow(BattleHud hud, WorldProgress world, ActivityDefinition activity, float afterVictory, MaterialStorage storage, MaterialConfig materials, IRandomSource random)
        {
            this.hud = hud;
            this.world = world;
            this.activity = activity;
            this.afterVictory = afterVictory;
            this.storage = storage;
            this.materials = materials;
            this.random = random;
        }

        public IEnumerator Conclude(BattleResult result)
        {
            if (!result.Victory)
            {
                ActivityResultScreen.ShowDefeat(hud, activity.displayName, world.Rate(activity).ToString(), Retry, Abandon);
                yield break;
            }
            var rating = world.Rate(activity);
            var drops = MaterialRewards.Roll(activity, world.DaringMultiplier(rating), materials, random);
            storage.Add(drops);
            string reward = Reward(rating);
            bool story = world.IsStoryMission(activity);
            world.Win(activity);
            yield return new WaitForSeconds(afterVictory);
            string progress = !story ? null : world.IsStoryComplete ? "The story is complete" : $"Story missions {world.StoryWon} / {world.StoryCount}";
            ActivityResultScreen.ShowVictory(hud, activity.displayName, reward, progress, drops, () => Load(GameScenes.WorldMap));
        }

        string Reward(ThreatRating rating)
        {
            if (activity.reward == RewardKind.None) return "No reward";
            string daring = world.IsDaring(rating) ? " (Daring +50%)" : "";
            return activity.reward == RewardKind.Gold
                ? $"+{ActivityBattle.Gold(activity, world.DaringMultiplier(rating))} gold{daring}"
                : $"Reward: {ActivityText.RewardName(activity.reward)}{daring}";
        }

        public void Abandon()
        {
            world.ClearChoice();
            Load(GameScenes.WorldMap);
        }

        static void Retry() => Load(GameScenes.Preparation);

        static void Load(string scene)
        {
            GameClock.Paused = false;
            SceneManager.LoadScene(scene);
        }
    }
}
