using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class BlendTreeMotionEditorView : VisualElement
    {
        private readonly ObjectField clipSelector = new ObjectField();
        private readonly FloatField thresholdField = new FloatField("Threshold");
        private AnimaticMotionBlendTree.Motion data;
        private int index;

        public System.Action<int, AnimaticMotionBlendTree.Motion> OnChange;

        public BlendTreeMotionEditorView()
        {
            style.flexDirection = FlexDirection.Row;

            clipSelector.objectType = typeof(AnimationClip);
            clipSelector.allowSceneObjects = false;
            clipSelector.RegisterValueChangedCallback(OnClipChange);
            Add(clipSelector);
            thresholdField.labelElement.style.minWidth = 0;
            thresholdField.RegisterValueChangedCallback(OnThresholdChange);
            Add(thresholdField);
        }

        public void UpdateView(int index, AnimaticMotionBlendTree.Motion data)
        {
            this.index = index;
            this.data = data;
            clipSelector.SetValueWithoutNotify(data.Clip);
            thresholdField.SetValueWithoutNotify(data.Threshold);
        }

        private void OnThresholdChange(ChangeEvent<float> evt)
        {
            data.Threshold = evt.newValue;
            OnChange?.Invoke(index, data);
        }

        private void OnClipChange(ChangeEvent<Object> evt)
        {
            data.Clip = evt.newValue as AnimationClip;
            OnChange?.Invoke(index, data);
        }
    }
}
