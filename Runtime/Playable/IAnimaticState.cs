﻿using UnityEngine.Playables;

namespace Animatic
{
    public interface IAnimaticState
    {
        AnimaticMotion Motion { get; set; }
        void Init(PlayableGraph graph);
        void Connect<V>(V destination, int destinationInputPort) where V : struct, IPlayable;
        void Play();
        float GetTime();
        int GetLoopCount();
        void OnPrareFrame(FrameData info);
        void SetBlendParam(float value) { }
        void Destroy();
    }
}
