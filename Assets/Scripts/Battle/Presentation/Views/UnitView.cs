using System;
using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public class UnitView : MonoBehaviour
    {
        public BattleUnit Unit { get; private set; }
        public UnitDefinition Definition => Unit.Definition;
        public UnitVisuals Visuals { get; private set; }
        public Team Team => Unit.Team;
        public UnitSnapshot State { get; private set; }
        public bool IsAlive => State.Alive;
        public int Health => State.Health;
        public int MaxHealth => State.MaxHealth;
        public float HealthFraction => State.HealthFraction;
        public IReadOnlyList<StatusSnapshot> Statuses => State.Statuses;
        public UnitAnimator Animator { get; private set; }
        public HitReaction Reaction { get; private set; }
        public SelectionRing Ring { get; private set; }
        public Vector3 HomePosition { get; set; }
        public float Height => Visuals.ScaledHeight;
        public float Radius => Visuals.Radius;
        public Vector3 Center => transform.position + Vector3.up * Height * 0.5f;
        public Vector3 Top => transform.position + Vector3.up * (Height + 0.1f);
        public Vector3 Facing => Team == Team.Heroes ? Vector3.right : Vector3.left;

        public event Action Changed;

        internal void Bind(BattleUnit unit, UnitVisuals visuals, UnitSnapshot state, UnitAnimator animator, HitReaction reaction, SelectionRing ring)
        {
            Unit = unit;
            Visuals = visuals;
            State = state;
            Animator = animator;
            Reaction = reaction;
            Ring = ring;
        }

        public void Show(UnitSnapshot state)
        {
            State = state;
            Changed?.Invoke();
        }
    }
}
