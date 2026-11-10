using System.Collections;
using System.IO;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Feedback;
using Featherguard.Meta;
using Featherguard.Run;
using Featherguard.UI;
using UnityEngine;

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
        RunFlow flow;
        BattleController battle;
        ToonMaterials toon;

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
            var visuals = new BattleVisuals(config.presentation);
            flow = new RunFlow(hud, Meta);
            hud.Init(battleCamera, visuals, flow.Abandon);

            var session = RunSession.Current;
            if (session == null)
            {
                yield return CampScreen.Show(hud, Meta);
                session = RunSession.Start(config.run, config.heroes, UnityRandomSource.Shared);
                Meta.ApplyTo(session);
            }
            hud.BindWallet(session.Wallet);

            var setup = session.CreateBattleSetup();
            var testEncounter = TestEncounterSetting.Current;
            if (testEncounter != null)
                UseTestEncounter(setup, testEncounter);
            var model = new BattleModel(setup, config.rules, session.Random);
            toon ??= new ToonMaterials(config.toonMaterial);
            battle = new BattleController(model, new BattleServices
            {
                Config = config.presentation,
                Visuals = visuals,
                Factory = new UnitViewFactory(config.presentation, config.flashMaterial, config.ringMaterial, visuals.Units, toon),
                View = hud,
                Feedback = feedback,
                Host = this,
                UnitPicker = new PointerUnitPicker(battleCamera, config.presentation.tapTolerance),
            });
            flow.TrackKills(session, model, battle.Presenter);
            hud.BindBattle(battle.Commands, battle.Presenter.Rage);

            yield return battle.Run();
            yield return flow.Conclude(session, battle.Result);
        }

        void Update()
        {
            if (!GameClock.Paused) battle?.Tick();
        }

        void OnDestroy() => toon?.Dispose();

        static void UseTestEncounter(BattleSetup setup, EncounterDefinition encounter)
        {
            setup.Encounter = encounter;
            setup.EnemyHealthScale = encounter.enemyHealth;
            setup.EnemyDamageScale = encounter.enemyDamage;
            setup.Elite = false;
        }
    }
}
