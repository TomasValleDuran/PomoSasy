using UnityEngine;
using UnityEngine.EventSystems;

namespace Input
{
    public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] private RectTransform background;
        [SerializeField] private RectTransform handle;

        public Vector2 InputVector { get; private set; }

        private float _radius;
        private Canvas _canvas;

        private void Start()
        {
            _radius = background.sizeDelta.x / 2f;
            _canvas = GetComponentInParent<Canvas>();

            background.gameObject.SetActive(false);
            handle.gameObject.SetActive(false);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            // Move joystick to where the user touched
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                _canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out position
            );

            background.anchoredPosition = position;
            handle.anchoredPosition = Vector2.zero;
            background.gameObject.SetActive(true); // show it
            handle.gameObject.SetActive(true);

            OnDrag(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                background,
                eventData.position,
                eventData.pressEventCamera,
                out position
            );

            position = Vector2.ClampMagnitude(position, _radius);
            handle.anchoredPosition = position;

            InputVector = position / _radius;
            InputVector = InputVector.normalized;
            if (InputVector.magnitude < 0.1f) InputVector = Vector2.zero;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            InputVector = Vector2.zero;
            handle.anchoredPosition = Vector2.zero;
            background.gameObject.SetActive(false);
            handle.gameObject.SetActive(false);
        }
    }
}