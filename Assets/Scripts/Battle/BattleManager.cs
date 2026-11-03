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
        const float CritChance = 0.15f;

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
        IEnumerator pendingAction;
        float rage;
        int waveIndex;

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

            for (int i = 0; i < config.heroes.Length; i++)
                AddUnit(heroes, BattleUnit.Spawn(config.heroes[i], HeroSlots[i], config));
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

        public void Restart()
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
                foreach (var enemy in enemies.Where(e => e.IsAlive))
                    enemy.SetRing(EnemyRing);
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
                if (!AnyAlive(enemies))
                {
                    if (waveIndex + 1 >= config.waves.Length)
                    {
                        yield return EndBattle(true);
                        yield break;
                    }
                    yield return new WaitForSeconds(0.6f);
                    yield return SpawnWave(waveIndex + 1);
                    continue;
                }

                yield return EnemyTurn();
                if (!AnyAlive(heroes))
                {
                    yield return EndBattle(false);
                    yield break;
                }
            }
        }

        IEnumerator PlayerTurn()
        {
            foreach (var hero in heroes)
            {
                hero.HasActed = false;
                hero.Shielded = false;
                hero.NotifyChanged();
            }
            hud.ShowBanner("Twoja tura");
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
                    if (!AnyAlive(enemies)) break;
                    AcceptingInput = true;
                    Select(FirstReadyHero());
                }
                yield return null;
            }

            AcceptingInput = false;
            Select(null);
        }

        IEnumerator EnemyTurn()
        {
            hud.ShowBanner("Tura wroga");
            yield return new WaitForSeconds(0.8f);
            foreach (var enemy in enemies.ToList())
            {
                if (!enemy.IsAlive) continue;
                if (!AnyAlive(heroes)) yield break;
                var (ability, target) = ChooseEnemyAction(enemy);
                yield return Execute(enemy, ability, target);
                yield return new WaitForSeconds(0.2f);
            }
        }

        (AbilityDefinition, BattleUnit) ChooseEnemyAction(BattleUnit enemy)
        {
            var support = enemy.Definition.support;
            if (support != null && support.effect == AbilityEffect.Heal)
            {
                var wounded = enemies
                    .Where(e => e.IsAlive && e.Health < e.MaxHealth * 0.6f)
                    .OrderBy(e => (float)e.Health / e.MaxHealth)
                    .FirstOrDefault();
                if (wounded != null && Random.value < 0.75f)
                    return (support, wounded);
            }
            var living = heroes.Where(h => h.IsAlive).ToList();
            return (enemy.Definition.attack, living[Random.Range(0, living.Count)]);
        }

        IEnumerator SpawnWave(int index)
        {
            waveIndex = index;
            foreach (var fallen in enemies)
                StartCoroutine(Sink(fallen));
            enemies.Clear();

            hud.SetWave(index + 1, config.waves.Length);
            hud.ShowBanner(index == config.waves.Length - 1 ? "Ostatnia fala!" : $"Fala {index + 1}");

            var wave = config.waves[index].enemies;
            for (int i = 0; i < wave.Length; i++)
            {
                var unit = BattleUnit.Spawn(wave[i], EnemySlots[i] + Vector3.right * 6f, config);
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

            if (ability.effect == AbilityEffect.Damage && actor.Empowered)
            {
                actor.Empowered = false;
                actor.NotifyChanged();
            }
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
                    bool crit = Random.value < CritChance;
                    float amount = ability.power * Random.Range(0.9f, 1.1f);
                    if (actor.Empowered) amount *= 1.5f;
                    if (crit) amount *= 1.5f;
                    int dealt = target.ApplyDamage(Mathf.RoundToInt(amount));
                    bool heavy = ability.heavy || crit;

                    var contact = target.Center - actor.Facing * target.Radius * 0.7f + Vector3.back * target.Radius;
                    feedback.Impact(contact, ability.fxColor, heavy);
                    feedback.Play(ability.impactSound);
                    target.Reaction.Flash();
                    target.Reaction.Knockback(actor.Facing, heavy ? 0.4f : 0.22f);
                    hud.Popup(target.Top, crit ? $"{dealt}!" : dealt.ToString(), crit ? PopupStyle.Crit : PopupStyle.Damage);
                    AddRage(dealt * (actor.Team == Team.Heroes ? 0.45f : 0.25f));

                    if (target.IsAlive)
                        target.Animator.Play("Hit");
                    else
                        Kill(target);
                    break;
                }
                case AbilityEffect.Heal:
                {
                    int healed = target.ApplyHeal(ability.power);
                    Vfx.Instance.Rise(target.transform.position, ability.fxColor, target.Radius);
                    feedback.Play(ability.impactSound);
                    hud.Popup(target.Top, $"+{healed}", PopupStyle.Heal);
                    break;
                }
                case AbilityEffect.Shield:
                {
                    target.Shielded = true;
                    target.NotifyChanged();
                    Vfx.Instance.Rise(target.transform.position, ability.fxColor, target.Radius);
                    feedback.Play(ability.impactSound);
                    hud.Popup(target.Top, "Tarcza", PopupStyle.Buff);
                    break;
                }
                case AbilityEffect.Empower:
                {
                    target.Empowered = true;
                    target.NotifyChanged();
                    Vfx.Instance.Rise(target.transform.position, ability.fxColor, target.Radius);
                    feedback.Play(ability.impactSound);
                    hud.Popup(target.Top, "Moc!", PopupStyle.Buff);
                    break;
                }
            }
        }

        void Kill(BattleUnit unit)
        {
            unit.Animator.Play("Death", 0.04f);
            unit.SetRing(null);
            HitFeedback.Instance.Play(SfxId.Death);
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
            hud.ShowEnd(victory);
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
