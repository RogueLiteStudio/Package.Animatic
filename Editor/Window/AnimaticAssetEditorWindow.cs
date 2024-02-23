using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
namespace Animatic
{
    public class AnimaticAssetEditorWindow : EditorWindow
    {
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        internal static bool OnGraphOpened(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is AnimaticAsset asset)
            {
                GetWindow<AnimaticAssetEditorWindow>().SetAsset(asset);
                return true;
            }

            return false;
        }
        private System.Action reDrawFunc;
        private ObjectField assetSelectField;
        public AnimaticAsset Asset;
        public string SelectedGUID;
        public GameObject SimulateObject;
        public AnimaticSimulate Simulate;

        public Vector2 ListScrollPos;
        public Vector2 ViewScrollPos;

        public void CreateGUI()
        {
            Toolbar toolbar = new Toolbar();
            OnCreateToolBar(toolbar);
            rootVisualElement.Add(toolbar);
            var split = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            var motionListView = new IMGUIContainer(DrawMotionList);
            split.Add(motionListView);
            var motionView = new IMGUIContainer(DrawMotionView);
            reDrawFunc = () => { motionListView.MarkDirtyRepaint(); motionView.MarkDirtyRepaint(); };
            split.Add(motionView);
            rootVisualElement.Add(split);
        }

        private void OnEnable()
        {
            Undo.undoRedoPerformed += DirtyRepaint;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= DirtyRepaint;
        }

        private void OnDestroy()
        {
            if (Simulate)
                DestroyImmediate(Simulate);
        }

        private void DirtyRepaint()
        {
            reDrawFunc?.Invoke();
        }

        private void OnCreateToolBar(Toolbar toolbar)
        {
            assetSelectField = new ObjectField();
            assetSelectField.objectType = typeof(AnimaticAsset);
            assetSelectField.allowSceneObjects = false;
            if (Asset)
                assetSelectField.SetValueWithoutNotify(Asset);
            assetSelectField.RegisterValueChangedCallback((ChangeEvent < Object > evt) => SetAsset(evt.newValue as AnimaticAsset));
            toolbar.Add(assetSelectField);
        }

        public void SetAsset(AnimaticAsset asset)
        {
            Undo.RegisterCompleteObjectUndo(this, "change asset");
            Asset = asset;
            DirtyRepaint();
        }

        private void DrawMotionList()
        {
            if (!Asset)
                return;

            using (var scroll = new GUILayout.ScrollViewScope(ListScrollPos))
            {
                ListScrollPos = scroll.scrollPosition;
                foreach (var m in Asset.Clips)
                {
                    bool isSelected = m.GUID == SelectedGUID;
                    if (GUILayout.Toggle(isSelected, m.Name, "Button") != isSelected)
                    {
                        SelectedGUID = m.GUID;
                        if (Simulate)
                            DestroyImmediate(Simulate);
                    }
                }
            }
        }

        private void DrawMotionView()
        {

        }
    }
}