using System.Collections.Generic;
using UnityEngine;

namespace Animatic
{
    [CreateAssetMenu(fileName = "AnimaticAsset", menuName = "Animatic/AnimaticAsset")]
    public class AnimaticAsset : ScriptableObject
    {
        public List<AnimaticMotionState> States = new List<AnimaticMotionState>();
        public List<AnimaticMotionBlendTree> BlendTree = new List<AnimaticMotionBlendTree>();
        public List<AnimaticTransition> Transition = new List<AnimaticTransition>();

        public IEnumerable<AnimaticMotion> Motions
        {
            get
            {
                foreach (var clip in States)
                {
                    yield return clip;
                }
                foreach (var bt in BlendTree)
                {
                    yield return bt;
                }
            }
        }
    }
}
