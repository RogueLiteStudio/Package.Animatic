using System.Collections.Generic;
using System.Linq;
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
                Open(asset);
                return true;
            }

            return false;
        }

        public static void Open(AnimaticAsset asset)
        {
            var window = GetWindow<AnimaticAssetEditorWindow>();
            window.assetSelectField.SetValueWithoutNotify(asset);
            window.SetAsset(asset);
        }
        public AnimaticAsset Asset;
        public string SelectedGUID;

        public bool EnableSimulate;
        public bool IsInstantiateObject;
        public GameObject SimulateObject;
        public AnimaticSimulate Simulate;

        //Toolbar
        private ObjectField assetSelectField;
        private ObjectField previewObjSelectField;
        private Button preViewButton;

        private ScrollView leftScrollView;
        private ScrollView rightScrollView;
        private RadioButtonList buttonList;

        private MotionEditorView currentEditorView;
        private Dictionary<string, MotionEditorView> editorViews = new Dictionary<string, MotionEditorView>();

        public Vector2 ListScrollPos;
        public Vector2 ListScrollPos2;

        public void CreateGUI()
        {
            Toolbar toolbar = new Toolbar();
            OnCreateToolBar(toolbar);
            rootVisualElement.Add(toolbar);
            var split = new TwoPaneSplitView(0, 200, TwoPaneSplitViewOrientation.Horizontal);
            //左侧列表
            var listView = new VisualElement();
            listView.style.flexDirection = FlexDirection.Column;
            listView.Add(leftScrollView = new ScrollView(ScrollViewMode.Vertical));
            leftScrollView.scrollOffset = ListScrollPos;
            leftScrollView.Add(buttonList = new RadioButtonList());
            buttonList.OnSelect = OnSelect;
            var createButton = new Button(() =>
            {
                RegistUndo("create motion");
                var menu = new GenericMenu();
                menu.AddItem(new GUIContent("State"), false, () =>
                {
                    var state = AssetUtil.CreateState(Asset, null);
                    OnSelect(state.GUID);
                    DirtyRepaint();
                });
                menu.AddItem(new GUIContent("BlendTree"), false, () =>
                {
                    var blendTree = AssetUtil.CreateBlendTree(Asset, null);
                    OnSelect(blendTree.GUID);
                    DirtyRepaint();
                });
                menu.ShowAsContext();
            });
            createButton.text = "创建";
            leftScrollView.Add(createButton);
            split.Add(listView);
            
            //右侧编辑器
            split.Add(rightScrollView = new ScrollView(ScrollViewMode.Vertical));
            rightScrollView.scrollOffset = ListScrollPos2;
            rightScrollView.style.paddingLeft = 5;
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
            ListScrollPos2 = rightScrollView.scrollOffset;
        }

        private void OnDestroy()
        {
            ClearSimulate();
            if (Simulate)
                DestroyImmediate(Simulate);
        }

        private void DirtyRepaint()
        {
            assetSelectField.SetValueWithoutNotify(Asset);
            previewObjSelectField.style.display = Asset ? DisplayStyle.Flex : DisplayStyle.None;
            previewObjSelectField.SetValueWithoutNotify(SimulateObject);
            preViewButton.style.display = SimulateObject ? DisplayStyle.None : DisplayStyle.Flex;
            RefrehButtonList();
            RefrshEditorView();
        }

        private void RefrehButtonList()
        {
            if (Asset)
            {
                buttonList.style.display = DisplayStyle.Flex;
                buttonList.Refresh(Asset.Motions, (m) => m.Name, (m) => m.GUID, SelectedGUID);
            }
            else
            {
                buttonList.style.display = DisplayStyle.None;
            }
        }

        private void OnSelect(string guid)
        {
            if (guid == SelectedGUID)
                return;
            RegistUndo("select motion");
            leftScrollView.scrollOffset = Vector2.zero;
            SelectedGUID = guid;
            RefrshEditorView();
        }

        private void RefrshEditorView()
        {
            if (currentEditorView != null && currentEditorView.viewDataKey != SelectedGUID)
            {
                currentEditorView.style.display = DisplayStyle.None;
                currentEditorView = null;
            }
            if (Asset == null)
                return;
            var motion = Asset.Motions.FirstOrDefault(it => it.GUID == SelectedGUID);
            if (motion == null)
                return;
            if (!editorViews.TryGetValue(SelectedGUID, out var editorView))
            {
                if (motion is AnimaticMotionState state)
                {
                    var view = new MotionStateEditorView();
                    view.viewDataKey = SelectedGUID;
                    view.Asset = Asset;
                    view.Simulate = Simulate;
                    editorViews.Add(SelectedGUID, view);
                    editorView = view;
                    rightScrollView.Add(view);
                }
            }
            if (editorView == null)
                return;
            editorView.UpdateView();
            currentEditorView = editorView;
            currentEditorView.style.display = DisplayStyle.Flex;
        }

        private void OnCreateToolBar(Toolbar toolbar)
        {
            assetSelectField = new ObjectField("编辑资源");
            assetSelectField.labelElement.style.minWidth = 0;
            assetSelectField.objectType = typeof(AnimaticAsset);
            assetSelectField.allowSceneObjects = false;
            if (Asset)
                assetSelectField.SetValueWithoutNotify(Asset);
            assetSelectField.RegisterValueChangedCallback((ChangeEvent<Object> evt) => SetAsset(evt.newValue as AnimaticAsset));
            toolbar.Add(assetSelectField);
            previewObjSelectField = new ObjectField("预览对象");
            previewObjSelectField.labelElement.style.minWidth = 0;
            previewObjSelectField.objectType = typeof(GameObject);
            previewObjSelectField.allowSceneObjects = true;
            previewObjSelectField.RegisterValueChangedCallback(OnBinderPrefabChange);
            toolbar.Add(previewObjSelectField);

            preViewButton = new Button();
            preViewButton.text = "在场景中预览";
            preViewButton.clicked += () =>
            {
                if (SimulateObject || !Asset)
                {
                    return;
                }
                var prefab = AnimaticAssetPrefabBinder.GetPrefab(Asset);
                SimulateObject = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                Simulate.BindTarget(Asset, SimulateObject);
                IsInstantiateObject = true;
                preViewButton.style.display = DisplayStyle.None;
                previewObjSelectField.SetValueWithoutNotify(SimulateObject);
            };
            toolbar.Add(preViewButton);
        }

        private void OnBinderPrefabChange(ChangeEvent<Object> evt)
        {
            if (evt.newValue is GameObject go)
            {
                if (PrefabUtility.IsPartOfPrefabAsset(go))
                {
                    EditorUtility.DisplayDialog("Error", "预览对象不能是Prefab", "OK");
                    previewObjSelectField.SetValueWithoutNotify(SimulateObject);
                    return;
                }
                if (go != SimulateObject)
                {
                    ClearSimulate();
                    SimulateObject = go;
                    IsInstantiateObject = true;
                    Simulate.BindTarget(Asset, SimulateObject);
                }
                preViewButton.style.display = DisplayStyle.None;
            }
            else
            {
                preViewButton.style.display = DisplayStyle.Flex;
                ClearSimulate();
            }
        }

        private void ClearSimulate()
        {
            if (SimulateObject)
            {
                if (IsInstantiateObject)
                {
                    DestroyImmediate(SimulateObject);
                    IsInstantiateObject = false;
                }
                SimulateObject = null;
            }
        }

        public void SetAsset(AnimaticAsset asset)
        {
            if (Asset)
                RegistUndo("change asset", false);
            Asset = asset;
            DirtyRepaint();
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