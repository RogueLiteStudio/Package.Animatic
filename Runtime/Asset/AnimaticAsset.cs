using System.Collections.Generic;
using UnityEngine;

namespace Animatic
{
    [CreateAssetMenu(fileName = "AnimaticAsset", menuName = "Animatic/AnimaticAsset")]
    public class AnimaticAsset : ScriptableObject
    {
        public List<AnimaticClip> Clips = new List<AnimaticClip>();
        public List<AnimaticBlendTree> BlendTree = new List<AnimaticBlendTree>();
        public List<AnimaticTransition> Transition = new List<AnimaticTransition>();

        public IEnumerable<AnimaticMotion> Motions
        {
            get
            {
                foreach (var clip in Clips)
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
