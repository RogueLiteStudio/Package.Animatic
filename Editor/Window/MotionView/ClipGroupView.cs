using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class ClipGroupView : VisualElement
    {
        private float scale = 1;
        private int frameLength = 30;
        private float frameRate = 30;
        private TimebarView timebarView;
        private VisualElement container;
        private FrameLocationView frameLocation;

        public event System.Action<int> OnFrameLocation
        {
            add
            {
                frameLocation.OnFrameLocation += value;
            }
            remove
            {
                frameLocation.OnFrameLocation -= value;
            }
        }

        public ClipGroupView()
        {
            timebarView = new TimebarView();
            timebarView.style.left = 0f;
            timebarView.style.right = 0f;
            timebarView.style.top = 0f;
            timebarView.style.height = AnimaticViewStyle.TimeBarHeight;
            Add(timebarView);
            container = new VisualElement();
            container.StretchToParentSize();
            container.style.top = AnimaticViewStyle.TimeBarHeight;
            container.style.flexDirection = FlexDirection.Column;
            Add(container);

            frameLocation = new FrameLocationView();
            frameLocation.StretchToParentSize();
            Add(frameLocation);
        }

        public void SetFrameInfo(int frameLength, float frameRate)
        {
            this.frameLength = frameLength;
            this.frameRate = frameRate;
            UpdateView();
        }

        public void SetFrameLocation(int frame)
        {
            frameLocation.SetFrameLocation(frame);
        }

        public void AddClipElement(ClipElement element)
        {
            container.Add(element);
        }

        private void UpdateView()
        {
            int length = Mathf.Max(30, frameLength);
            float frameWidth = (30 / frameRate) * AnimaticViewStyle.FrameWidth * scale;
            float minWidth = length * AnimaticViewStyle.FrameWidth * (30 / frameRate) * scale;
            style.minWidth = minWidth;
            timebarView.SetFramInfo(frameRate, frameWidth);
            frameLocation.SetFrameWidth(frameWidth);
            foreach (var child in container.Children())
            {
                if (child is ClipElement element)
                {
                    element.SetFrameWidth(frameWidth);
                }
            }
        }
    }
}
