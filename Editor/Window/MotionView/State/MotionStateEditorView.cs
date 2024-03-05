using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class MotionStateEditorView : MotionEditorView
    {
        private readonly TextField nameField = new TextField("Name");
        private readonly ObjectField clipSelectField = new ObjectField("AnimationClip");
        private readonly Label clipInfoLabel = new Label();
        private readonly ScrollView clipScroll = new ScrollView(ScrollViewMode.Vertical);
        private readonly ClipGroupView groupView = new ClipGroupView();
        private readonly MotionStateClipView clipView = new MotionStateClipView();
        private readonly ScaleableClipView scaleableClipView = new ScaleableClipView();

        private AnimaticMotionState motionState;
        private int selectClipIndex = -1;


        public MotionStateEditorView()
        {
            style.flexDirection = FlexDirection.Column;
            nameField.RegisterValueChangedCallback(OnNameChange);
            Add(nameField);

            clipSelectField.objectType = typeof(AnimationClip);
            clipSelectField.RegisterValueChangedCallback(OnClipSelect);
            Add(clipSelectField);

            Add(clipInfoLabel);
            clipScroll.style.left = 0;
            clipScroll.style.right = 0;
            Add(clipScroll);
            clipView.OnDragClipFrameOffset = OnDragClipFrameOffset;
            groupView.AddClipElement(clipView);
            scaleableClipView.OnClipClick = (idx)=>
            {
                selectClipIndex = idx;
                UpdateClipInfo();
            };
            groupView.AddClipElement(scaleableClipView);
            clipScroll.Add(groupView);
        }


        protected override void OnUpdateView(AnimaticMotion motion)
        {
            var state = motion as AnimaticMotionState;
            motionState = state;
            viewDataKey = state.GUID;
            nameField.SetValueWithoutNotify(state.Name);
            UpdateClipInfo();
        }

        private void UpdateClipInfo()
        {
            var clip = motionState.Animation;
            float length = motionState.GetLength();
            float frameRate = clip ? clip.frameRate : 30;
            if (clip)
            {
                clipInfoLabel.text = $"动画信息：长度 = {clip.length}, 帧率 = {clip.frameRate}, {(clip.isHumanMotion ? "人形" : "非人形")}";
                length = Mathf.Max(length, clip.length);
            }
            else
            {
                clipInfoLabel.text = "动画信息：";
            }
            clipSelectField.SetValueWithoutNotify(clip);
            groupView.SetFrameInfo(Mathf.RoundToInt(length * frameRate), frameRate);
            clipView.UpdateSelectClipIndex(motionState, selectClipIndex);
            scaleableClipView.UpdateClips(motionState);
        }

        private void OnDragClipFrameOffset(ClipDragType type, int offset)
        {
            if (motionState.Clips != null && selectClipIndex >= 0 && selectClipIndex < motionState.Clips.Length)
            {
                var animation = motionState.Animation;
                if (animation == null)
                    return;
                int frameCount = Mathf.RoundToInt(animation.length * animation.frameRate);
                var clip = motionState.Clips[selectClipIndex];
                if (type == ClipDragType.DragClip)
                {
                    int start = Mathf.Clamp(clip.StartFrame + offset, 0, frameCount - clip.FrameCount);
                    if (clip.StartFrame == start)
                        return;
                    clip.StartFrame = start;
                }
                else if (type == ClipDragType.DragLeftHandle)
                {
                    int start = Mathf.Clamp(clip.StartFrame + offset, 0, frameCount - clip.FrameCount);
                    if (clip.StartFrame == start)
                        return;
                    int count = Mathf.Clamp(clip.FrameCount - offset, 1, frameCount - clip.StartFrame);
                    if (clip.FrameCount == count)
                        return;
                    clip.StartFrame = start;
                    clip.FrameCount = count;
                }
                else if (type == ClipDragType.DragRightHandle)
                {
                    int count = Mathf.Clamp(clip.FrameCount + offset, 1, frameCount);
                    if (clip.FrameCount == count)
                        return;
                    clip.FrameCount = count;
                }
                RegistUndo("move scaleable clip");
                motionState.Clips[selectClipIndex] = clip;
                UpdateClipInfo();
            }
        }

        private void OnClipSelect(ChangeEvent<Object> evt)
        {
            if (evt.newValue is AnimationClip clip)
            {
                RegistUndo("change clip");
                motionState.Animation = clip;
                if (string.IsNullOrEmpty(motionState.Name) || motionState.Name.StartsWith("New ", System.StringComparison.OrdinalIgnoreCase))
                {
                    motionState.Name = clip.name;
                }
                OnUpdateView(motionState);
            }
        }

        private void OnNameChange(ChangeEvent<string> evt)
        {
            RegistUndo("change name");
            motionState.Name = evt.newValue;
        }

    }
}
