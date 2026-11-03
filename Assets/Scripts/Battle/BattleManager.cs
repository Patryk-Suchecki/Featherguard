using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Featherguard.Feel;
using Featherguard.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

namespace Featherguard.Battle
{
    public class BattleManager : MonoBehaviour
    {
        const float MaxRage = 100f;

        static readonly Vector3[] HeroSlots = { new(-2.3f, 0f, 0.45f), new(-3.7f, 0f, -0.55f), new(-5.0f, 0f, 0.6f) };
        static readonly Vector3[] EnemySlots = { new(2.3f, 0f, 0.45f), new(3.7f, 0f, -0.55f), new(5.0f, 0f, 0.6f) };

        static readonly Color SelectedRing = new(1f, 0.85f, 0.2f, 1f);
        static readonly Color EnemyRing = new(1f, 0.3f, 0.25f, 0.75f);
        static readonly Color AllyRing = new(0.35f, 1f, 0.5f, 0.7f);

        [SerializeField] BattleConfig config;
        [SerializeField] Camera battleCamera;
        [SerializeField] CameraShake cameraShake;

        readonly List<BattleUnit> heroes = new();
        readonly List<BattleUnit> enemies = new();
        BattleHud hud;
        EncounterDefinition encounter;
        IEnumerator pendingAction;
        float rage;
        int waveIndex;
        bool finished;
        bool waveAdvanced;

        public IReadOnlyList<BattleUnit> Heroes => heroes;
        public BattleUnit Selected { get; private set; }
        public bool SpecialArmed { get; private set; }
        public bool AcceptingInput { get; private set; }
        public float RageFraction => rage / MaxRage;
        public bool RageReady => rage >= MaxRage;

        IEnumerator Start()
        {
            Time.timeScale = 1f;
            gameObject.AddComponent<HitFeedback>().Init(config.sounds, cameraShake);
            gameObject.AddComponent<Vfx>().Init(config.particleMaterial);
            hud = gameObject.AddComponent<BattleHud>();
            hud.Init(this, battleCamera);
            if (!RunState.Active)
            {
                yield return CampScreen.Show(hud);
                RunState.Begin(config.run, config.heroes);
            }
            encounter = RunState.Current;
            hud.RefreshGold();
            rage = RunState.StartRage;
            RunState.StartRage = 0f;

            for (int i = 0; i < config.heroes.Length; i++)
            {
                var hero = BattleUnit.Spawn(config.heroes[i], HeroSlots[i], config);
                foreach (var modifier in RunState.ModifiersOf(hero.Definition))
                    hero.AddModifier(modifier);
                hero.SetHealth(RunState.StoredHealth(hero.Definition) ?? hero.MaxHealth);
                AddUnit(heroes, hero);
            }
            hud.BuildHeroCards();

            yield return SpawnWave(0);
            yield return BattleLoop();
        }

        void Update()
        {
            if (!AcceptingInput || pendingAction != null) return;
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return;
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject()) return;

            var ray = battleCamera.ScreenPointToRay(pointer.position.ReadValue());
            if (!Physics.Raycast(ray, out var hit, 100f)) return;
            var unit = hit.collider.GetComponentInParent<BattleUnit>();
            if (unit != null && unit.IsAlive)
                OnUnitClicked(unit);
        }

        public void SelectFromCard(BattleUnit hero)
        {
            if (!AcceptingInput || pendingAction != null || !IsReady(hero)) return;
            HitFeedback.Instance.Play(SfxId.Click);
            Select(hero);
        }

        public void ToggleSpecial()
        {
            if (!AcceptingInput || pendingAction != null || !RageReady || Selected == null) return;
            var special = Selected.Definition.special;
            if (special.TargetsAll)
            {
                SpecialArmed = true;
                Use(Selected, special, null);
                return;
            }
            SpecialArmed = !SpecialArmed;
            HitFeedback.Instance.Play(SfxId.Click);
            RefreshHighlights();
        }

        public void ContinueRun()
        {
            RunState.Advance();
            Reload();
        }

        public void NewRun()
        {
            RunState.End();
            Reload();
        }

