using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace Animatic
{
    public class TransitionItemView : VisualElement
    {
        private readonly Button motionSelect = new Button();
        private readonly FloatField durationField = new FloatField("过渡时长");
        public System.Func<IEnumerable<string>> GetSelectableList;
        public System.Action<string, int> OnMotionChanged;
        public System.Action<float, int> OnDurationChanged;

        private string motionName;
        private bool valueHasChanged;
        private float value;
        private float maxValue;
        private int index;
        public TransitionItemView()
        {
            style.flexDirection = FlexDirection.Row;
            motionSelect.style.minWidth = 100;
            motionSelect.clicked += OnClickSelect;
            Add(motionSelect);
            durationField.labelElement.style.minWidth = 0;
            durationField.style.minWidth = 100;
            durationField.RegisterValueChangedCallback(OnValueChanged);
            durationField.RegisterCallback<FocusOutEvent>((evt) =>
            {
                if (valueHasChanged)
                {
                    OnDurationChanged?.Invoke(value, index);
                    valueHasChanged = false;
                }
            });
            Add(durationField);
        }

        public void UpdateView(TransitionInfo transition, float currentMotionLength, int index)
        {
            this.index = index;
            value = transition.Duration;
            if (transition.Source != null)
            {
                motionName = transition.Source.Name;
                motionSelect.text = transition.Source.Name;
                maxValue = Mathf.Min(transition.Source.GetLength(), currentMotionLength);
                durationField.SetValueWithoutNotify(transition.Duration);
                durationField.isReadOnly = false;
                motionSelect.style.color = Color.white;
            }
            else
            {
                motionName = null;
                motionSelect.text = transition.GUID == null ? "" : "Miss Motion";
                maxValue = currentMotionLength;
                durationField.SetValueWithoutNotify(transition.Duration);
                motionSelect.style.color = Color.red;
                durationField.isReadOnly = true;
            }
        }

        private void OnClickSelect()
        {
            if (GetSelectableList == null)
                return;
            var names = GetSelectableList();
            GenericMenu menu = new GenericMenu();
            bool hasSelf = false;
            foreach ( var name in names )
            {
                if (name == motionName)
                {
                    hasSelf = true;
                    menu.AddDisabledItem(new GUIContent(name), true);
                }
                else
                {
                    menu.AddItem(new GUIContent(name), false, (obj) => OnMotionChanged?.Invoke(obj as string, index), name);
                }
            }
            if (!hasSelf && motionName != null)
            {
                menu.AddDisabledItem(new GUIContent(motionSelect.text), true);
            }
            menu.ShowAsContext();
        }

        private void OnValueChanged(ChangeEvent<float> evt)
        {
            value = Mathf.Clamp(evt.newValue, 0, maxValue);
            valueHasChanged = true;
        }

    }
}
