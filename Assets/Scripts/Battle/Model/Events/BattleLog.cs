using System.Collections.Generic;

namespace Featherguard.Battle
{
    public sealed class BattleLog
    {
        readonly Stack<List<BattleEvent>> scopes = new();
        List<BattleEvent> root = new();

        public void Emit(BattleEvent battleEvent) => (scopes.Count > 0 ? scopes.Peek() : root).Add(battleEvent);

        public void Push(List<BattleEvent> scope) => scopes.Push(scope);

        public void Pop() => scopes.Pop();

        public IReadOnlyList<BattleEvent> Flush()
        {
            var events = root;
            root = new List<BattleEvent>();
            return events;
        }
    }
}
