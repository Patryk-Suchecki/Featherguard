using System.Collections;
using System.IO;
using Featherguard.Battle;
using Featherguard.Feel;
using Featherguard.Meta;
using Featherguard.Run;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public class GameBootstrap : MonoBehaviour
    {
        [SerializeField] GameConfig config;
        [SerializeField] Camera battleCamera;
        [SerializeField] CameraShake cameraShake;

        static MetaProgression meta;

        FeedbackHub feedback;
        BattleHud hud;
        RunSession session;
        BattleInstance battle;

        MetaProgression Meta => meta ??= new MetaProgression(config.meta, new JsonFileSaveStore(Path.Combine(Application.persistentDataPath, config.meta.saveFileName)));

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetOnLoad() => meta = null;

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            feedback = gameObject.AddComponent<FeedbackHub>();
            feedback.Init(config.sounds, cameraShake, config.particleMaterial, config.feedback);
            Ui.Sprites = config.ui;
            hud = gameObject.AddComponent<BattleHud>();
            hud.Init(battleCamera, AbandonRun);

            session = RunSession.Current;
            if (session == null)
            {
                yield return CampScreen.Show(hud, Meta);
                session = RunSession.Start(config.run, config.heroes);
                Meta.ApplyTo(session);
            }
            hud.BindWallet(session.Wallet);

            battle = new BattleInstance(session.CreateBattleSetup(), new BattleServices
            {
                Rules = config.rules,
                Factory = new UnitFactory(config.rules, config.flashMaterial, config.ringMaterial),
                View = hud,
                Feedback = feedback,
                Host = this,
                Camera = battleCamera,
            });
            battle.EnemyKilled += OnEnemyKilled;
            hud.BindBattle(battle.Commands, battle.Rage);

            yield return battle.Run();
            yield return Conclude(battle.Result);
        }

        void Update() => battle?.Tick();

        void OnEnemyKilled(BattleUnit enemy)
        {
            int gold = session.RecordKill();
            hud.Popup(enemy.Top + Vector3.up * 0.6f, $"+{gold} zł", PopupKind.Gold);
        }

        IEnumerator Conclude(BattleResult result)
        {
            if (!result.Victory)
            {
                RunEndScreen.Show(hud, session, false, Meta.RecordRun(session, false), NewRun);
                yield break;
            }
            session.RecordVictory(result);
            yield return new WaitForSeconds(0.8f);
            if (session.IsFinal)
            {
                RunEndScreen.Show(hud, session, true, Meta.RecordRun(session, true), NewRun);
                yield break;
            }
            yield return RewardSequence.Play(hud, session);
            session.Advance();
            Reload();
        }

        void AbandonRun()
        {
            if (RunSession.Current != null)
                Meta.RecordRun(RunSession.Current, false);
            NewRun();
        }

        void NewRun()
        {
            RunSession.End();
            Reload();
        }

        static void Reload()
        {
            GameClock.Paused = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
