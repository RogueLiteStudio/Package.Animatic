using UnityEngine.Playables;
using UnityEngine;
namespace Animatic
{
    public class AnimaticSimulate : ScriptableObject
    {
        public enum SimulateType
        {
            None,
            Normal,
            Blend,
        }

        [SerializeField]
        private AnimaticAsset Asset;
        [SerializeField]
        private GameObject Object;
        [SerializeField]
        private PlayableGraph playableGraph;
        [SerializeField]
        private string currentName;
        [SerializeField]
        private SimulateType simulateType;


        public void Evaluate(string motionName, float passTime)
        {
            if (!Asset || !Object)
            {
                return;
            }
            if (simulateType != SimulateType.Normal || motionName != currentName)
            {

            }
        }

        private void OnDestroy()
        {
            Asset = null;
            Object = null;
            playableGraph.Destroy();
        }

        private void BuildNormalSimulate(string motionName)
        {

        }

        public static AnimaticSimulate Create(AnimaticAsset asset, GameObject obj)
        {
            if (obj.GetComponentInChildren<Animator>())
            {
                Debug.LogError($"创建 AnimaticSimulate 失败, {obj.name} 缺少有效的 Animator");
                return null;
            }
            AnimaticSimulate simulate = CreateInstance<AnimaticSimulate>();
            simulate.hideFlags = HideFlags.HideAndDontSave;
            simulate.Asset = asset;
            simulate.Object = obj;
            simulate.simulateType = SimulateType.None;
            return simulate;
        }
    }
}