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
        public AnimationClipPlayable Playable;
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
                int preIndex = playIndex++;
                if (playIndex >= SpeedClips.Length)
                {
                    playIndex = 0;
                    loopCount++;
                    if (!Loop)
                    {
                        Playable.SetTime(SpeedClips[preIndex].StatTime + SpeedClips[preIndex].Duration);
                        Playable.SetSpeed(0);
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
            Playable = AnimationClipPlayable.Create(graph, Clip);
            monitorPlayable.ConnectInput(0, Playable, 0);
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
            Playable.SetTime(SpeedClips[playIndex].StatTime + passTime);
            Playable.SetDuration(SpeedClips[playIndex].Duration);
            Playable.SetSpeed(SpeedClips[playIndex].Speed);
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
    }
}
