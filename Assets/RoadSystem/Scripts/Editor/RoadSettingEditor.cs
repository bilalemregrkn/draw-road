#if UNITY_EDITOR
using System.Collections.Generic;
using RoadSystem.Road;
using UnityEditor;
using UnityEngine;

namespace RoadSystem.Editor
{
    [CustomEditor(typeof(RoadSetting))]
    [CanEditMultipleObjects]
    public class RoadSettingEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            GUILayout.Space(8);

            if (GUILayout.Button("Fetch All RoadPieces"))
                FetchAllRoadPieces();
        }

        private void FetchAllRoadPieces()
        {
            foreach (var t in targets)
                FetchForTarget((RoadSetting)t);
        }

        private void FetchForTarget(RoadSetting setting)
        {
            var found = new List<RoadPiece>();

            var guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/RoadSystem/Prefabs/Road" });
            foreach (var guid in guids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;

                var piece = go.GetComponent<RoadPiece>();
                if (piece != null)
                    found.Add(piece);
            }

            var so = new SerializedObject(setting);
            var listProp = so.FindProperty("listRoadPiece");
            listProp.arraySize = found.Count;
            for (int i = 0; i < found.Count; i++)
                listProp.GetArrayElementAtIndex(i).objectReferenceValue = found[i];

            so.ApplyModifiedProperties();
            EditorUtility.SetDirty(setting);

            Debug.Log($"[RoadSetting] Fetched {found.Count} RoadPiece prefabs.");
        }
    }
}
#endif
