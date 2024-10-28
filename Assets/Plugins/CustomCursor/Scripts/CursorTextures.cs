using System.Collections.Generic;
using UnityEngine;

namespace CustomCursor
{
    [CreateAssetMenu(fileName = "CursorTextures", menuName = "Custom Cursors/CursorTextures")]
    public class CursorTextures : ScriptableObject
    {
        public List<Texture2D> textures; // List of cursor textures
    }
}