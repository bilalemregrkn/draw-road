using UnityEngine;

namespace CustomCursor
{
    public class CustomMouseCursor : MonoBehaviour
    {
        public CursorTextures cursorTextures; // Assign your cursor textures ScriptableObject in the inspector
        public Vector2 cursorHotspot = Vector2.zero; // Adjust the hotspot of the cursor if necessary
        public float cursorScale = 1.0f; // Adjust the scale of the cursor

        public float targetScaleFactor = 1.1f; // Adjust the scale of the cursor
        private float _cursorScaleFactor = 1.0f; // Adjust the scale of the cursor

        private int _currentTextureIndex;

        void Start()
        {
            // Hide the default cursor
            Cursor.visible = false;
        }

        void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _cursorScaleFactor = targetScaleFactor;
            }
            else
            {
                _cursorScaleFactor = 1;
            }
        }

        public void NextCursor()
        {
            _currentTextureIndex = (_currentTextureIndex + 1) % cursorTextures.textures.Count;
        }

        void OnGUI()
        {
            if (cursorTextures.textures.Count == 0)
                return;

            // Get the current cursor texture from the array
            Texture2D cursorTexture = cursorTextures.textures[_currentTextureIndex];

            // Calculate scaled width and height of the cursor
            float scaledWidth = cursorTexture.width * cursorScale * _cursorScaleFactor;
            float scaledHeight = cursorTexture.height * cursorScale * _cursorScaleFactor;

            // Draw the custom cursor at the current mouse position with scaling
            GUI.DrawTexture(
                new Rect(Input.mousePosition.x - cursorHotspot.x,
                    Screen.height - Input.mousePosition.y - cursorHotspot.y, scaledWidth, scaledHeight), cursorTexture);
        }
    }
}