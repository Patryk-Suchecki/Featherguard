using System.Collections;
using System.IO;
using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
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
        public const string TestEncounterPref = "Featherguard.TestEncounter";

        [SerializeField] GameConfig config;
        [SerializeField] Camera battleCamera;
        [SerializeField] CameraShake cameraShake;

        static MetaProgression meta;

        FeedbackHub feedback;
        BattleHud hud;
        RunSession session;
        BattleController battle;
        UnitViewFactory factory;
        readonly Dictionary<BattleUnit, int> killRewards = new();

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

            var setup = session.CreateBattleSetup();
            var testEncounter = TestEncounter();
            if (testEncounter != null)
                UseTestEncounter(setup, testEncounter);
            var model = new BattleModel(setup, config.rules);
            model.EnemyKilled += enemy => killRewards[enemy] = session.RecordKill();
            factory = new UnitViewFactory(config.presentation, config.flashMaterial, config.ringMaterial, config.toonMaterial);
            battle = new BattleController(model, new BattleServices
            {
                Config = config.presentation,
                Factory = factory,
                View = hud,
                Feedback = feedback,
                Host = this,
                Camera = battleCamera,
            });
            battle.Presenter.UnitDefeated += OnUnitDefeated;
            hud.BindBattle(battle.Commands, battle.Presenter.Rage);

            yield return battle.Run();
            yield return Conclude(battle.Result);
        }

        void Update() => battle?.Tick();

        void OnDestroy() => factory?.Dispose();

#if UNITY_EDITOR
        static EncounterDefinition TestEncounter()
        {
            string path = UnityEditor.EditorPrefs.GetString(TestEncounterPref, "");
            return path.Length > 0 ? UnityEditor.AssetDatabase.LoadAssetAtPath<EncounterDefinition>(path) : null;
        }
#else
        static EncounterDefinition TestEncounter() => null;
#endif

        static void UseTestEncounter(BattleSetup setup, EncounterDefinition encounter)
        {
            setup.Encounter = encounter;
            setup.EnemyHealthScale = encounter.enemyHealth;
            setup.EnemyDamageScale = encounter.enemyDamage;
            setup.Elite = false;
        }

        void OnUnitDefeated(UnitView unit)
        {
            if (killRewards.Remove(unit.Unit, out int gold))
                hud.Popup(unit.Top + Vector3.up * 0.6f, $"+{gold} gold", PopupKind.Gold);
        }

        IEnumerator Conclude(BattleResult result)
        {
            if (!result.Victory)
            {
                FinishRun(false);
                yield break;
            }
            session.RecordVictory(result);
            yield return new WaitForSeconds(0.8f);
            if (session.IsFinal)
            {
                FinishRun(true);
                yield break;
            }
            yield return RewardSequence.Play(hud, session);
            session.Advance();
            Reload();
        }

        void FinishRun(bool completed)
        {
            int feathers = Meta.RecordRun(session, completed);
            RunSession.End();
            RunEndScreen.Show(hud, session, completed, feathers, NewRun);
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
