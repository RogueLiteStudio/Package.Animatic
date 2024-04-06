using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
namespace Animatic
{
    public enum AnimaticPlayingState
    {
        None,
        Pause,
        Playing,
    }

    [RequireComponent(typeof(Animator))]
    public class AnimaticPlayer : MonoBehaviour
    {
        [SerializeField]
        private AnimaticAsset asset;
        [SerializeField]
        private string defaultState;
        //AnimaticState
        private readonly Dictionary<string, IAnimaticState> states = new Dictionary<string, IAnimaticState>();
        private readonly List<IAnimaticState> waitDestoryState = new List<IAnimaticState>();
        private IAnimaticState currentState;
        //Playable
        private PlayableGraph graph;
        private Playable rootPlayable;
        private MixerTransitionPlayable mixerTransition;
        //运行时状态
        private float speed = 1;
        private AnimaticPlayingState playingState;
        public AnimaticPlayingState PlayingState=>playingState;

        public AnimaticAsset CurrentAsset=>asset;

        public float Speed => speed;

        private void Start()
        {
            var animator = GetComponent<Animator>();
            graph = PlayableGraph.Create();
            var mixer = AnimationMixerPlayable.Create(graph, 0);
            var transitionPlayable = ScriptPlayable<MixerTransitionPlayable>.Create(graph, 1);
            transitionPlayable.ConnectInput(0, mixer, 0);
            rootPlayable = transitionPlayable;
            mixerTransition = transitionPlayable.GetBehaviour();
            mixerTransition.Mixer = mixer;
            mixerTransition.Player = this;
            var playableOutput = AnimationPlayableOutput.Create(graph, "AnimaticPlayer", animator);
            playableOutput.SetSourcePlayable(transitionPlayable);
            graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            graph.Play();
            if (!string.IsNullOrEmpty(defaultState))
            {
                Play(defaultState);
            }
        }

        private void OnDestroy()
        {
            graph.Destroy();
        }

        private void OnEnable()
        {
            if (playingState == AnimaticPlayingState.Pause)
            {
                playingState = AnimaticPlayingState.Playing;
                rootPlayable.SetSpeed(speed);
            }
        }

        private void OnDisable()
        {
            if (playingState == AnimaticPlayingState.Playing)
            {
                playingState = AnimaticPlayingState.Pause;
                rootPlayable.SetSpeed(0);
            }
        }

        public void SetSpeed(float speed)
        {
            if (this.speed != speed)
            {
                this.speed = speed;
                if (playingState == AnimaticPlayingState.Pause)
                {
                    speed = 0;
                }
                rootPlayable.SetSpeed(speed);
            }
        }


        public void Play(string stateName)
        {
            if (currentState != null && currentState.Motion.Name == stateName)
            {
                return;
            }
            if (!asset)
            {
                Debug.LogError($"播放失败 缺少 Asset : {stateName} --> {name}", this);
                return;
            }
            var state = GetState(stateName);
            if (state == null)
            {
                Debug.LogWarning($"播放失败，不存在的AnimaticState : {stateName} --> {asset.name}", this);
                return;
            }
            float duration = 0;
            if (currentState != null)
            {
                duration = GetTransitionDuration(currentState, state);
            }
            currentState = state;
            mixerTransition.Play(state, duration);
        }

        public void SetAsset(AnimaticAsset asset)
        {
            if (this.asset == asset)
                return;
            this.asset = asset;
#if UNITY_EDITOR
            if (!Application.IsPlaying(gameObject))
                return;
#endif
            waitDestoryState.AddRange(states.Values);
            states.Clear();
        }

        public void OnSwitchStateFinish()
        {
            foreach (var state in waitDestoryState)
            {
                state.Destroy();
            }
            waitDestoryState.Clear();
        }

        private IAnimaticState GetState(string name)
        {
            if (!states.TryGetValue(name, out var state))
            {
                state = asset.CreateStateFromAsset(name);
                if (state != null)
                {
                    states.Add(name, state);
                    state.Init(graph);
                }
            }
            return state;
        }

        private float GetTransitionDuration(IAnimaticState from, IAnimaticState to)
        {
            string fromGUID = from.Motion.GUID;
            string toGUID = to.Motion.GUID;
            foreach (var t in asset.Transition)
            {
                if (t.SourceGUID == fromGUID && t.DestGUID == toGUID)
                {
                    return t.Duration;
                }
            }
            return 0;
        }


#if UNITY_EDITOR
        [ContextMenu("SwitchDefultState")]
        private void SwitchDefultState()
        {
            if (!string.IsNullOrWhiteSpace(defaultState))
            {
                Play(defaultState);
            }
        }
#endif
    }
}
