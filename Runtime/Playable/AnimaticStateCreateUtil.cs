using UnityEngine;

namespace Animatic
{
    public static class AnimaticStateCreateUtil
    {
        public static IAnimaticState CreateState(AnimaticMotion motion)
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
                    };
                }
            }
            var state = new AnimaticQueueState
            {
                Clip = motion.Animation,
                Loop = motion.Loop,
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
    }
}
