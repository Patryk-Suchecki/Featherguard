using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Featherguard.Battle.Presentation
{
    public sealed class PointerUnitPicker : IUnitPicker
    {
        const float DragThreshold = 0.035f;

        readonly Camera camera;
        readonly float tolerance;
        readonly List<RaycastResult> uiHits = new();
        bool tracking;
        bool moved;
        Vector2 pressedAt;

        public PointerUnitPicker(Camera camera, float tolerance)
        {
            this.camera = camera;
            this.tolerance = tolerance;
        }

        public UnitPointer Read(IEnumerable<UnitView> candidates)
        {
            var pointer = Pointer.current;
            if (pointer == null) return default;
            var position = pointer.position.ReadValue();

            if (pointer.press.wasPressedThisFrame)
            {
                if (OverUi(position)) return default;
                tracking = true;
                moved = false;
                pressedAt = position;
                return new UnitPointer(true, true, false, false, Pick(position, candidates), position);
            }
            if (!tracking) return default;

            moved |= Vector2.Distance(position, pressedAt) > Screen.height * DragThreshold;
            if (pointer.press.wasReleasedThisFrame || !pointer.press.isPressed)
            {
                tracking = false;
                return new UnitPointer(false, false, true, moved, Pick(position, candidates), position);
            }
            return new UnitPointer(false, true, false, moved, Pick(position, candidates), position);
        }

        bool OverUi(Vector2 position)
        {
            if (EventSystem.current == null) return false;
            uiHits.Clear();
            EventSystem.current.RaycastAll(new PointerEventData(EventSystem.current) { position = position }, uiHits);
            return uiHits.Count > 0;
        }

        UnitView Pick(Vector2 position, IEnumerable<UnitView> candidates)
        {
            if (Physics.Raycast(camera.ScreenPointToRay(position), out var hit, 100f))
            {
                var direct = hit.collider.GetComponentInParent<UnitView>();
                if (direct != null && direct.IsAlive) return direct;
            }

            UnitView best = null;
            float bestDistance = Screen.height * tolerance;
            foreach (var unit in candidates.Where(u => u.IsAlive))
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