        static void Reload()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        void OnUnitClicked(BattleUnit unit)
        {
            if (Selected == null)
            {
                if (unit.Team == Team.Heroes && IsReady(unit))
                    Select(unit);
                return;
            }

            var ability = SpecialArmed ? Selected.Definition.special : null;
            if (unit.Team == Team.Enemies)
                ability ??= Selected.Definition.attack;
            else
                ability ??= Selected.Definition.support;

            if (ability == null || ability.TargetsEnemies != (unit.Team == Team.Enemies)) return;
            if (ability.TargetsEnemies && !ability.TargetsAll && !unit.Has(StatusType.Taunt) && enemies.Any(e => e.IsAlive && e.Has(StatusType.Taunt)))
            {
                hud.Popup(unit.Top, "Prowokacja!", PopupStyle.Buff);
                return;
            }
            Use(Selected, ability, unit);
        }

        void Use(BattleUnit actor, AbilityDefinition ability, BattleUnit target)
        {
            if (SpecialArmed)
            {
                rage = 0f;
                SpecialArmed = false;
            }
            actor.HasActed = true;
            pendingAction = Execute(actor, ability, target);
        }

        void Select(BattleUnit hero)
        {
            Selected = hero;
            SpecialArmed = false;
            RefreshHighlights();
        }

        void RefreshHighlights()
        {
            foreach (var unit in heroes.Concat(enemies))
                unit.SetRing(null);
            if (Selected == null || !AcceptingInput) return;

            var ability = SpecialArmed ? Selected.Definition.special : null;
            bool showEnemies = ability == null || ability.TargetsEnemies;
            bool showAllies = ability == null ? Selected.Definition.support != null : !ability.TargetsEnemies;

            if (showEnemies)
            {
                bool taunted = (ability == null || !ability.TargetsAll) && enemies.Any(e => e.IsAlive && e.Has(StatusType.Taunt));
                foreach (var enemy in enemies.Where(e => e.IsAlive && (!taunted || e.Has(StatusType.Taunt))))
                    enemy.SetRing(EnemyRing);
            }
            if (showAllies)
                foreach (var hero in heroes.Where(h => h.IsAlive))
                    hero.SetRing(AllyRing);
            Selected.SetRing(SelectedRing);
        }

        IEnumerator BattleLoop()
        {
            while (true)
            {
                yield return PlayerTurn();
                yield return AfterTurn();
                if (finished) yield break;
                if (waveAdvanced) continue;

                yield return EnemyTurn();
                yield return AfterTurn();
                if (finished) yield break;
            }
        }

        IEnumerator AfterTurn()
        {
            waveAdvanced = false;
            if (!AnyAlive(heroes))
            {
                finished = true;
                yield return EndBattle(false);
            }
            else if (!AnyAlive(enemies))
            {
                if (waveIndex + 1 >= encounter.waves.Length)
                {
                    finished = true;
                    yield return EndBattle(true);
                    yield break;
                }
                waveAdvanced = true;
                yield return new WaitForSeconds(0.6f);
                yield return SpawnWave(waveIndex + 1);
            }
        }

        IEnumerator BeginTurn(List<BattleUnit> side)
        {
            foreach (var unit in side.Where(u => u.IsAlive).ToList())
            {
                var poison = unit.Statuses.FirstOrDefault(st => st.Type == StatusType.Poison);
                if (poison != null)
                {
                    int dealt = unit.ApplyDamage(poison.Potency);
                    unit.Reaction.Flash();
                    Vfx.Instance.Rise(unit.transform.position, StatusEffect.Tint(StatusType.Poison), unit.Radius);
                    HitFeedback.Instance.Play(SfxId.Hit, 0.5f);
                    hud.Popup(unit.Top, dealt.ToString(), PopupStyle.Damage);
                    if (unit.IsAlive)
                        unit.Animator.Play("Hit");
                    else
                        Kill(unit);
                    yield return new WaitForSeconds(0.35f);
                }
                if (unit.IsAlive && unit.Has(StatusType.Stun))
                {
                    hud.Popup(unit.Top, "Ogłuszony", PopupStyle.Buff);
                    yield return new WaitForSeconds(0.25f);
                }
            }
        }

        static void EndTurn(List<BattleUnit> side)
        {
            foreach (var unit in side.Where(u => u.IsAlive))
                unit.EndTurn();
        }

