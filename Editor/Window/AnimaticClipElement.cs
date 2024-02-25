using UnityEngine.UIElements;

namespace Animatic
{
    public class AnimaticClipElement : VisualElement
    {
        protected float frameWidth = AnimaticViewStyle.FrameWidth;
        public AnimaticClipElement()
        {
            style.position = Position.Absolute;
            style.height = AnimaticViewStyle.ClipHeight;
            style.backgroundColor = AnimaticViewStyle.ClipColor;
        }

        public void SetFrameWidth(float width)
        {
            if (frameWidth != width)
            {
                float scale = width / frameWidth;
                style.width = style.width.value.value * scale;
                style.left = style.left.value.value * scale;
                frameWidth = width;
                OnFrameWidthChange(scale);
            }
        }

        protected virtual void OnFrameWidthChange(float scale)
        {
        }
    }
}
