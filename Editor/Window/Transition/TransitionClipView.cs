using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class TransitionClipView : ClipElement
    {
        private readonly Label nameLabel = new Label();
        public TransitionClipView() 
        {
            Add(nameLabel);
            nameLabel.style.unityTextAlign = TextAnchor.MiddleCenter;
            nameLabel.StretchToParentSize();
        }

        public void UpdateByMotion(AnimaticMotion motion, float starTime)
        {
            if (motion == null)
            {
                style.display = DisplayStyle.None;
                return;
            }
            style.display = DisplayStyle.Flex;
            nameLabel.text = motion.Name;
            style.width = Mathf.RoundToInt(motion.GetLength() * AnimaticViewStyle.FrameRate) * frameWidth;
            style.left = Mathf.RoundToInt(starTime * AnimaticViewStyle.FrameRate) * frameWidth;
        }
    }
}