        IEnumerator PlayerTurn()
        {
            hud.ShowBanner("Twoja tura");
            yield return BeginTurn(heroes);
            foreach (var hero in heroes)
            {
                hero.HasActed = hero.Has(StatusType.Stun);
                hero.NotifyChanged();
            }
            AcceptingInput = true;
            Select(FirstReadyHero());

            while (AnyAlive(enemies) && heroes.Any(IsReady))
            {
                if (pendingAction != null)
                {
                    AcceptingInput = false;
                    RefreshHighlights();
                    yield return pendingAction;
                    pendingAction = null;
                    yield return CheckEnrage();
                    if (!AnyAlive(enemies)) break;
                    AcceptingInput = true;
                    Select(FirstReadyHero());
                }
                yield return null;
            }

            AcceptingInput = false;
            Select(null);
            EndTurn(heroes);
        }

        IEnumerator EnemyTurn()
        {
            hud.ShowBanner("Tura wroga");
            yield return new WaitForSeconds(0.8f);
            yield return BeginTurn(enemies);
            foreach (var enemy in enemies.ToList())
            {
                if (!enemy.IsAlive || enemy.Has(StatusType.Stun)) continue;
                if (!AnyAlive(heroes)) break;
                var (ability, target) = ChooseEnemyAction(enemy);
                enemy.TurnsTaken++;
                yield return Execute(enemy, ability, target);
                yield return CheckEnrage();
                yield return new WaitForSeconds(0.2f);
            }
            EndTurn(enemies);
        }

        IEnumerator CheckEnrage()
        {
            foreach (var boss in enemies.Where(e => e.IsAlive && !e.Enraged && e.Definition.enrageAt > 0f).ToList())
            {
                if (boss.Health > boss.MaxHealth * boss.Definition.enrageAt) continue;
                boss.Enraged = true;
                hud.Callout($"{boss.Definition.displayName} wpada w szał!");
                HitFeedback.Instance.Play(SfxId.Rage, 1f, 0f);
                HitFeedback.Instance.Shake(0.25f, 0.6f);
                Vfx.Instance.Burst(boss.Center, new Color(1f, 0.3f, 0.1f), 40, 7f, 0.45f);
                boss.AddModifier(new StatModifier(StatType.Damage, multiply: 0.4f));
                boss.Animator.Play("Victory", 0.1f);
                yield return new WaitForSeconds(0.9f);
                foreach (var summon in boss.Definition.enrageSummons)
                {
                    int slot = System.Array.FindIndex(EnemySlots, s => !enemies.Any(e => e.IsAlive && Vector3.Distance(e.HomePosition, s) < 0.1f));
                    if (slot < 0) break;
                    foreach (var fallen in enemies.Where(e => !e.IsAlive && Vector3.Distance(e.HomePosition, EnemySlots[slot]) < 0.1f).ToList())
                    {
                        enemies.Remove(fallen);
                        StartCoroutine(Sink(fallen));
                    }
                    var unit = BattleUnit.Spawn(summon, EnemySlots[slot] + Vector3.right * 6f, config);
                    unit.HomePosition = EnemySlots[slot];
                    unit.AddModifier(new StatModifier(StatType.MaxHealth, multiply: encounter.enemyHealth - 1f));
                    unit.AddModifier(new StatModifier(StatType.Damage, multiply: encounter.enemyDamage - 1f));
                    unit.SetHealth(unit.MaxHealth);
                    AddUnit(enemies, unit);
                    HitFeedback.Instance.Play(SfxId.Whoosh, 0.6f);
                    StartCoroutine(HopTo(unit, EnemySlots[slot], 0.55f, 1.1f));
                    yield return new WaitForSeconds(0.25f);
                }
                yield return new WaitForSeconds(0.5f);
            }
        }

