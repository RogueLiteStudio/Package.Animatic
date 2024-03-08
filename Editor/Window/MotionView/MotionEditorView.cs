using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Animatic
{
    public class MotionEditorView : VisualElement
    {
        public AnimaticAsset Asset;
        public AnimaticSimulate Simulate;

        public void UpdateView()
        {
            var motion = Asset.Motions.FirstOrDefault(it=>it.GUID == viewDataKey);
            OnUpdateView(motion);
        }

        public void SetActive(bool active)
        {
            var newStyle = active ? DisplayStyle.Flex : DisplayStyle.None;
            if (style.display != newStyle)
            {
                style.display = newStyle;
                if (!active)
                    OnDeActive();
                else
                    OnActive();
            }
        }

        protected virtual void OnDeActive(){ }

        protected virtual void OnActive() { }


        protected void RegistUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(Asset, name);
            EditorUtility.SetDirty(Asset);
        }

        protected virtual void OnUpdateView(AnimaticMotion motion)
        {

        }
    }
}
