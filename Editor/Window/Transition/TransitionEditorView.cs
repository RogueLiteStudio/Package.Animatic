using UnityEngine.UIElements;

namespace Animatic
{
    public class TransitionEditorView : VisualElement
    {
        public AnimaticAsset Asset;

        private readonly ClipGroupView groupView = new ClipGroupView();
        private readonly PlayButtonList playButtonList = new PlayButtonList();
        public void UpdateView(AnimaticMotion motion)
        {

        }
    }
}