        (AbilityDefinition, BattleUnit) ChooseEnemyAction(BattleUnit enemy)
        {
            var def = enemy.Definition;
            if (def.special != null && def.specialEvery > 0 && enemy.TurnsTaken % def.specialEvery == 0)
                return (def.special, null);
            var support = enemy.Definition.support;
            if (support != null && support.effect == AbilityEffect.Status && Random.value < 0.45f)
            {
                var unshielded = enemies.Where(e => e.IsAlive && !e.Has(StatusType.Shield)).ToList();
                if (unshielded.Count > 0)
                    return (support, unshielded[Random.Range(0, unshielded.Count)]);
            }
            if (support != null && support.effect == AbilityEffect.Heal)
            {
                var wounded = enemies
                    .Where(e => e.IsAlive && e.Health < e.MaxHealth * 0.6f)
                    .OrderBy(e => (float)e.Health / e.MaxHealth)
                    .FirstOrDefault();
                if (wounded != null && Random.value < 0.75f)
                    return (support, wounded);
            }
            var attack = enemy.Definition.attack;
            var living = heroes.Where(h => h.IsAlive).ToList();
            var taunting = living.Where(h => h.Has(StatusType.Taunt)).ToList();
            if (taunting.Count > 0)
                return (attack, taunting[Random.Range(0, taunting.Count)]);

            float expected = attack.power * attack.hits * enemy.Stat(StatType.Damage);
            var weights = living.Select(h =>
            {
                float weight = 1f + 2f * (1f - (float)h.Health / h.MaxHealth);
                if (expected * h.Stat(StatType.DamageTaken) >= h.Health) weight += 3f;
                if (h.Has(StatusType.Shield)) weight *= 0.5f;
                return weight;
            }).ToList();
            float roll = Random.value * weights.Sum();
            for (int i = 0; i < living.Count; i++)
            {
                roll -= weights[i];
                if (roll <= 0f) return (attack, living[i]);
            }
            return (attack, living[^1]);
        }

        IEnumerator SpawnWave(int index)
        {
            waveIndex = index;
            foreach (var fallen in enemies)
                StartCoroutine(Sink(fallen));
            enemies.Clear();

            var waves = encounter.waves;
            hud.SetProgress(RunState.EncounterIndex + 1, RunState.EncounterCount, index + 1, waves.Length);
            if (index == 0)
                hud.ShowBanner(encounter.boss ? $"BOSS: {encounter.displayName}" : RunState.CurrentElite ? $"ELITA: {encounter.displayName}" : encounter.displayName);
            else
                hud.ShowBanner(index == waves.Length - 1 ? "Ostatnia fala!" : $"Fala {index + 1}");

            var wave = waves[index].enemies;
            for (int i = 0; i < wave.Length; i++)
            {
                var unit = BattleUnit.Spawn(wave[i], EnemySlots[i] + Vector3.right * 6f, config);
                float elite = RunState.CurrentElite ? 1.35f : 1f;
                unit.AddModifier(new StatModifier(StatType.MaxHealth, multiply: encounter.enemyHealth * elite - 1f));
                unit.AddModifier(new StatModifier(StatType.Damage, multiply: encounter.enemyDamage * elite - 1f));
                unit.SetHealth(unit.MaxHealth);
                unit.HomePosition = EnemySlots[i];
                AddUnit(enemies, unit);
                HitFeedback.Instance.Play(SfxId.Whoosh, 0.5f);
                StartCoroutine(HopTo(unit, EnemySlots[i], 0.55f, 1.1f));
                yield return new WaitForSeconds(0.18f);
            }
            yield return new WaitForSeconds(0.8f);
        }

        IEnumerator Execute(BattleUnit actor, AbilityDefinition ability, BattleUnit target)
        {
            var targets = ResolveTargets(actor, ability, target);
            if (targets.Count == 0) yield break;

            if (ability == actor.Definition.special)
            {
                hud.Callout(ability.displayName);
                HitFeedback.Instance.Play(SfxId.Rage);
                yield return new WaitForSeconds(0.5f);
            }

            switch (ability.delivery)
            {
                case AbilityDelivery.Melee:
                {
                    var victim = targets[0];
                    var strikePosition = victim.transform.position - actor.Facing * (victim.Radius + actor.Radius + 0.3f);
                    HitFeedback.Instance.Play(SfxId.Whoosh, 0.6f);
                    yield return HopTo(actor, strikePosition, 0.3f, 0.55f);
                    yield return actor.Animator.PlayUntilHitFrame(ability.animationState);
                    HitFeedback.Instance.Play(ability.castSound);
                    for (int i = 0; i < ability.hits; i++)
                    {
                        foreach (var t in targets.Where(t => t.IsAlive))
                            Resolve(actor, ability, t);
                        if (i < ability.hits - 1)
                            yield return new WaitForSeconds(0.15f);
                    }
                    yield return new WaitForSeconds(0.4f);
                    yield return HopTo(actor, actor.HomePosition, 0.32f, 0.45f);
                    break;
                }
                case AbilityDelivery.Arrow:
                case AbilityDelivery.Magic:
                {
                    yield return actor.Animator.PlayUntilHitFrame(ability.animationState);
                    var flights = new List<Coroutine>();
                    for (int i = 0; i < ability.hits; i++)
                    {
                        HitFeedback.Instance.Play(ability.castSound);
                        foreach (var t in targets)
                            flights.Add(StartCoroutine(Fly(actor, ability, t)));
                        yield return new WaitForSeconds(0.12f);
                    }
                    foreach (var flight in flights)
                        yield return flight;
                    break;
                }
                default:
                {
                    yield return actor.Animator.PlayUntilHitFrame(ability.animationState);
                    HitFeedback.Instance.Play(ability.castSound);
                    foreach (var t in targets)
                        Resolve(actor, ability, t);
                    break;
                }
            }

            if (ability.effect == AbilityEffect.Damage)
                actor.RemoveStatus(StatusType.Empower);
            if (actor.IsAlive)
                foreach (var application in ability.statuses.Where(a => a.onSelf))
                    TryApplyStatus(actor, actor, application);
            yield return new WaitForSeconds(0.35f);
        }

