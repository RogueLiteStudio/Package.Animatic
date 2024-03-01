using System.Linq;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animatic
{
    public class AnimaticBlendTreeSimulate : TMotionSimulate<AnimaticMotionBlendTree>
    {
        private readonly static AnimationClipPlayable[] empty = new AnimationClipPlayable[0];
        private AnimationClipPlayable[] clipPlayables = empty;
        private AnimationMixerPlayable mixerPlayable;

        public override void Connect<V>(V destination, int destinationInputPort)
        {
            destination.ConnectInput(destinationInputPort, mixerPlayable, 0);
        }

        public override void Destroy()
        {
            for (int i = 0; i < clipPlayables.Length; ++i)
            {
                clipPlayables[i].Destroy();
            }
            clipPlayables = empty;
            if (mixerPlayable.IsValid())
            {
                mixerPlayable.Destroy();
            }
        }

        protected override void OnBuild(PlayableGraph graph, AnimaticMotionBlendTree motion)
        {
            int validCount = motion.Motions.Count(it => it.Clip);
            if (validCount == 0)
                return;
            mixerPlayable = AnimationMixerPlayable.Create(graph, validCount);
            clipPlayables = new AnimationClipPlayable[validCount];
            for (int i=0; i<validCount; ++i)
            {
                if (!motion.Motions[i].Clip)
                {
                    --i;
                    continue;
                }
                clipPlayables[i] = AnimationClipPlayable.Create(graph, motion.Motions[i].Clip);
                graph.Connect(clipPlayables[i], 0, mixerPlayable, i);
                mixerPlayable.SetInputWeight(i, 0);
            }
        }

        protected override void OnSimulate(AnimaticMotionBlendTree motion, float passTime, float blendParam)
        {
            if (clipPlayables.Length == 0)
                return;
            int idx = 0;
            int preIndex = -1;
            int nextIndex = -1;
            float preValue = float.MinValue;
            float nextValue = float.MaxValue;
            for (int i=0; i<motion.Motions.Count; ++i)
            {
                var m = motion.Motions[i];
                if (!m.Clip)
                    continue;
                if (m.Threshold >= blendParam && m.Threshold < nextValue)
                {
                    nextValue = m.Threshold;
                    nextIndex = idx;
                }
                if (m.Threshold <= blendParam && m.Threshold > preValue)
                {
                    preValue = m.Threshold;
                    preIndex = idx;
                }
                idx++;
            }
            float preWeight = 0;
            float nextWeight = 0;
            if (preIndex >=0 && nextIndex >= 0)
            {
                preWeight = (blendParam - preValue) / (nextValue - preValue);
                nextWeight = 1 - preWeight;
            }
            else if (preIndex >= 0)
            {
                preWeight = 1;
            }
            else if (nextIndex >= 0)
            {
                nextWeight = 1;
            }
            float length = motion.GetLength();

            if (motion.Loop)
            {
                passTime %= length;
            }
            else
            {
                passTime = Mathf.Min(passTime, length);
            }
            for (int i=0; i<clipPlayables.Length; ++i)
            {
                if (i == preIndex)
                {
                    mixerPlayable.SetInputWeight(i, preWeight);
                }
                else if (i == nextIndex)
                {
                    mixerPlayable.SetInputWeight(i, nextWeight);
                }
                else
                {
                    mixerPlayable.SetInputWeight(i, 0);
                }
                clipPlayables[i].SetTime(passTime);
            }

        }

        protected override bool RebuildCheck(AnimaticMotionBlendTree motion)
        {
            int validCount = motion.Motions.Count(it=>it.Clip);
            if (clipPlayables.Length != validCount)
                return true;
            for (int i=0; i<clipPlayables.Length; ++i)
            {
                if (clipPlayables[i].GetAnimationClip() != motion.Motions[i].Clip)
                    return true;
            }
            return false;
        }

        public override bool Valid()
        {
            return clipPlayables.Length > 0;
        }
    }
}
