using UnityEngine.Playables;
using UnityEngine;
using UnityEngine.Animations;
using UnityEditor;
namespace Animatic
{
    public class AnimaticSimulate : ScriptableObject
    {
        public enum SimulateType
        {
            None,
            Normal,
            CorssFade,
        }

        [SerializeField]
        private AnimaticAsset Asset;
        [SerializeField]
        private GameObject Object;
        [SerializeField]
        private SimulateType simulateType;

        private PlayableGraph playableGraph;
        private AnimationPlayableOutput playableOutput;
        private AnimationMixerPlayable mixerPlayable;

        private IMotionSimulate normal;
        private IMotionSimulate crossFade;

        public bool Evaluate(string motionName, float passTime, float blendParam = 0)
        {
            if (!Asset || !Object)
                return false;
            var motion = GetMotion(motionName);
            if (motion == null)
                return false;
            GraphValidate(SimulateType.Normal);
            SimulateValidate(ref normal, motion, 0);
            if (!normal.Valid())
                return false;
            mixerPlayable.SetInputWeight(0, 1);
            normal.Simulate(motion, passTime, blendParam);

            return true;
        }

        public bool EvaluateOriginal(string motionName, float passTime, int index = 0)
        {
            if (!Asset || !Object)
                return false;
            var motion = GetMotion(motionName);
            if (motion == null)
                return false;
            if (motion is AnimaticMotionState state)
            {
                if (!state.Animation)
                    return false;
                state.Animation.SampleAnimation(Object, passTime);
                SceneView.RepaintAll();
                return true;
            }
            else if (motion is AnimaticMotionBlendTree blendTree)
            {
                if (blendTree.Motions.Count == 0)
                    return false;
                index = Mathf.Clamp(index, 0, blendTree.Motions.Count - 1);
                var clip = blendTree.Motions[index].Clip;
                if (clip != null)
                {
                    clip.SampleAnimation(Object, passTime);
                    SceneView.RepaintAll();
                    return true;
                }
            }
            return false;
        }

        public bool EvaluateCrossFade(string motionName, string crossFadeMotionName, float passTime, float crossDuration, float motionBlendTreeParam = 0, float crossFadeBlendTreeParam2 = 0)
        {
            if (!Asset || !Object)
                return false;
            var motion = GetMotion(motionName);
            var crossFadeMotion = GetMotion(crossFadeMotionName);
            if (motion == null || crossFadeMotion == null)
                return false;
            GraphValidate(SimulateType.CorssFade);
            SimulateValidate(ref normal, motion, 0);
            SimulateValidate(ref crossFade, crossFadeMotion, 1);
            if (!normal.Valid() || !crossFade.Valid())
                return false;
            float motionLength = motion.GetLength();
            float crossFadeLength = crossFadeMotion.GetLength();
            crossDuration = Mathf.Min(crossDuration, motionLength);
            float motionWeight = 1;
            if (passTime > motionLength)
            {
                motionWeight = 0;
            }
            else
            {
                float unCrossTime = motionLength - crossDuration;
                if (passTime > unCrossTime)
                {
                    motionWeight = 1 - (passTime - unCrossTime) / crossDuration;
                }
            }
            float crossFadeWeight = 1 - motionWeight;
            mixerPlayable.SetInputWeight(0, motionWeight);
            mixerPlayable.SetInputWeight(1, crossFadeWeight);
            normal.Simulate(motion, Mathf.Min(passTime, motionLength), motionBlendTreeParam);
            float crossFadeTime = Mathf.Max(0, passTime - motionLength);
            crossFade.Simulate(crossFadeMotion, Mathf.Min(crossFadeTime, crossFadeLength), crossFadeBlendTreeParam2);
            SceneView.RepaintAll();
            return true;
        }


        private void SimulateValidate(ref IMotionSimulate simulate, AnimaticMotion motion, int inputIndex)
        {
            do 
            {
                if (simulate == null)
                    break;
                if (simulate.RebuildCheck(motion))
                {
                    simulate.Destroy();
                    break;
                }
                return;
            } while (false);
            if (motion is AnimaticMotionState)
            {
                simulate = new AnimaticClipSimulate();
                simulate.Build(playableGraph, motion);
                simulate.Connect(mixerPlayable, inputIndex);
            }
            else if (motion is AnimaticMotionBlendTree)
            {
                simulate = new AnimaticClipSimulate();
                simulate.Build(playableGraph, motion);
                simulate.Connect(mixerPlayable, inputIndex);
            }
        }
        private void GraphValidate(SimulateType type)
        {
            if (type != simulateType)
            {
                if (type != SimulateType.CorssFade)
                {
                    if (crossFade != null)
                    {
                        crossFade.Destroy();
                        crossFade = null;
                    }
                }
                if (playableGraph.IsValid())
                {
                    playableGraph.Destroy();
                }
                simulateType = type;
            }
            if (!playableGraph.IsValid())
            {
                playableGraph = PlayableGraph.Create($"{Asset.name}_Simulate");
                var animator = Object.GetComponentInChildren<Animator>();
                playableOutput = AnimationPlayableOutput.Create(playableGraph, "AnimaticSimulate", animator);
                mixerPlayable = AnimationMixerPlayable.Create(playableGraph, type == SimulateType.CorssFade ? 2 : 1);
                playableOutput.SetSourcePlayable(mixerPlayable);
                playableGraph.SetTimeUpdateMode(DirectorUpdateMode.Manual);
                playableGraph.Play();
            }
        }

        private void OnDestroy()
        {
            Asset = null;
            Object = null;
        }

        private void OnDisable()
        {
            //编辑器虚拟机重载时会调用，需要销毁
            if (normal != null)
            {
                normal.Destroy();
                normal = null;
            }
            if (crossFade != null)
            {
                crossFade.Destroy();
                crossFade = null;
            }
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
        }

        private AnimaticMotion GetMotion(string name)
        {
            foreach (var m in Asset.States)
            {
                if (m.Name == name)
                {
                    return m;
                }
            }
            foreach (var m in Asset.BlendTree)
            {
                if (m.Name == name)
                {
                    return m;
                }
            }
            return null;
        }

        public void BindTarget(AnimaticAsset asset, GameObject obj)
        {
            if (Asset == asset && obj == Object)
                return;

            if (obj && !obj.GetComponentInChildren<Animator>())
            {
                Debug.LogError($"绑定 AnimaticSimulate 失败, {obj.name} 缺少有效的 Animator");
                return;
            }
            simulateType = SimulateType.None;
            Asset = asset;
            Object = obj;
            if (playableGraph.IsValid())
            {
                playableGraph.Destroy();
            }
        }

        public static AnimaticSimulate Create()
        {
            AnimaticSimulate simulate = CreateInstance<AnimaticSimulate>();
            simulate.hideFlags = HideFlags.HideAndDontSave;
            simulate.simulateType = SimulateType.None;
            return simulate;
        }
    }
}