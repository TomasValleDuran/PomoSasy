using UnityEngine;

namespace UI
{
    /// <summary>
    /// Resizes a RectTransform to fit the device's safe area, keeping HUD and buttons clear of
    /// notches, rounded corners and the home indicator. Put this on a full-screen panel that wraps
    /// your HUD; anchor children to it as usual. Re-applies if the safe area changes (rotation).
    /// </summary>
    [RequireComponent(typeof(RectTransform))]
    public class SafeAreaFitter : MonoBehaviour
    {
        [Tooltip("Ignore the horizontal insets (left/right). Useful for full-bleed backgrounds.")]
        [SerializeField] private bool ignoreX;
        [Tooltip("Ignore the vertical insets (top/bottom).")]
        [SerializeField] private bool ignoreY;

        private RectTransform _rect;
        private Rect _lastSafeArea;
        private Vector2Int _lastScreen;

        private void Awake()
        {
            _rect = GetComponent<RectTransform>();
            Apply();
        }

        private void Update()
        {
            // Cheap guard: only recompute when the safe area or resolution actually changes.
            if (Screen.safeArea != _lastSafeArea ||
                Screen.width != _lastScreen.x ||
                Screen.height != _lastScreen.y)
            {
                Apply();
            }
        }

        private void Apply()
        {
            _lastSafeArea = Screen.safeArea;
            _lastScreen = new Vector2Int(Screen.width, Screen.height);

            if (Screen.width <= 0 || Screen.height <= 0)
                return;

            Rect safe = Screen.safeArea;
            Vector2 min = safe.position;
            Vector2 max = safe.position + safe.size;

            min.x /= Screen.width;
            min.y /= Screen.height;
            max.x /= Screen.width;
            max.y /= Screen.height;

            if (ignoreX) { min.x = 0f; max.x = 1f; }
            if (ignoreY) { min.y = 0f; max.y = 1f; }

            _rect.anchorMin = min;
            _rect.anchorMax = max;
            _rect.offsetMin = Vector2.zero;
            _rect.offsetMax = Vector2.zero;
        }
    }
}
