using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animatic
{
    public class AnimaticClipState : IAnimaticState
    {
        public AnimationClip Clip;
        public float Speed = 1;
        public bool Loop;
        public AnimaticMotion Motion { get; set; }
        private AnimationClipPlayable clipPlayable;
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
            clipPlayable = AnimationClipPlayable.Create(graph, Clip);
            monitorPlayable.ConnectInput(0, clipPlayable, 0);
        }

        public void Play()
        {
            loopCount = 0;
            clipPlayable.SetTime(0);
            clipPlayable.SetSpeed(Speed);
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
                    clipPlayable.SetTime(Clip.length);
                    clipPlayable.SetSpeed(0);
                    return;
                }
                loopCount++;
                passTime -= ScaleDuration;
                clipPlayable.SetTime(passTime);
            }
        }

        public void Destroy()
        {
            clipPlayable.Destroy();
            monitorPlayable.Destroy();
        }
    }
}
