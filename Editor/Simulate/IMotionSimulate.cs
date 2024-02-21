using UnityEngine.Playables;

namespace Animatic
{
    public interface IMotionSimulate
    {
        bool RebuildCheck(AnimaticMotion motion);
        void Build(PlayableGraph graph, AnimaticMotion motion);
        void Connect<V>(V destination, int destinationInputPort) where V : struct, IPlayable;
        void Simulate(AnimaticMotion motion, float passTime, float blendParam);
        void Destroy();
        bool Valid();
    }
}
