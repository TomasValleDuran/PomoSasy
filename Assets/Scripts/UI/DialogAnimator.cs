using System.Collections;
using UnityEngine;

namespace UI
{
    /// <summary>
    /// Fade + scale-in/out for any panel (dialogs, wave banner). Uses UNSCALED time so it
    /// still animates while the game is paused (Time.timeScale == 0). Drive it via Show()/Hide(),
    /// or use the static <see cref="Set"/> helper which falls back to SetActive when no animator
    /// is attached — so existing code keeps working on panels that don't have one.
    /// </summary>
    [RequireComponent(typeof(CanvasGroup))]
    public class DialogAnimator : MonoBehaviour
    {
        [SerializeField] private float duration = 0.18f;
        [Tooltip("Scale the panel grows from when showing (and shrinks to when hiding).")]
        [SerializeField] private float startScale = 0.92f;

        private CanvasGroup _canvasGroup;
        private Coroutine _routine;

        private void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
        }

        public void Show()
        {
            gameObject.SetActive(true);
            Play(true);
        }

        public void Hide()
        {
            if (!gameObject.activeSelf)
                return;

            Play(false);
        }

        private void Play(bool show)
        {
            if (_routine != null)
                StopCoroutine(_routine);

            _routine = StartCoroutine(Animate(show));
        }

        private IEnumerator Animate(bool show)
        {
            float fromAlpha = show ? 0f : 1f;
            float toAlpha = show ? 1f : 0f;
            float fromScale = show ? startScale : 1f;
            float toScale = show ? 1f : startScale;

            _canvasGroup.alpha = fromAlpha;
            transform.localScale = Vector3.one * fromScale;

            float t = 0f;
            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / duration);
                // Ease-out when showing, ease-in when hiding.
                float e = show ? 1f - (1f - k) * (1f - k) : k * k;
                _canvasGroup.alpha = Mathf.Lerp(fromAlpha, toAlpha, e);
                transform.localScale = Vector3.one * Mathf.Lerp(fromScale, toScale, e);
                yield return null;
            }

            _canvasGroup.alpha = toAlpha;
            transform.localScale = Vector3.one * toScale;

            if (!show)
                gameObject.SetActive(false);

            _routine = null;
        }

        /// <summary>
        /// Show/hide a panel: animates if it has a <see cref="DialogAnimator"/>, otherwise plain SetActive.
        /// </summary>
        public static void Set(GameObject panel, bool visible)
        {
            if (panel == null)
                return;

            if (panel.TryGetComponent(out DialogAnimator animator))
            {
                if (visible) animator.Show();
                else animator.Hide();
            }
            else
            {
                panel.SetActive(visible);
            }
        }
    }
}
