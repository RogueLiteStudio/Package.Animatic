using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animatic.Runtime
{
    public class AnimaticClipState : IAnimaticState
    {
        public AnimationClip Clip;
        public float Speed = 1;
        public bool Loop;
        public AnimationClipPlayable Playable;
        private ScriptPlayable<ClipMonitorPlayable> monitorPlayable;
        private float passTime;
        private int loopCount;
        private float ScaleDuration;
        public void Connect<V>(V destination, int destinationInputPort) where V : struct, IPlayable
        {
            destination.ConnectInput(destinationInputPort, monitorPlayable, 0);
        }

        public void Init(PlayableGraph graph)
        {
            ScaleDuration = Clip.length / Speed;
            monitorPlayable = ScriptPlayable<ClipMonitorPlayable>.Create(graph, 1);
            monitorPlayable.GetBehaviour().Owner = this;
            Playable = AnimationClipPlayable.Create(graph, Clip);
            monitorPlayable.ConnectInput(0, Playable, 0);
        }

        public void Play()
        {
            loopCount = 0;
            Playable.SetTime(0);
            Playable.SetSpeed(Speed);
        }

        public float GetTime()
        {
            return passTime;
        }

        public int GetLoopCount()
        {
            return loopCount;
        }

        public void OnPrareFrame(FrameData info)
        {
            float dt = info.deltaTime * info.effectiveSpeed;
            passTime += dt;
            if (passTime > ScaleDuration)
            {
                if (!Loop)
                {
                    passTime = ScaleDuration;
                    Playable.SetTime(Clip.length);
                    Playable.SetSpeed(0);
                    return;
                }
                loopCount++;
                passTime -= ScaleDuration;
                Playable.SetTime(passTime);
            }
        }
    }
}
