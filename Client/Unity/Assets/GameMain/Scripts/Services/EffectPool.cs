using DarkGod.Tools;
using UnityEngine;

namespace DarkGod.Main
{
    public class EffectPool : BasePool<GameObject>
    {
        public EffectPool(GameObject _prefab)
            : base(_prefab, collectionCheck: true)
        {
        }

        public override GameObject OnCreatePoolItem()
        {
            GameObject gameObject = Object.Instantiate(prefab);
            gameObject.name = prefab.name;
            gameObject.SetActive(false);
            return gameObject;
        }

        public override void OnGetPoolItem(GameObject _effect)
        {
            _effect.SetActive(true);
        }

        public override void OnReleasePoolItem(GameObject _effect)
        {
            _effect.transform.parent = null;
            _effect.SetActive(false);
        }

        public override void OnDestroyPoolItem(GameObject _effect)
        {
            if (!_effect.IsNull())
            {
                Object.Destroy(_effect);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
        }
    }
}
