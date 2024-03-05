using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class MotionStateClipView : ClipElement
    {
        private readonly Label label = new Label();
        private readonly DragableClipView selectClip = new DragableClipView();
        public System.Action<ClipDragType, int> OnDragClipFrameOffset;

        public MotionStateClipView()
        {
            label.style.unityTextAlign = TextAnchor.MiddleCenter;
            label.StretchToParentSize();
            Add(label);

            selectClip.OnClipDragOffset = OnClipDragOffset;
            selectClip.visible = false;
            selectClip.style.backgroundColor = new Color(135/255f, 206/255f, 235/255f, 1);
            selectClip.StretchToParentSize();
            Add(selectClip);
        }

        public void UpdateSelectClipIndex(AnimaticMotionState clip, int index)
        {
            if (clip.Animation)
            {

                label.text = clip.Animation.name;
                style.width = frameWidth * (clip.Animation.length * clip.Animation.frameRate);
            }
            else
            {
                label.text = string.Empty;
            }
            bool active = false;
            if (clip.Clips != null)
            {
                for (int i=0; i<clip.Clips.Length; ++i)
                {
                    if (i == index)
                    {
                        active = true;
                        var v = clip.Clips[i];
                        var clipStyle = selectClip.style;
                        clipStyle.left = frameWidth * v.StartFrame;
                        clipStyle.width = frameWidth * v.FrameCount;
                    }
                }
            }
            selectClip.visible = active;
        }

        private void OnClipDragOffset(ClipDragType type, float offset)
        {
            int frameOffset = (int)(offset / frameWidth);
            if (frameOffset != 0)
            {
                selectClip.OnDragApply(frameWidth);
                OnDragClipFrameOffset?.Invoke(type, frameOffset);
            }
        }

        protected override void OnFrameWidthChange(float scale)
        {
            if (selectClip.visible)
            {
                var s = selectClip.style;
                s.left = s.left.value.value * scale;
                s.width = s.width.value.value * scale;
            }
        }
    }
}
