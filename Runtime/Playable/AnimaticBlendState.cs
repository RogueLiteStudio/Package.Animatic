using System;
using UnityEngine.Playables;
using UnityEngine.Animations;
using UnityEngine;

namespace Animatic
{
    public class AnimaticBlendState : IAnimaticState
    {
        public float MinThreshold;
        public float MaxThreshold;
        public bool Loop;
        public AnimaticMotionBlendTree.Motion[] Motions;
        public AnimaticMotion Motion { get; set; }
        private float duration;
        private float passTime;
        private float blendParam = float.MaxValue;
        private int loopCount;
        private AnimationClipPlayable[] clipPlayables;
        private AnimationMixerPlayable mixerPlayable;
        private ScriptPlayable<ClipMonitorPlayable> monitorPlayable;

        public void Connect<V>(V destination, int destinationInputPort) where V : struct, IPlayable
        {
            destination.ConnectInput(destinationInputPort, monitorPlayable, 0);
        }

        public void Destroy()
        {
            for (int i = 0; i < clipPlayables.Length; ++i)
            {
                clipPlayables[i].Destroy();
            }
            monitorPlayable.Destroy();
            mixerPlayable.Destroy();
        }

        public int GetLoopCount()
        {
            return loopCount;
        }

        public float GetTime()
        {
            return passTime;
        }

        public void Init(PlayableGraph graph)
        {
            monitorPlayable = ScriptPlayable<ClipMonitorPlayable>.Create(graph, 1);
            monitorPlayable.GetBehaviour().Owner = this;
            mixerPlayable = AnimationMixerPlayable.Create(graph, Motions.Length);
            monitorPlayable.ConnectInput(0, mixerPlayable, 0);
            clipPlayables = new AnimationClipPlayable[Motions.Length];
            duration = 0;
            for (int i=0; i<Motions.Length; ++i)
            {
                clipPlayables[i] = AnimationClipPlayable.Create(graph, Motions[i].Clip);
                graph.Connect(clipPlayables[i], 0, mixerPlayable, i);
                mixerPlayable.SetInputWeight(i, 0);
                duration = Mathf.Max(duration, Motions[i].Clip.length);
            }
        }

        public void OnPrareFrame(FrameData info)
        {
            float dt = info.deltaTime * info.effectiveSpeed;
            passTime += dt;
            for (int i=0; i<Motions.Length; ++i)
            {
                if (passTime > Motions[i].Clip.length)
                {
                    passTime = Motions[i].Clip.length;
                    clipPlayables[i].SetTime(passTime - Motions[i].Clip.length);
                }
            }
            if (passTime > duration)
            {
                if (!Loop)
                {
                    for (int i=0; i < clipPlayables.Length; ++i)
                    {
                        clipPlayables[i].SetSpeed(0);
                    }
                    return;
                }
                passTime -= duration;
                loopCount++;
            }
        }

        public void Play()
        {
            passTime = 0;
            loopCount = 0;
            blendParam = float.MaxValue;
            for (int i=0; i< clipPlayables.Length; ++i)
            {
                clipPlayables[i].SetTime(0);
                clipPlayables[i].SetSpeed(1);
            }
            SetBlendParam(0);
        }

        public void SetBlendParam(float value)
        {
            value = Mathf.Clamp(value, MinThreshold, MaxThreshold);
            if (blendParam == value)
                return;
            blendParam = value;
            for (int i=0; i<Motions.Length-1; ++i)
            {
                if (value >= Motions[i].Threshold && value <= Motions[i+1].Threshold)
                {
                    float range = Motions[i + 1].Threshold - Motions[i].Threshold;
                    float w1 = (value - Motions[i].Threshold) / range;
                    float w2 = 1 - w1;
                    mixerPlayable.SetInputWeight(i, w1);
                    mixerPlayable.SetInputWeight(i + 1, w2);
                    ++i;
                }
                else
                {
                    mixerPlayable.SetInputWeight(i, 0);
                }
            }
        }
    }
}
