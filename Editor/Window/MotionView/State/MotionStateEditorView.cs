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
        private readonly ClipGroupView groupView = new ClipGroupView();

        private AnimaticMotionState motionState;


        public MotionStateEditorView()
        {
            style.flexDirection = FlexDirection.Column;
            nameField.RegisterValueChangedCallback(OnNameChange);
            Add(nameField);

            clipSelectField.objectType = typeof(AnimationClip);
            clipSelectField.RegisterValueChangedCallback(OnClipSelect);
            Add(clipSelectField);

            Add(clipInfoLabel);
            Add(groupView);
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
        }

        private void OnClipSelect(ChangeEvent<Object> evt)
        {
            if (evt.newValue is AnimationClip clip)
            {
                RegistUndo("change clip");
                motionState.Animation = clip;
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
