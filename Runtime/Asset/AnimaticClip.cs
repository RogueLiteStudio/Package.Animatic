using UnityEngine;
namespace Animatic
{
    [System.Serializable]
    public struct ScaleableClip
    {
        public int StartFrame;
        public int FrameCount;
        public float Speed;
    }

    [System.Serializable]
    public class AnimaticClip : AnimaticMotion
    {
        public AnimationClip Animation;
        public ScaleableClip[] Clips;

        public float GetLength()
        {
            if (Animation)
            {
                float length = Animation.length;
                if (!Clips.IsEmpty())
                {
                    float frameTime = 1/Animation.frameRate;
                    int frameCount = Mathf.RoundToInt(length / Animation.frameRate);
                    float clipsLength = 0;
                    for (int i=0; i<Clips.Length; ++i)
                    {
                        if (Clips[i].StartFrame >= frameCount)
                            continue;
                        int frame = Clips[i].FrameCount;
                        if (frame + Clips[i].StartFrame > frameCount)
                        {
                            frame = frameCount - Clips[i].StartFrame;
                        }
                        clipsLength += (frame * frameTime)/ Clips[i].Speed;
                    }
                    length = clipsLength;
                }
                return length;
            }
            return 0;
        }
    }
}