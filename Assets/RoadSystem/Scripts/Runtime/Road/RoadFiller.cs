using UnityEngine;

namespace RoadSystem.Scripts.Runtime.Road
{
    public class RoadFiller : MonoBehaviour
    {
        public RoadType type;
        public bool Initialized { get; private set; }

        [SerializeField] private Transform pivotRotate;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public void Initialize(Vector2 coordinate, float eulerAngleY, bool flip)
        {
            Initialized = true;

            var position = new Vector3(coordinate.x, 0, coordinate.y);
            transform.position = position;
            spriteRenderer.flipX = flip;

            SetRotate(eulerAngleY);

            gameObject.SetActive(true);
        }

        private void SetRotate(float eulerAngle)
        {
            var angles = pivotRotate.localEulerAngles;
            angles.y = eulerAngle;
            pivotRotate.localEulerAngles = angles;
        }

        public void SetAlpha(float alpha)
        {
            var color = spriteRenderer.color;
            color.a = alpha;
            spriteRenderer.color = color;
        }

        public void Release()
        {
            Initialized = false;
            gameObject.SetActive(false);
        }
    }
}