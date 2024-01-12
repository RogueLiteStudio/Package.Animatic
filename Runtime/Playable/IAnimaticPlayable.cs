using UnityEngine.Playables;

namespace Animatic
{
    public interface IAnimaticPlayable
    {
        void Init(PlayableGraph graph);
        void Play();
    }
}
