using System;

namespace Animatic
{
    public static class AssetUtil
    {
        public static AnimaticMotionState CreateState(AnimaticAsset asset, string name)
        {
            var state = new AnimaticMotionState
            {
                GUID = Guid.NewGuid().ToString(),
                Name = name
            };
            asset.States.Add(state);
            return state;
        }

        public static AnimaticMotionBlendTree CreateBlendTree(AnimaticAsset asset, string name)
        {
            var blendTree = new AnimaticMotionBlendTree
            {
                GUID = Guid.NewGuid().ToString(),
                Name = name
            };
            asset.BlendTree.Add(blendTree);
            return blendTree;
        }

        public static AnimaticMotion FindMotion(this AnimaticAsset asset, string guid)
        {
            foreach (var motion in asset.Motions)
            {
                if (motion.GUID == guid)
                    return motion;
            }
            return null;
        }
        public static AnimaticMotion FindMotionByName(this AnimaticAsset asset, string name)
        {
            foreach (var motion in asset.Motions)
            {
                if (motion.Name == name)
                    return motion;
            }
            return null;
        }

        public static bool HasName(this AnimaticAsset asset, string name)
        {
            foreach (var motion in asset.Motions)
            {
                if (motion.Name == name)
                    return true;
            }
            return false;
        }
    }
}
