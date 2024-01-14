using UnityEngine.Playables;

namespace Animatic
{
    public interface IAnimaticState
    {
        void Init(PlayableGraph graph);
        void Connect<V>(V destination, int destinationInputPort) where V : struct, IPlayable;
        void Play();
        float GetTime();
        int GetLoopCount();
        public void OnPrareFrame(FrameData info);
    }
}
