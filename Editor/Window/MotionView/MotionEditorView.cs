using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace Animatic
{
    public class MotionEditorView : VisualElement
    {
        public AnimaticAsset Asset;
        public AnimaticSimulate Simulate;

        public System.Action OnNameChanged;

        protected readonly IVisualElementScheduledItem playingTimer;

        protected virtual AnimaticMotion Motion => null;

        protected readonly TextField nameField = new TextField("Name");
        private readonly Toggle loopField = new Toggle("Loop");

        public MotionEditorView()
        {
            playingTimer = schedule.Execute(OnPlayingTimer).Every(10);
            playingTimer.Pause();

            style.flexDirection = FlexDirection.Column;
            nameField.RegisterValueChangedCallback(OnNameChange);
            Add(nameField);
            loopField.RegisterValueChangedCallback(OnLoopChange);
            Add(loopField);
        }
        private void OnNameChange(ChangeEvent<string> evt)
        {
            RegistUndo("change name");
            Motion.Name = evt.newValue;
            OnNameChanged?.Invoke();
        }

        private void OnLoopChange(ChangeEvent<bool> evt)
        {
            RegistUndo("change loop");
            Motion.Loop = evt.newValue;
        }

        public void UpdateView()
        {
            var motion = Asset.Motions.FirstOrDefault(it=>it.GUID == viewDataKey);
            nameField.SetValueWithoutNotify(motion.Name);
            loopField.SetValueWithoutNotify(motion.Loop);
            OnUpdateView(motion);
        }

        public void SetActive(bool active)
        {
            var newStyle = active ? DisplayStyle.Flex : DisplayStyle.None;
            if (style.display != newStyle)
            {
                style.display = newStyle;
                if (!active)
                {
                    playingTimer.Pause();
                    OnDeActive();
                }
                else
                {
                    OnActive();
                }
            }
        }

        protected virtual void OnDeActive(){ }

        protected virtual void OnActive() { }


        protected void RegistUndo(string name)
        {
            Undo.RegisterCompleteObjectUndo(Asset, name);
            EditorUtility.SetDirty(Asset);
        }

        protected virtual void OnUpdateView(AnimaticMotion motion)
        {

        }
        protected virtual void OnPlayingTimer(TimerState timerState) { }
    }
}
