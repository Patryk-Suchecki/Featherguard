using System.Collections;
using System.Linq;
using Featherguard.Feedback;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class BattleAnnouncer
    {
        const string PlayerTurnText = "Your turn";
        const string EnemyTurnText = "Enemy turn";
        static readonly Color EnrageColor = new(1f, 0.3f, 0.1f);

        readonly BattlePresentationConfig config;
        readonly IBannerView view;
        readonly ICombatFeedback feedback;
        readonly UnitStage stage;
        readonly RageMeter rage;

        public BattleAnnouncer(BattlePresentationConfig config, IBannerView view, ICombatFeedback feedback, UnitStage stage, RageMeter rage)
        {
            this.config = config;
            this.view = view;
            this.feedback = feedback;
            this.stage = stage;
            this.rage = rage;
        }

        public void Register(BattleEventRouter router)
        {
            router.On<WaveStarted>(StartWave);
            router.On<WaveSpawned>(_ => BattleEventRouter.Wait(config.afterSpawn));
            router.On<TurnStarted>(StartTurn);
            router.On<Enraged>(Enrage);
            router.OnInstant<RageChanged>(ShowRage);
            router.On<BattleEnded>(End);
        }

        IEnumerator StartWave(WaveStarted e)
        {
            if (e.Wave > 0) yield return BattleEventRouter.Wait(config.waveDelay);
            view.SetProgress(e.EncounterNumber, e.EncounterCount, e.Wave + 1, e.Waves);
            view.ShowBanner(WaveBanner(e));
        }

        static string WaveBanner(WaveStarted e)
        {
            if (e.Wave > 0)
                return e.Wave == e.Waves - 1 ? "Final wave!" : $"Wave {e.Wave + 1}";
            if (e.Encounter.boss) return $"BOSS: {e.Encounter.displayName}";
            return e.Elite ? $"ELITE: {e.Encounter.displayName}" : e.Encounter.displayName;
        }

        IEnumerator StartTurn(TurnStarted e)
        {
            bool heroes = e.Team == Team.Heroes;
            view.ShowBanner(heroes ? PlayerTurnText : EnemyTurnText);
            if (!heroes) yield return BattleEventRouter.Wait(config.enemyTurnDelay);
        }

        IEnumerator Enrage(Enraged e)
        {
            var boss = stage.ViewOf(e.Boss);
            view.Callout($"{e.Boss.Definition.displayName} goes berserk!");
            feedback.Play(SfxId.Rage, 1f, 0f);
            feedback.Shake(0.25f, 0.6f);
            if (boss != null)
            {
                feedback.Burst(boss.Center, EnrageColor, 40, 7f, 0.45f);
                boss.Animator.Play("Victory", 0.1f);
            }
            yield return BattleEventRouter.Wait(config.enrageHold);
        }

        void ShowRage(RageChanged e)
        {
            rage.Set(e.Value);
            if (e.BecameReady) feedback.Play(SfxId.Buff, 0.8f, 0f);
        }

        IEnumerator End(BattleEnded e)
        {
            yield return BattleEventRouter.Wait(config.endDelay);
            var winners = e.Victory ? Team.Heroes : Team.Enemies;
            foreach (var unit in stage.Views.Where(v => v.Team == winners && v.IsAlive))
                unit.Animator.Play("Victory", 0.15f);
            feedback.Play(e.Victory ? SfxId.Victory : SfxId.Defeat, 1f, 0f);
        }
    }
}
