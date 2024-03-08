using UnityEditor;
using UnityEngine;

namespace Animatic
{
    [CustomEditor(typeof(AnimaticAsset))]
    public class AnimaticAssetEditor :Editor
    {
        private GameObject prefab;
        private void OnEnable()
        {
            prefab = AnimaticAssetPrefabBinder.GetPrefab((AnimaticAsset)target);
        }
        public override void OnInspectorGUI()
        {
            if (GUILayout.Button("编辑"))
            {
                AnimaticAssetEditorWindow.Open((AnimaticAsset)target);
            }
            EditorGUI.BeginChangeCheck();
            prefab = EditorGUILayout.ObjectField("预览Prefab", prefab, typeof(GameObject), false) as GameObject;
            if (EditorGUI.EndChangeCheck())
            {
                AnimaticAssetPrefabBinder.Bind((AnimaticAsset)target, prefab);
            }
            using (new EditorGUI.DisabledScope(true))
            {
                DrawDefaultInspector();
            }
        }
    }
}
