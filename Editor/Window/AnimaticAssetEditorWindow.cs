using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using VisualElementExtern;
namespace Animatic
{
    public class AnimaticAssetEditorWindow : EditorWindow
    {
        [UnityEditor.Callbacks.OnOpenAsset(0)]
        internal static bool OnGraphOpened(int instanceID, int line)
        {
            if (EditorUtility.InstanceIDToObject(instanceID) is AnimaticAsset asset)
            {
                var window = GetWindow<AnimaticAssetEditorWindow>();
                window.assetSelectField.SetValueWithoutNotify(asset);
                window.SetAsset(asset);
                return true;
            }

            return false;
        }
        private ObjectField assetSelectField;
        public AnimaticAsset Asset;
        public string SelectedGUID;
        public GameObject SimulateObject;
        public AnimaticSimulate Simulate;
        private ScrollView leftScrollView;
        private RadioButtonList buttonList;

        public Vector2 ListScrollPos;

        public void CreateGUI()
        {
            Toolbar toolbar = new Toolbar();
            OnCreateToolBar(toolbar);
            rootVisualElement.Add(toolbar);
            var split = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            split.Add(leftScrollView = new ScrollView(ScrollViewMode.Vertical));
            leftScrollView.scrollOffset = ListScrollPos;
            leftScrollView.Add(buttonList = new RadioButtonList());
            buttonList.OnSelect = OnSelect;
            var motionView = new IMGUIContainer(DrawMotionView);
            split.Add(motionView);
            rootVisualElement.Add(split);

            DirtyRepaint();
        }

        private void Awake()
        {
            Simulate = AnimaticSimulate.Create();
        }

        private void OnEnable()
        { 
            Undo.undoRedoPerformed += DirtyRepaint;
        }

        private void OnDisable()
        {
            Undo.undoRedoPerformed -= DirtyRepaint;
            ListScrollPos = leftScrollView.scrollOffset;
        }

        private void OnDestroy()
        {
            if (Simulate)
                DestroyImmediate(Simulate);
        }

        private void DirtyRepaint()
        {
            RefrehButtonList();
        }

        private void RefrehButtonList()
        {
            if (Asset)
            {
                buttonList.Refresh(Asset.Motions, (m) => m.Name, (m) => m.GUID, SelectedGUID);
            }
        }

        private void OnSelect(string guid)
        {
            if (guid == SelectedGUID)
                return;
            SelectedGUID = guid;
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
            RegistUndo("change asset", false);
            Asset = asset;
            DirtyRepaint();
        }

        private void DrawMotionView()
        {

        }

        public void RegistUndo(string name, bool withAsset = true)
        {
            if (withAsset && Asset)
            {
                Undo.RegisterCompleteObjectUndo(Asset, name);
                EditorUtility.SetDirty(Asset);
            }
            Undo.RegisterCompleteObjectUndo(this, name);
        }
    }
}