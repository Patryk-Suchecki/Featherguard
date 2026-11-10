using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Featherguard.Battle.Presentation
{
    public sealed class BattleEventRouter
    {
        readonly Dictionary<Type, Func<BattleEvent, IEnumerator>> handlers = new();
        readonly MonoBehaviour host;

        public BattleEventRouter(MonoBehaviour host) => this.host = host;

        public void On<T>(Func<T, IEnumerator> handler) where T : BattleEvent => handlers[typeof(T)] = e => handler((T)e);

        public void OnInstant<T>(Action<T> handler) where T : BattleEvent => handlers[typeof(T)] = e =>
        {
            handler((T)e);
            return null;
        };

        public IEnumerator Play(IReadOnlyList<BattleEvent> events)
        {
            foreach (var battleEvent in events)
            {
                var routine = Handle(battleEvent);
                if (routine != null) yield return routine;
            }
        }

        public void PlayInstant(IEnumerable<BattleEvent> events)
        {
            foreach (var battleEvent in events)
            {
                var routine = Handle(battleEvent);
                if (routine != null) host.StartCoroutine(routine);
            }
        }

        public static IEnumerator Wait(float seconds)
        {
            if (seconds > 0f) yield return new WaitForSeconds(seconds);
        }

        IEnumerator Handle(BattleEvent battleEvent) =>
            handlers.TryGetValue(battleEvent.GetType(), out var handler) ? handler(battleEvent) : null;
    }
}
