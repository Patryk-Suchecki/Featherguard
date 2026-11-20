using System;
using System.Collections;
using UnityEngine;

namespace Featherguard.Game
{
    public class CameraFlight : MonoBehaviour
    {
        Transform view;
        float duration;
        Coroutine flight;

        public bool Flying => flight != null;

        public void Init(Transform view, float duration)
        {
            this.view = view;
            this.duration = duration;
        }

        public void Jump(Pose target) => view.SetPositionAndRotation(target.position, target.rotation);

        public void Fly(Pose target, Action arrived)
        {
            if (flight != null) StopCoroutine(flight);
            flight = StartCoroutine(Flight(target, arrived));
        }

        IEnumerator Flight(Pose target, Action arrived)
        {
            var start = new Pose(view.position, view.rotation);
            for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / duration)
            {
                float k = Mathf.SmoothStep(0f, 1f, t);
                view.SetPositionAndRotation(Vector3.Lerp(start.position, target.position, k), Quaternion.Slerp(start.rotation, target.rotation, k));
                yield return null;
            }
            Jump(target);
            flight = null;
            arrived();
        }
    }
}
