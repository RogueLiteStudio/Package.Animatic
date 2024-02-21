using UnityEngine.Playables;

namespace Animatic
{
    public abstract class TMotionSimulate<T> : IMotionSimulate where T : AnimaticMotion
    {
        public abstract void Destroy();

        public bool RebuildCheck(AnimaticMotion motion)
        {
            if (motion is T t)
            {
                return RebuildCheck(t);
            }
            return true;
        }

        protected abstract bool RebuildCheck(T motion);

        public void Simulate(AnimaticMotion motion, float passTime, float blendParam)
        {
            OnSimulate(motion as T, passTime, blendParam);
        }

        protected abstract void OnSimulate(T motion, float passTime, float blendParam);

        public abstract void Connect<V>(V destination, int destinationInputPort) where V : struct, IPlayable;

        public void Build(PlayableGraph graph, AnimaticMotion motion)
        {
            OnBuild(graph, motion as T);
        }

        protected abstract void OnBuild(PlayableGraph graph, T motion);
        public abstract bool Valid();
    }
}
