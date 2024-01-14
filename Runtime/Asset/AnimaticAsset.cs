using System.Collections.Generic;
using UnityEngine;

namespace Animatic
{
    public class AnimaticAsset : ScriptableObject
    {
        public List<AnimaticMotion> Motions = new List<AnimaticMotion>();
        public List<AnimaticTransition> Transition = new List<AnimaticTransition>();
        public List<AnimaticBlendTree> BlendTree = new List<AnimaticBlendTree>();
    }
}
