using System.Collections.Generic;
using UnityEngine;

namespace Animatic
{
    [System.Serializable]
    public class AnimaticBlendTree : AnimaticMotion
    {
        [System.Serializable]
        public struct Motion
        {
            public AnimationClip Clip;
            public float Threshold;
        }
        public List<Motion> Motions = new List<Motion>();

        public override float GetLength()
        {
            float length = 0;
            foreach (var m in Motions)
            {
                if (m.Clip)
                {
                    if (length == 0)
                        length = m.Clip.length;
                    else
                        length = Mathf.Min(length, m.Clip.length);
                }
            }
            return length;
        }
    }
}
