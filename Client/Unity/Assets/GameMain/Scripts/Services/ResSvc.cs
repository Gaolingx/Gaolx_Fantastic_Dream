//���ܣ���Դ���ط���

using Cysharp.Threading.Tasks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Video;
using YooAsset;
using HuHu;
using System.Threading;

namespace DarkGod.Main
{
    public class ResSvc : Singleton<ResSvc>
    {
        protected override void Awake()
        {
            base.Awake();

            GameRoot.MainInstance.OnGameEnter += InitSvc;
        }

        public void InitSvc()
        {
            PECommon.Log("Init ResSvc...");
        }

        #region Resource Load
        private List<AssetHandle> _cacheAssetHandles = new();

        private Dictionary<string, AssetHandle> _assetHandleDict = new();

        private void GCAssetHandleTODO(AssetHandle assetHandle)
        {
            _cacheAssetHandles.Add(assetHandle);
        }

        public void ReleaseAssetHandles()
        {
            for (int i = 0; i < _cacheAssetHandles.Count; i++)
            {
                _cacheAssetHandles[i].Release();
            }
            _cacheAssetHandles.Clear();
        }

        //�첽�ļ��س�������Ҫ��ʾ������
        public async void AsyncLoadScene(string packageName, string sceneName, System.Action loaded)
        {
            GameRoot.MainInstance.loadingWnd.SetWndState();
            await LoadSceneAsyncHandle(packageName, sceneName);

            loaded?.Invoke();
            GameRoot.MainInstance.loadingWnd.SetWndState(false);
        }

        private async UniTask LoadSceneAsyncHandle(string packageName, string path, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            var sceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
            bool suspendLoad = false;
            var package = YooAssets.GetPackage(packageName);
            var handle = package.LoadSceneAsync(path, sceneMode, suspendLoad);

            while (handle is { IsValid: true, IsDone: false })
            {
                await UniTask.Yield();
                GameRoot.MainInstance.loadingWnd.SetProgress(handle.Progress);
            }
            await handle.ToUniTask(progress, timing);
        }

        public async UniTask<AudioClip> LoadAudioClipAsync(string packageName, string audioClipPath, bool isCache = false, CancellationToken cancellationToken = default, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            AudioClip audioClip = null;
            audioClip = await LoadAssetAsync<AudioClip>(packageName, audioClipPath, isCache, cancellationToken, progress, timing);
            return audioClip;
        }

        public AudioClip LoadAudioClipSync(string packageName, string audioClipPath, bool isCache = false)
        {
            AudioClip audioClip = null;
            audioClip = LoadAssetSync<AudioClip>(packageName, audioClipPath, isCache);
            return audioClip;
        }

        public GameObject LoadGameObjectSync(string packageName, Transform parentTrans, string prefabPath, bool isCache)
        {
            GameObject prefab = LoadAssetSync<GameObject>(packageName, prefabPath, isCache);
            GameObject instantiatedPrefab = Instantiate(prefab, parentTrans);
            return instantiatedPrefab;
        }

        public async UniTask<GameObject> LoadGameObjectAsync(string packageName, string prefabPath, Vector3 GameObjectPos, Vector3 GameObjectRota, Vector3 GameObjectScal, bool isCache = false, bool isLocalPos = true, bool isLocalEulerAngles = true, Transform transform = null, bool isRename = false, bool isNeedDestroy = true, CancellationToken cancellationToken = default, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            GameObject prefab = await LoadAssetAsync<GameObject>(packageName, prefabPath, isCache, cancellationToken, progress, timing);
            GameObject instantiatedPrefab = null;
            if (isNeedDestroy)
            {
                instantiatedPrefab = Instantiate(prefab);
            }
            else
            {
                instantiatedPrefab = Instantiate(prefab, this.transform);
            }

            UIItemUtils.SetGameObjectTrans(instantiatedPrefab, GameObjectPos, GameObjectRota, GameObjectScal, isLocalPos, isLocalEulerAngles, true, transform, isRename);


            PECommon.Log("Prefab load Async. name:" + instantiatedPrefab.name + ". path:" + prefabPath + ",isCache:" + isCache);
            return instantiatedPrefab;
        }

