using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animatic
{
    internal struct TransitionData
    {
        public int FromInput;
        public int ToInput;
        public float Time;
        public float Duration;
        public bool Update(float dt, AnimationMixerPlayable mixer)
        {
            Time += dt;
            if (Time >= Duration)
            {
                mixer.SetInputWeight(FromInput, 0);
                mixer.SetInputWeight(ToInput, 1);
                return false;
            }
            float weight = Mathf.Clamp01(Time / Duration);
            mixer.SetInputWeight(FromInput, 1 - weight);
            mixer.SetInputWeight(ToInput, weight);
            return true;
        }
    }
    public class MixerTransitionPlayable : PlayableBehaviour
    {
        public AnimationMixerPlayable Mixer;
        public AnimaticPlayer Player;
        private int currentInput;
        private int inputCount;
        private TransitionData transition;
        private bool enable;

        public void Play(IAnimaticState state, float transitionDuration)
        {
            if (inputCount < 2)
            {
                inputCount++;
                Mixer.SetInputCount(inputCount);
                currentInput = inputCount - 1;
                if (inputCount == 2)
                {
                    transition = new TransitionData { ToInput = 1, FromInput = 0, Duration = transitionDuration };
                    enable = true;
                }
            }
            else
            {
                int preInput = currentInput;
                currentInput = (currentInput + 1)%2;
                transition = new TransitionData { ToInput = currentInput, FromInput = preInput, Duration = transitionDuration };
                enable = true;
            }
            state.Connect(Mixer, currentInput);
            if (!enable)
            {
                Mixer.SetInputWeight(currentInput, 1);
            }
            state.Play();
        }


        public override void PrepareFrame(Playable playable, FrameData info)
        {
            if (!enable)
                return;
            float dt = info.deltaTime * info.effectiveSpeed;
            enable = transition.Update(dt, Mixer);
            if (!enable)
            {
                Player.OnSwitchStateFinish();
            }
        }
    }
}
