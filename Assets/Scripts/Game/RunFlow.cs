using System.Collections;
using System.Collections.Generic;
using Featherguard.Battle;
using Featherguard.Battle.Presentation;
using Featherguard.Core;
using Featherguard.Meta;
using Featherguard.Run;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Featherguard.Game
{
    public sealed class RunFlow
    {
        readonly BattleHud hud;
        readonly MetaProgression meta;
        readonly float afterVictory;
        readonly Dictionary<BattleUnit, int> killRewards = new();

        public RunFlow(BattleHud hud, MetaProgression meta, float afterVictory)
        {
            this.hud = hud;
            this.meta = meta;
            this.afterVictory = afterVictory;
        }

        public void TrackKills(RunSession session, BattleModel model, BattlePresenter presenter)
        {
            model.EnemyKilled += enemy => killRewards[enemy] = session.RecordKill();
            presenter.UnitDefeated += ShowKillReward;
        }

        public IEnumerator Conclude(RunSession session, BattleResult result)
        {
            if (!result.Victory)
            {
                Finish(session, false);
                yield break;
            }
            session.RecordVictory(result);
            yield return new WaitForSeconds(afterVictory);
            if (session.IsFinal)
            {
                Finish(session, true);
                yield break;
            }
            yield return RewardSequence.Play(hud, session);
            session.Advance();
            Reload();
        }

        public void Abandon()
        {
            if (RunSession.Current != null)
                meta.RecordRun(RunSession.Current, false);
            NewRun();
        }

        void ShowKillReward(UnitView unit)
        {
            if (killRewards.Remove(unit.Unit, out int gold))
                hud.Popup(unit.Top + Vector3.up * 0.6f, $"+{gold} gold", PopupKind.Gold);
        }

        void Finish(RunSession session, bool completed)
        {
            int feathers = meta.RecordRun(session, completed);
            RunSession.End();
            RunEndScreen.Show(hud, session, completed, feathers, NewRun);
        }

        static void NewRun()
        {
            RunSession.End();
            GameClock.Paused = false;
            SceneManager.LoadScene(GameScenes.Stronghold);
        }

        static void Reload()
        {
            GameClock.Paused = false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }
}
