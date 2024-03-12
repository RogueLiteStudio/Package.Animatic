using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class BlendClipView : ClipElement
    {
        private readonly Label label = new Label();

        public BlendClipView()
        {
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.StretchToParentSize();
            Add(label);
        }

        public void UpdateClip(AnimationClip clip)
        {
            if (!clip)
            {
                style.display = DisplayStyle.None;
                return;
            }
            style.display = DisplayStyle.Flex;
            label.text = clip.name;
            style.width = frameWidth * (clip.length * clip.frameRate);
        }
    }
}
