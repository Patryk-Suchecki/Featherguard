using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Featherguard.Battle
{
    public interface IUnitPicker
    {
        bool TryPick(out BattleUnit unit);
    }

    public sealed class PointerUnitPicker : IUnitPicker
    {
        readonly Camera camera;
        readonly System.Func<IEnumerable<BattleUnit>> candidates;
        readonly float tolerance;
        readonly List<RaycastResult> uiHits = new();

        public PointerUnitPicker(Camera camera, System.Func<IEnumerable<BattleUnit>> candidates, float tolerance)
        {
            this.camera = camera;
            this.candidates = candidates;
            this.tolerance = tolerance;
        }

        public bool TryPick(out BattleUnit unit)
        {
            unit = null;
            var pointer = Pointer.current;
            if (pointer == null || !pointer.press.wasPressedThisFrame) return false;
            var position = pointer.position.ReadValue();
            if (OverUi(position)) return false;
            unit = Pick(position);
            return unit != null;
        }

        bool OverUi(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            return uiHits.Count > 0;
        }

        BattleUnit Pick(Vector2 position)
        {
            if (Physics.Raycast(camera.ScreenPointToRay(position), out var hit, 100f))
            {
                var direct = hit.collider.GetComponentInParent<BattleUnit>();
                if (direct != null && direct.IsAlive) return direct;
            }

            BattleUnit best = null;
            float bestDistance = Screen.height * tolerance;
            foreach (var unit in candidates().Where(u => u.IsAlive))
            {
                float distance = DistanceToSegment(position, camera.WorldToScreenPoint(unit.transform.position), camera.WorldToScreenPoint(unit.Top));
                if (distance < bestDistance)
                {
                    bestDistance = distance;
                    best = unit;
                }
            }
            return best;
        }

        static float DistanceToSegment(Vector2 point, Vector2 a, Vector2 b)
        {
            var ab = b - a;
            float t = ab.sqrMagnitude > 0f ? Mathf.Clamp01(Vector2.Dot(point - a, ab) / ab.sqrMagnitude) : 0f;
            return Vector2.Distance(point, a + ab * t);
        }
    }
}
