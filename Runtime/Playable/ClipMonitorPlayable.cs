using UnityEngine.Playables;

namespace Animatic
{
    internal class ClipMonitorPlayable : PlayableBehaviour
    {
        public IAnimaticState Owner;

        public override void PrepareFrame(Playable playable, FrameData info)
        {
            Owner?.OnPrareFrame(info);
        }

        public override void OnPlayableDestroy(Playable playable)
        {
            Owner = default;
        }
    }
}
