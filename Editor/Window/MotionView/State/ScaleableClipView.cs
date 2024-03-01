using System.Collections.Generic;
using UnityEngine.UIElements;
using UnityEngine;

namespace Animatic
{
    public class ScaleableClipView : ClipElement
    {
        private readonly List<Label> clips = new List<Label>();
        private int selectIndex = -1;

        public System.Action<int> OnClipClick;

        public ScaleableClipView()
        {
            style.flexDirection = FlexDirection.Row;
        }

        public void UpdateClips(AnimaticMotionState animaticClip)
        {
            if (animaticClip.Clips == null)
                return;   
            for (int i=0; i< animaticClip.Clips.Length; ++i)
            {
                var clip = animaticClip.Clips[i];
                var view = GetClip(i);
                view.text = $"{clip.StartFrame} -> {clip.StartFrame + clip.FrameCount} | {clip.Speed}";
                float speed = clip.Speed;
                if (speed == 0)
                    speed = 1;
                view.style.width = frameWidth * clip.FrameCount / speed;
            }
            if (clips.Count > animaticClip.Clips.Length)
            {
                for (int i= animaticClip.Clips.Length; i<clips.Count; ++i)
                {
                    clips[i].style.width = 0;
                }
            }
        }

        protected override void OnFrameWidthChange(float scale)
        {
            for (int i = 0; i < clips.Count; ++i)
            {
                var lable = clips[i];
                float x = lable.style.left.value.value;
                lable.style.left = x * scale;
                lable.style.width = lable.style.width.value.value * scale;
            }
        }

        private Label GetClip(int index)
        {
            if (index >= clips.Count)
            {
                Label label = new Label();
                clips.Add(label);

                label.style.top = 0;
                label.style.bottom = 0;
                label.SetBorderColor(Color.white);
                label.RegisterCallback<ClickEvent>(OnClick);
                Add(label);
            }
            return clips[index];
        }

        private void OnClick(ClickEvent evt)
        {
            if (evt.currentTarget is Label label)
            {
                for (int i=0; i<clips.Count; ++i)
                {
                    if (clips[i] == label)
                    {
                        if (selectIndex != i)
                        {
                            selectIndex = i;
                            UpdateSelect();
                            OnClipClick?.Invoke(i);
                        }
                    }
                }
            }
        }

        private void UpdateSelect()
        {
            for (int i=0; i<clips.Count; ++i)
            {
                SetSelcted(clips[i], selectIndex == i);
            }
        }

        private void SetSelcted(Label label, bool selected)
        {
            label.SetBorderWidth(selected ? 1f : 0f);
        }
    }
}
