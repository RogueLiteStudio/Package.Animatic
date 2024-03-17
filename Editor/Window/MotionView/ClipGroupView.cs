using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class ClipGroupView : VisualElement
    {
        private float scale = 1;
        private int frameLength = 30;
        private float frameRate = 30;
        private float frameWidth= AnimaticViewStyle.FrameWidth;
        private int currentSelectFrame = 0;

        private readonly TimebarView timebarView = new TimebarView();
        private readonly VisualElement container = new VisualElement();
        private readonly VisualElement lineView = new VisualElement();

        public event System.Action<int> OnFrameLocation
        {
            add
            {
                timebarView.OnFrameLocation += value;
            }
            remove
            {
                timebarView.OnFrameLocation -= value;
            }
        }

        public int SelectFrame => currentSelectFrame;

        public ClipGroupView()
        {
            style.flexDirection = FlexDirection.Column;

            timebarView.style.left = 0;
            timebarView.style.right = 0;
            timebarView.style.height = AnimaticViewStyle.TimeBarHeight;
            timebarView.OnFrameLocation = SetFrameLocation;
            Add(timebarView);

            container.style.left = 0;
            container.style.right = 0;
            container.style.flexDirection = FlexDirection.Column;
            Add(container);

            lineView.style.position = Position.Absolute;
            lineView.style.top = 0;
            lineView.style.bottom = 0;
            lineView.style.width = 2;
            lineView.style.borderLeftWidth = 2;
            lineView.style.borderLeftColor = Color.green;
            lineView.pickingMode = PickingMode.Ignore;
            Add(lineView);

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
            currentSelectFrame = frame;
            lineView.style.left = frame * frameWidth;
        }

        public void AddClipElement(ClipElement element)
        {
            element.style.marginTop = 5;
            container.Add(element);
        }

        private void UpdateView()
        {
            int length = Mathf.Max(30, frameLength);
            frameWidth = (30 / frameRate) * AnimaticViewStyle.FrameWidth * scale;
            float minWidth = length * AnimaticViewStyle.FrameWidth * (30 / frameRate) * scale;
            style.minWidth = minWidth + 20;
            
            timebarView.SetFramInfo(frameRate, frameWidth);
            lineView.style.left = currentSelectFrame * frameWidth;
            foreach (var child in container.Children())
            {
                if (child is ClipElement element)
                {
                    element.SetFrameWidth(frameWidth);
                }
            }
            MarkDirtyRepaint();
        }
    }
}
