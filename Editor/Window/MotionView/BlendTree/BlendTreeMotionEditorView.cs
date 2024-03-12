using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class BlendTreeMotionEditorView : VisualElement
    {
        private readonly ObjectField clipSelector = new ObjectField();
        private readonly FloatField thresholdField = new FloatField("Threshold", 10);
        private AnimaticMotionBlendTree.Motion data;
        private int index;
        private bool valueHasChanged;

        public System.Action<int, AnimaticMotionBlendTree.Motion> OnChange;

        public BlendTreeMotionEditorView()
        {
            style.flexDirection = FlexDirection.Row;

            clipSelector.objectType = typeof(AnimationClip);
            clipSelector.allowSceneObjects = false;
            clipSelector.RegisterValueChangedCallback(OnClipChange);
            clipSelector.style.minWidth = 150;
            Add(clipSelector);
            thresholdField.labelElement.style.minWidth = 0;
            thresholdField.style.minWidth = 150;
            thresholdField.RegisterValueChangedCallback(OnThresholdChange);
            thresholdField.RegisterCallback<FocusOutEvent>((evt) => 
            { 
                if (valueHasChanged)
                {
                    OnChange?.Invoke(index, data);
                    valueHasChanged = false;
                }
            });
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
            valueHasChanged = true;
        }

        private void OnClipChange(ChangeEvent<Object> evt)
        {
            data.Clip = evt.newValue as AnimationClip;
            OnChange?.Invoke(index, data);
        }
    }
}
