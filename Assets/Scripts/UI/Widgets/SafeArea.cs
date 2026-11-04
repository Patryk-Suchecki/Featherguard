using UnityEngine;

namespace Featherguard.UI
{
    public class SafeArea : MonoBehaviour
    {
        Rect applied;
        Vector2Int screen;

        void Update()
        {
            var safe = Screen.safeArea;
            if (safe == applied && screen.x == Screen.width && screen.y == Screen.height) return;
            applied = safe;
            screen = new Vector2Int(Screen.width, Screen.height);
            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(safe.xMin / Screen.width, safe.yMin / Screen.height);
            rt.anchorMax = new Vector2(safe.xMax / Screen.width, safe.yMax / Screen.height);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        public static RectTransform Create(Transform parent)
        {
            var rt = Ui.Stretch("Safe", parent);
            rt.gameObject.AddComponent<SafeArea>().Update();
            return rt;
        }
    }
}
