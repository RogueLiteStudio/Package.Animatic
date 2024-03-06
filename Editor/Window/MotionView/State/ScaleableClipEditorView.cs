using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class ScaleableClipEditorView : VisualElement
    {
        private readonly IntegerField startField = new IntegerField("开始帧");
        private readonly IntegerField countField = new IntegerField("帧长度");
        private readonly FloatField speedField = new FloatField("速度");
        private ScaleableClip Value;
        private int index;
        private int maxFrameCount;

        public System.Action<int, ScaleableClip> OnValueChange;
        public ScaleableClipEditorView()
        {
            style.flexDirection = FlexDirection.Column;
            startField.RegisterValueChangedCallback(OnStartChange);
            countField.RegisterValueChangedCallback(OnCountChange);
            speedField.RegisterValueChangedCallback(OnSpeedChange);
            Add(startField);
            Add(countField);
            Add(speedField);
        }

        public void Refresh(ScaleableClip clip, int index, int maxFrameCount)
        {
            Value = clip;
            this.index = index;
            this.maxFrameCount = maxFrameCount;
            startField.SetValueWithoutNotify( clip.StartFrame);
            countField.SetValueWithoutNotify(clip.FrameCount);
            speedField.SetValueWithoutNotify(clip.Speed);
        }
        private void OnStartChange(ChangeEvent<int> evt)
        {
            Value.StartFrame = Mathf.Clamp(evt.newValue, 0, maxFrameCount -1);
            Value.StartFrame = Mathf.Clamp(Value.StartFrame, 0, maxFrameCount - Value.FrameCount);
            if (Value.StartFrame != evt.newValue)
            {
                startField.SetValueWithoutNotify(Value.StartFrame);
            }
            OnValueChange?.Invoke(index, Value);
        }
        private void OnCountChange(ChangeEvent<int> evt)
        {
            Value.FrameCount = Mathf.Clamp(evt.newValue, 1, maxFrameCount - Value.StartFrame);
            if (Value.FrameCount != evt.newValue)
            {
                countField.SetValueWithoutNotify(Value.FrameCount);
            }
            OnValueChange?.Invoke(index, Value);
        }
        private void OnSpeedChange(ChangeEvent<float> evt)
        {
            if (Mathf.Approximately(evt.newValue, 0))
            {
                speedField.SetValueWithoutNotify(Value.Speed);
                return;
            }
            Value.Speed = evt.newValue;
            OnValueChange?.Invoke(index, Value);
        }
    }
}