        List<BattleUnit> ResolveTargets(BattleUnit actor, AbilityDefinition ability, BattleUnit target)
        {
            if (ability.TargetsAll)
            {
                var side = ability.TargetsEnemies ? Opponents(actor) : Allies(actor);
                return side.Where(u => u.IsAlive).ToList();
            }
            return target != null && target.IsAlive ? new List<BattleUnit> { target } : new List<BattleUnit>();
        }

        IEnumerator Fly(BattleUnit actor, AbilityDefinition ability, BattleUnit target)
        {
            bool arrow = ability.delivery == AbilityDelivery.Arrow;
            Vector3 from = actor.transform.position + Vector3.up * actor.Height * 0.5f + actor.Facing * 0.45f;
            Vector3 to = target.Center + Random.insideUnitSphere * 0.12f;

            Transform projectile;
            ParticleSystem trail = null;
            if (arrow)
            {
                projectile = Instantiate(config.arrowPrefab).transform;
            }
            else
            {
                projectile = new GameObject("Bolt").transform;
                projectile.position = from;
                trail = Vfx.Instance.Trail(projectile, ability.fxColor);
            }

            float distance = Vector3.Distance(from, to);
            float duration = distance / (arrow ? 16f : 10f);
            float arc = distance * (arrow ? 0.12f : 0.05f);
            Vector3 previous = from;
            for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                Vector3 position = Vector3.Lerp(from, to, t) + Vector3.up * arc * 4f * t * (1f - t);
                projectile.position = position;
                if (arrow && position != previous)
                    projectile.rotation = Quaternion.LookRotation(previous - position);
                previous = position;
                yield return null;
            }