        public async UniTask<TextAsset> LoadCfgDataAsync(string packageName, string textAssetPath, bool isCache = false, CancellationToken cancellationToken = default, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            TextAsset textAsset = null;
            textAsset = await LoadAssetAsync<TextAsset>(packageName, textAssetPath, isCache, cancellationToken, progress, timing);
            return textAsset;
        }

        public async UniTask<Sprite> LoadSpriteAsync(string packageName, string spritePath, bool isCache = false, CancellationToken cancellationToken = default, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            Sprite sprite = null;
            sprite = await LoadAssetAsync<Sprite>(packageName, spritePath, isCache, cancellationToken, progress, timing);
            return sprite;
        }

        public async UniTask<VideoClip> LoadVideoClipASync(string packageName, string videoClipPath, bool isCache = false, CancellationToken cancellationToken = default, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            VideoClip videoClip = null;
            videoClip = await LoadAssetAsync<VideoClip>(packageName, videoClipPath, isCache, cancellationToken, progress, timing);
            return videoClip;
        }

        public T LoadAssetSync<T>(string packageName, string assetPath, bool isCache = false) where T : UnityEngine.Object
        {
            AssetHandle handle;
            if (_assetHandleDict.ContainsKey(assetPath))
            {
                handle = _assetHandleDict[assetPath];
                if (!handle.IsValid)
                {
                    _assetHandleDict.Remove(assetPath);
                    PECommon.Log($"Asset Load Failed:��Asset handle {assetPath} �Ѿ��ͷ�!", PELogType.Warn);
                    return null;
                }
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetSync<T>(assetPath);
                if (isCache)
                {
                    if (!_assetHandleDict.ContainsKey(assetPath))
                    {
                        _assetHandleDict.Add(assetPath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            return handle.AssetObject as T;
        }

        public async UniTask<T> LoadAssetAsync<T>(string packageName, string assetPath, bool isCache = false, CancellationToken cancellationToken = default, System.IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update) where T : UnityEngine.Object
        {
            AssetHandle handle;
            if (_assetHandleDict.ContainsKey(assetPath))
            {
                handle = _assetHandleDict[assetPath];
                await UniTask.WaitUntil(() => handle.IsDone || !handle.IsValid);
                if (!handle.IsValid)
                {
                    _assetHandleDict.Remove(assetPath);
                    PECommon.Log($"Asset Load Failed:��Asset handle {assetPath} �Ѿ��ͷ�!", PELogType.Warn);
                    return null;
                }
            }
            else
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetAsync<T>(assetPath);
                await handle.ToUniTask(progress, timing).AttachExternalCancellation(cancellationToken).SuppressCancellationThrow();
                if (isCache)
                {
                    if (!_assetHandleDict.ContainsKey(assetPath))
                    {
                        _assetHandleDict.Add(assetPath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            return handle.AssetObject as T;
        }

        #endregion

        #region UnloadAsset
        // ж���������ü���Ϊ�����Դ����
        // �������л�����֮�������Դ�ͷŷ�������д��ʱ�����ʱ��ȥ�ͷš�
        public void UnloadUnusedAssets(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            package.UnloadUnusedAssets();
        }

        // ����ж��ָ������Դ����
        // ע�⣺�������Դ���ڱ�ʹ�ã��÷�������Ч��
        public void TryUnloadUnusedAsset(string packageName, string path)
        {
            var package = YooAssets.GetPackage(packageName);
            package.TryUnloadUnusedAsset(path);
        }

        // ǿ��ж��������Դ�����÷������ں��ʵ�ʱ�����á�
        // ע�⣺Package�����ٵ�ʱ��Ҳ���Զ����ø÷�����
        public void ForceUnloadAllAssets(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            package.ForceUnloadAllAssets();
        }

        #endregion

        private void OnDestroy()
        {
            GameRoot.MainInstance.OnGameEnter -= InitSvc;
        }
    }
}
