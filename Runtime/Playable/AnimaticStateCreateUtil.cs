using System.Linq;
using UnityEngine;

namespace Animatic
{
    public static class AnimaticStateCreateUtil
    {
        public static IAnimaticState CreateState(AnimaticClip motion)
        {
            if (!motion.Animation)
                return null;
            float length = motion.Animation.length;
            float frameTime = 1 / motion.Animation.frameRate;
            int frameCount = Mathf.RoundToInt(length / frameTime);
            if (motion.Clips.IsEmpty())
            {
                return new AnimaticClipState
                {
                    Clip = motion.Animation,
                    Loop = motion.Loop,
                    Speed = 1,
                    Motion = motion,
                };
            }
            if(motion.Clips.Length == 1)
            {
                var clip = motion.Clips[0];
                if (clip.StartFrame == 0 && clip.FrameCount >= frameCount)
                {
                    return new AnimaticClipState
                    {
                        Clip = motion.Animation,
                        Loop = motion.Loop,
                        Speed = clip.Speed,
                        Motion = motion,
                    };
                }
            }
            var state = new AnimaticQueueState
            {
                Clip = motion.Animation,
                Loop = motion.Loop,
                Motion = motion,
            };
            state.SpeedClips = new AnimaticQueueState.SpeedClip[motion.Clips.Length];
            for (int i=0; i<state.SpeedClips.Length; ++i)
            {
                var clip = motion.Clips[i];

                var speedClip = new AnimaticQueueState.SpeedClip
                {
                    Speed = clip.Speed,
                    StatTime = clip.StartFrame * frameTime,
                    Duration = clip.FrameCount * frameTime,
                };
                if (speedClip.StatTime + speedClip.Duration > length)
                {
                    speedClip.Duration = length - speedClip.StatTime;
                }
                speedClip.ScaleDuration = speedClip.Duration / speedClip.Speed;
                state.SpeedClips[i] = speedClip;
            }
            return state;
        }

        public static IAnimaticState CreateState(AnimaticBlendTree blendTree)
        {
            int validCount = blendTree.Motions.Count(it=>it.Clip);
            if (validCount < 1)
                return null;
            if (validCount == 1)
            {
                var m = blendTree.Motions.First(it=>it.Clip);
                return new AnimaticClipState
                {
                    Motion = blendTree,
                    Clip = m.Clip,
                    Loop = blendTree.Loop,
                    Speed = 1,
                };
            }
            float min = float.MaxValue;
            float max = float.MinValue;
            foreach (var m in blendTree.Motions)
            {
                if (!m.Clip)
                    continue;
                min = Mathf.Min(m.Threshold, min);
                max = Mathf.Max(m.Threshold, max);
            }
            AnimaticBlendTree.Motion[] motions = blendTree.Motions.Where(it=>it.Clip).OrderBy(it=>it.Threshold).ToArray();
            return new AnimaticBlendState 
            {
                Motion = blendTree,
                Motions = motions, 
                Loop = blendTree.Loop, 
                MinThreshold = min, 
                MaxThreshold = max 
            };

        }

        public static IAnimaticState CreateStateFromAsset(this AnimaticAsset asset, string name)
        {
            var motion = asset.Clips.FirstOrDefault(it => it.Name == name);
            if (motion != null)
                return CreateState(motion);
            var blendTree = asset.BlendTree.FirstOrDefault(it => it.Name == name);
            if (blendTree != null)
                return CreateState(blendTree);
            return null;
        }
    }
}