            if (trail != null)
                Vfx.Instance.Release(trail);
            Destroy(projectile.gameObject);
            if (target.IsAlive)
                Resolve(actor, ability, target);
        }

        void Resolve(BattleUnit actor, AbilityDefinition ability, BattleUnit target)
        {
            var feedback = HitFeedback.Instance;
            switch (ability.effect)
            {
                case AbilityEffect.Damage:
                {
                    bool crit = Random.value < actor.Stat(StatType.CritChance);
                    float amount = ability.power * actor.Stat(StatType.Damage) * Random.Range(0.9f, 1.1f);
                    if (crit) amount *= actor.Stat(StatType.CritDamage);
                    int dealt = target.ApplyDamage(Mathf.RoundToInt(amount));
                    bool heavy = ability.heavy || crit;

                    var contact = target.Center - actor.Facing * target.Radius * 0.7f + Vector3.back * target.Radius;
                    feedback.Impact(contact, ability.fxColor, heavy);
                    feedback.Play(ability.impactSound);
                    target.Reaction.Flash();
                    target.Reaction.Knockback(actor.Facing, heavy ? 0.4f : 0.22f);
                    hud.Popup(target.Top, crit ? $"{dealt}!" : dealt.ToString(), crit ? PopupStyle.Crit : PopupStyle.Damage);
                    AddRage(dealt * (actor.Team == Team.Heroes ? 0.45f * actor.Stat(StatType.RageGain) : 0.25f));
                    float lifesteal = actor.Stat(StatType.Lifesteal);
                    if (lifesteal > 0f && actor.IsAlive)
                    {
                        int stolen = actor.ApplyHeal(Mathf.RoundToInt(dealt * lifesteal));
                        if (stolen > 0)
                            hud.Popup(actor.Top, $"+{stolen}", PopupStyle.Heal);
                    }

                    if (target.IsAlive)
                        target.Animator.Play("Hit");
                    else
                        Kill(target);
                    break;
                }
                case AbilityEffect.Heal:
                {
                    int healed = target.ApplyHeal(Mathf.RoundToInt(ability.power * actor.Stat(StatType.Healing)));
                    Vfx.Instance.Rise(target.transform.position, ability.fxColor, target.Radius);
                    feedback.Play(ability.impactSound);
                    hud.Popup(target.Top, $"+{healed}", PopupStyle.Heal);
                    break;
                }
                case AbilityEffect.Status:
                {
                    Vfx.Instance.Rise(target.transform.position, ability.fxColor, target.Radius);
                    feedback.Play(ability.impactSound);
                    break;
                }
            }

            if (target.IsAlive)
                foreach (var application in ability.statuses.Where(a => !a.onSelf))
                    TryApplyStatus(actor, target, application);
        }

        void TryApplyStatus(BattleUnit actor, BattleUnit target, StatusApplication application)
        {
            if (Random.value > application.chance) return;
            target.AddStatus(application, target.Team == actor.Team);
            hud.Popup(target.Top + Vector3.up * 0.35f, StatusEffect.Announcement(application.type), PopupStyle.Buff);
        }

        void Kill(BattleUnit unit)
        {
            unit.Animator.Play("Death", 0.04f);
            unit.SetRing(null);
            unit.ClearStatuses();
            HitFeedback.Instance.Play(SfxId.Death);
            if (unit.Team == Team.Enemies)
            {
                RunState.Kills++;
                RunState.AddGold(RunState.Run.goldPerKill);
                hud.RefreshGold();
                hud.Popup(unit.Top + Vector3.up * 0.6f, $"+{RunState.Run.goldPerKill} zł", PopupStyle.Gold);
            }
        }

        void AddRage(float amount)
        {
            bool wasReady = RageReady;
            rage = Mathf.Min(MaxRage, rage + amount);
            if (!wasReady && RageReady)
                HitFeedback.Instance.Play(SfxId.Buff, 0.8f, 0f);
        }

        IEnumerator EndBattle(bool victory)
        {
            yield return new WaitForSeconds(0.6f);
            foreach (var unit in victory ? heroes : enemies)
                if (unit.IsAlive)
                    unit.Animator.Play("Victory", 0.15f);
            HitFeedback.Instance.Play(victory ? SfxId.Victory : SfxId.Defeat, 1f, 0f);
            if (!victory)
            {
                hud.ShowRunEnd(false, MetaProgress.RecordRun(false));
                yield break;
            }
            RunState.RecordVictory(heroes);
            hud.RefreshGold();
            yield return new WaitForSeconds(0.8f);
            if (RunState.IsFinal)
                hud.ShowRunEnd(true, MetaProgress.RecordRun(true));
            else
                RewardFlow.Play(hud, this);
        }

        IEnumerator HopTo(BattleUnit unit, Vector3 destination, float duration, float height)
        {
            Vector3 start = unit.transform.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                float eased = Mathf.SmoothStep(0f, 1f, t);
                unit.transform.position = Vector3.Lerp(start, destination, eased) + Vector3.up * height * 4f * t * (1f - t);
                yield return null;
            }
            unit.transform.position = destination;
        }

        IEnumerator Sink(BattleUnit unit)
        {
            hud.RemoveUnitBar(unit);
            Vector3 start = unit.transform.position;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.6f)
            {
                unit.transform.position = start + Vector3.down * t * 1.2f;
                yield return null;
            }
            Destroy(unit.gameObject);
        }

        void AddUnit(List<BattleUnit> list, BattleUnit unit)
        {
            list.Add(unit);
            hud.AddUnitBar(unit);
        }

        List<BattleUnit> Opponents(BattleUnit unit) => unit.Team == Team.Heroes ? enemies : heroes;
        List<BattleUnit> Allies(BattleUnit unit) => unit.Team == Team.Heroes ? heroes : enemies;
        BattleUnit FirstReadyHero() => heroes.FirstOrDefault(IsReady);
        static bool IsReady(BattleUnit unit) => unit.IsAlive && !unit.HasActed;
        static bool AnyAlive(List<BattleUnit> units) => units.Any(u => u.IsAlive);
    }
}
