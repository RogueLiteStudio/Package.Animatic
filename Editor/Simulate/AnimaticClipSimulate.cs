using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

namespace Animatic
{
    public class AnimaticClipSimulate : TMotionSimulate<AnimaticMotionState>
    {
        private AnimationClipPlayable clipPlayable;
        public override void Connect<V>(V destination, int destinationInputPort)
        {
            if (clipPlayable.IsValid())
                destination.ConnectInput(destinationInputPort, clipPlayable, 0);
        }

        public override void Destroy()
        {
            if(clipPlayable.IsValid())
                clipPlayable.Destroy();
        }

        protected override bool RebuildCheck(AnimaticMotionState motion)
        {
            return !clipPlayable.IsNull() && clipPlayable.GetAnimationClip() != motion.Animation;
        }

        protected override void OnSimulate(AnimaticMotionState motion, float passTime, float blendParam)
        {
            var clip = motion.Animation;
            if (clip == null)
                return;
            float frameTime = 1 / clip.frameRate;
            int frameCount = Mathf.RoundToInt(clip.length * clip.frameRate);
            for (int i = 0; i < motion.Clips.Length; ++i)
            {
                var scaleableClip = motion.Clips[i];
                int endFrame = scaleableClip.StartFrame + scaleableClip.FrameCount;
                if (endFrame > frameCount)
                {
                    endFrame = frameCount;
                }
                float clipLength = (endFrame - scaleableClip.StartFrame) * frameTime / scaleableClip.Speed;
                if (clipLength < passTime)
                {
                    passTime -= clipLength;
                    continue;
                }
                passTime = scaleableClip.StartFrame * frameTime + passTime * scaleableClip.Speed;
                break;
            }
            clipPlayable.SetTime(passTime);
        }

        protected override void OnBuild(PlayableGraph graph, AnimaticMotionState motion)
        {
            if (motion.Animation == null)
                return;
            clipPlayable = AnimationClipPlayable.Create(graph, motion.Animation);
        }

        public override bool Valid()
        {
            return clipPlayable.IsValid();
        }
    }
}
