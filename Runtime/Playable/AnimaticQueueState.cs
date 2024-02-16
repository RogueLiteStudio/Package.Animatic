using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animatic
{
    public class AnimaticQueueState : IAnimaticState
    {
        public struct SpeedClip 
        {
            public float Speed;
            public float StatTime;
            public float Duration;//动画片段时长
            public float ScaleDuration;//速度影响后的时长
        }

        public AnimationClip Clip;
        public SpeedClip[] SpeedClips;
        public bool Loop;
        public AnimaticMotion Motion { get; set; }
        private AnimationClipPlayable clipPlayable;
        private float passTime;
        private int playIndex;
        private int loopCount;
        private ScriptPlayable<ClipMonitorPlayable> monitorPlayable;
        public void OnPrareFrame(FrameData info)
        {
            float dt = info.deltaTime * info.effectiveSpeed;
            passTime += dt;
            if (passTime > SpeedClips[playIndex].ScaleDuration)
            {
                passTime -= SpeedClips[playIndex].ScaleDuration;
                passTime /= SpeedClips[playIndex].Speed;
                int preIndex = playIndex++;
                passTime *= SpeedClips[playIndex].Speed;
                if (playIndex >= SpeedClips.Length)
                {
                    playIndex = 0;
                    loopCount++;
                    if (!Loop)
                    {
                        clipPlayable.SetTime(SpeedClips[preIndex].StatTime + SpeedClips[preIndex].Duration);
                        clipPlayable.SetSpeed(0);
                        return;
                    }
                }
                StartPlayCurrent();
            }
        }

        public void Init(PlayableGraph graph)
        {
            monitorPlayable = ScriptPlayable<ClipMonitorPlayable>.Create(graph, 1);
            monitorPlayable.GetBehaviour().Owner = this;
            clipPlayable = AnimationClipPlayable.Create(graph, Clip);
            monitorPlayable.ConnectInput(0, clipPlayable, 0);
        }

        public void Connect<V>(V destination, int destinationInputPort) where V : struct, IPlayable
        {
            destination.ConnectInput(destinationInputPort, monitorPlayable, 0);
        }

        public void Play()
        {
            passTime = 0;
            playIndex = 0;
            loopCount = 0;
            StartPlayCurrent();
        }

        private void StartPlayCurrent()
        {
            clipPlayable.SetTime(SpeedClips[playIndex].StatTime + passTime);
            clipPlayable.SetDuration(SpeedClips[playIndex].Duration);
            clipPlayable.SetSpeed(SpeedClips[playIndex].Speed);
        }

        public float GetTime()
        {
            float time = passTime;
            for (int i=1; i<SpeedClips.Length; ++i)
            {
                time += SpeedClips[i].ScaleDuration;
            }
            return time;
        }

        public int GetLoopCount() { return loopCount; }

        public void Destroy()
        {
            clipPlayable.Destroy();
            monitorPlayable.Destroy();
        }
    }
}
