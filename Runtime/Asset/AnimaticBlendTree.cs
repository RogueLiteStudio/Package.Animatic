using System.Collections.Generic;
using UnityEngine;

namespace Animatic
{
    [System.Serializable]
    public class AnimaticBlendTree
    {
        [System.Serializable]
        public struct Motion
        {
            public AnimationClip Clip;
            public float Threshold;
        }
        public string GUID;
        public string Name;
        public bool Loop;
        public List<Motion> Motions = new List<Motion>();
    }
}
