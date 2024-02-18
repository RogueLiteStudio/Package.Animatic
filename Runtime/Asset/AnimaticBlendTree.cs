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
            foreach (var m in Motions)
            {
                if (m.Clip)
                    return m.Clip.length;
            }
            return 0;
        }
    }
}
