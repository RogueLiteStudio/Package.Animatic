using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Animatic
{
    [FilePath("ProjectSettings/AnimaticAssetPrefabBinder.asset", FilePathAttribute.Location.ProjectFolder)]
    public class AnimaticAssetPrefabBinder : ScriptableSingleton<AnimaticAssetPrefabBinder>
    {
        [System.Serializable]
        struct Binder 
        {
            public AnimaticAsset Asset;
            public GameObject Prefab;
        }
        [SerializeField]
        private List<Binder> binders = new List<Binder>();

        private void OnEnable()
        {
            binders.RemoveAll(it => !it.Prefab || !it.Asset);
        }

        public static void Bind(AnimaticAsset asset, GameObject prefab)
        {
            if (prefab && !PrefabUtility.IsPartOfPrefabAsset(prefab))
                return;
            if (!asset)
                return;
            instance.DoBind(asset, prefab);
            instance.Save(true);
        }

        private void DoBind(AnimaticAsset asset, GameObject prefab)
        {
            for (int i = 0; i < binders.Count; ++i)
            {
                if (binders[i].Asset == asset)
                {
                    if (prefab == null)
                    {
                        binders.RemoveAt(i);
                    }
                    else
                    {
                        binders[i] = new Binder() { Asset = asset, Prefab = prefab };
                    }
                    return;
                }
            }
            if (prefab)
                binders.Add(new Binder() { Asset = asset, Prefab = prefab });
        }

        public static GameObject GetPrefab(AnimaticAsset asset)
        {
            for (int i = 0; i < instance.binders.Count; ++i)
            {
                if (instance.binders[i].Asset == asset)
                {
                    return instance.binders[i].Prefab;
                }
            }
            return null;
        }
    }
}
