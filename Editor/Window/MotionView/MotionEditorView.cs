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
