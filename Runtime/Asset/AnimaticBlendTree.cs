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
    }
}
