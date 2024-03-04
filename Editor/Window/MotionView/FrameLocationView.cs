using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class FrameLocationView : IMGUIContainer
    {
        private readonly VisualElement barView = new VisualElement();
        private readonly VisualElement lineView = new VisualElement();
        private float frameWidth = AnimaticViewStyle.FrameWidth;
        private int currentSelectFrame = 0;
        public System.Action<int> OnFrameLocation;
        public FrameLocationView()
        {
            barView.StretchToParentSize();
            barView.style.height = AnimaticViewStyle.TimeBarHeight;
            barView.AddManipulator(new TimeBarLocationManipulator(OnTimeBarLocation));
            Add(barView);
            
            lineView.style.position = Position.Absolute;
            lineView.style.top = 0;
            lineView.style.bottom = 0;
            lineView.style.width = 2;
            lineView.style.borderLeftWidth = 2;
            lineView.style.borderLeftColor = Color.green;
            lineView.pickingMode = PickingMode.Ignore;
            Add(lineView);
        }

        public void SetFrameWidth(float frameWidth)
        {
            this.frameWidth = frameWidth;
        }

        public void SetFrameLocation(int frame)
        {
            if (currentSelectFrame != frame)
            {
                currentSelectFrame = frame;
                lineView.style.left = frame * frameWidth;
            }
        }

        private void OnTimeBarLocation(Vector2 pos)
        {
            int frame = Mathf.FloorToInt(pos.x / frameWidth);
            if (currentSelectFrame != frame)
            {
                currentSelectFrame = frame;
                lineView.style.left = frame * frameWidth;
                OnFrameLocation?.Invoke(frame);
            }
        }
    }
}
