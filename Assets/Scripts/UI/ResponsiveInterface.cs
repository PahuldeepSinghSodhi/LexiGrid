using UnityEngine;
using UnityEngine.UI;

namespace LexiGrid.UI
{
    /// <summary>Reflows the playable scene for desktop and tall windows without stretching tiles.</summary>
    [ExecuteAlways]
    public class ResponsiveInterface : MonoBehaviour
    {
        public CanvasScaler Scaler;
        public RectTransform Board;
        public RectTransform Sidebar;
        public RectTransform MenuPreview;
        public RectTransform MenuCopy;
        public RectTransform Path;
        public RectTransform Controls;
        private Vector2 _lastSize;

        private void LateUpdate()
        {
            RectTransform canvas = transform as RectTransform;
            Vector2 size = canvas.rect.size;
            if (size == _lastSize) return;
            _lastSize = size;
            bool tall = size.x / size.y < 1.1f;
            Scaler.referenceResolution = tall ? new Vector2(900, 1200) : new Vector2(1440, 900);
            Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.Expand;
            if (Board != null)
            {
                Place(Board, tall ? new Vector2(0, 80) : new Vector2(-230, -25), new Vector2(580, 580));
                Place(Sidebar, tall ? new Vector2(0, -385) : new Vector2(360, -25),
                    tall ? new Vector2(720, 275) : new Vector2(390, 580));
                Place(Path, tall ? new Vector2(0, -240) : new Vector2(0, -360), new Vector2(850, 60));
                if (Controls != null) Place(Controls, tall ? new Vector2(0, -553) : new Vector2(0, -418), new Vector2(850, 48));
                foreach (var text in Sidebar.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true))
                {
                    var rect = text.rectTransform;
                    bool history = text.name.StartsWith("Text ") && rect.sizeDelta.y == 200;
                    if (history) { text.gameObject.SetActive(!tall); continue; }
                    float x = text.fontSize > 50 ? .75f : text.fontSize > 30 ? .25f : .5f;
                    float y = text.fontSize > 30 ? .64f : .22f;
                    rect.anchorMin = rect.anchorMax = tall ? new Vector2(x, y) :
                        new Vector2(.5f, text.fontSize > 50 ? .70f : text.fontSize > 30 ? .86f : .55f);
                }
            }
            if (MenuPreview != null)
            {
                MenuPreview.gameObject.SetActive(!tall);
                Place(MenuCopy, tall ? Vector2.zero : new Vector2(330, 0), new Vector2(560, 640));
            }
        }

        private static void Place(RectTransform rect, Vector2 position, Vector2 size)
        {
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;
        }
    }
}
