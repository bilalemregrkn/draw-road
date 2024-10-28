using UnityEngine;
using UnityEngine.EventSystems;

namespace RoadSystem.Draw
{
    public class DrawTool : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Camera _mainCamera;

        private void Start()
        {
            _mainCamera = Camera.main;
        }

        private Vector3 FireRay()
        {
            var screenPoint = Input.mousePosition;
            var ray = _mainCamera.ScreenPointToRay(screenPoint);

            if (!Physics.Raycast(ray, out RaycastHit hit))
                return Vector3.zero;

            var hitPoint = hit.point;
            return hitPoint;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
        }

        public void OnDrag(PointerEventData eventData)
        {
            var position = FireRay();
            EventBus.EventBus.Instance.Publish(new OnDragSignal(position));
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            EventBus.EventBus.Instance.Publish(new OnFingerUpSignal());
        }
    }

    public struct OnFingerUpSignal
    {
    }

    public struct OnDragSignal
    {
        public Vector3 Position;

        public OnDragSignal(Vector3 position)
        {
            Position = position;
        }
    }
}