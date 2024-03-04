using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class ClipGroupView : VisualElement
    {
        private float scale = 1;
        private int frameLength = 30;
        private float frameRate = 30;
        private readonly TimebarView timebarView = new TimebarView();
        private readonly VisualElement container = new VisualElement();
        private readonly FrameLocationView frameLocation = new FrameLocationView();

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
            style.flexDirection = FlexDirection.Column;

            timebarView.style.left = 0;
            timebarView.style.right = 0;
            timebarView.style.height = AnimaticViewStyle.TimeBarHeight;
            Add(timebarView);

            container.style.left = 0;
            container.style.right = 0;
            container.style.flexDirection = FlexDirection.Column;
            Add(container);

            frameLocation.StretchToParentSize();
            Add(frameLocation);

            style.paddingBottom = 10;
            this.SetBorderWidth(2);
            this.SetBorderColor(Color.gray);
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
            style.minWidth = minWidth + 20;
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
