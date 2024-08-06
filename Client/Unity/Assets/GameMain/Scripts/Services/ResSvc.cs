//功能：资源加载服务
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;
using HuHu;

namespace DarkGod.Main
{
    public class ResSvc : Singleton<ResSvc>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public void InitSvc()
        {
            InitRDNameCfg(PathDefine.RDNameCfg);
            InitMonsterCfg(PathDefine.MonsterCfg);
            InitMapCfg(PathDefine.MapCfg);
            InitGuideCfg(PathDefine.GuideCfg);
            InitStrongCfg(PathDefine.StrongCfg);
            InitBuyCfg(PathDefine.BuyCfg);
            InitTaskRewardCfg(PathDefine.TaskRewardCfg);
            InitNpcCfg(PathDefine.NpcCfg);

            InitSkillCfg(PathDefine.SkillCfg);
            InitSkillMoveCfg(PathDefine.SkillMoveCfg);
            InitSkillActionCfg(PathDefine.SkillActionCfg);

            PECommon.Log("Init ResSvc...");
        }

        public void ResetSkillCfgs()
        {
            //清空字典，避免key冲突
            skillDic.Clear();
            InitSkillCfg(PathDefine.SkillCfg);
            skillMoveDic.Clear();
            InitSkillMoveCfg(PathDefine.SkillMoveCfg);
            skillActionDic.Clear();
            InitSkillActionCfg(PathDefine.SkillActionCfg);

            PECommon.Log("Reset Skill Cfgs Done.");
        }


        #region Resource Load
        private List<AssetHandle> _cacheAssetHandles = new();

        private Dictionary<string, AssetHandle> _audioClipHandleDict = new();
        private Dictionary<string, AssetHandle> _textAssetHandleDict = new();
        private Dictionary<string, AssetHandle> _videoClipHandleDict = new();
        private Dictionary<string, AssetHandle> _spriteHandleDict = new();
        private Dictionary<string, AssetHandle> _prefabHandleDict = new();

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

        //异步的加载场景，需要显示进度条
        public async void AsyncLoadScene(string packageName, string sceneName, Action loaded)
        {
            GameRoot.MainInstance.loadingWnd.SetWndState();
            await LoadSceneAsyncHandle(packageName, sceneName);

            loaded?.Invoke();
            GameRoot.MainInstance.loadingWnd.SetWndState(false);
        }

        private async UniTask LoadSceneAsyncHandle(string packageName, string path, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
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

        public async UniTask<AudioClip> LoadAudioClipAsync(string packageName, string audioClipPath, bool isCache = false, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            AudioClip audioClip = null;
            _audioClipHandleDict.TryGetValue(audioClipPath, out AssetHandle handle);
            if (handle == null)
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetAsync<AudioClip>(audioClipPath);
                await handle.ToUniTask(progress, timing);
                audioClip = handle.AssetObject as AudioClip;
                if (isCache)
                {
                    if (!_audioClipHandleDict.ContainsKey(audioClipPath))
                    {
                        _audioClipHandleDict.Add(audioClipPath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            else
            {
                audioClip = handle.AssetObject as AudioClip;
            }

            return audioClip;
        }

        public AudioClip LoadAudioClipSync(string packageName, string audioClipPath, bool isCache = false)
        {
            AudioClip audioClip = null;
            _audioClipHandleDict.TryGetValue(audioClipPath, out AssetHandle handle);
            if (handle == null)
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetSync<AudioClip>(audioClipPath);
                audioClip = handle.AssetObject as AudioClip;
                if (isCache)
                {
                    if (!_audioClipHandleDict.ContainsKey(audioClipPath))
                    {
                        _audioClipHandleDict.Add(audioClipPath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            else
            {
                audioClip = handle.AssetObject as AudioClip;
            }

            return audioClip;
        }

        public GameObject LoadPrefabSync(string packageName, string prefabPath, bool isCache)
        {
            GameObject prefab = null;
            _prefabHandleDict.TryGetValue(prefabPath, out AssetHandle handle);
            if (handle == null)
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetSync<GameObject>(prefabPath);
                prefab = handle.AssetObject as GameObject;
                if (isCache)
                {
                    if (!_prefabHandleDict.ContainsKey(prefabPath))
                    {
                        _prefabHandleDict.Add(prefabPath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            else
            {
                prefab = handle.AssetObject as GameObject;
            }

            PECommon.Log("Prefab load Sync. name:" + prefab.name + ". path:" + prefabPath);
            return prefab;
        }

        public GameObject LoadGameObjectSync(string packageName, Transform parentTrans, string prefabPath, bool isCache)
        {
            GameObject prefab = LoadPrefabSync(packageName, prefabPath, isCache);
            GameObject instantiatedPrefab = Instantiate(prefab, parentTrans);
            return instantiatedPrefab;
        }

        public async UniTask<GameObject> LoadGameObjectAsync(string packageName, string prefabPath, Vector3 GameObjectPos, Vector3 GameObjectRota, Vector3 GameObjectScal, bool isCache = false, bool isLocalPos = true, bool isLocalEulerAngles = true, Transform transform = null, bool isRename = false, bool isNeedDestroy = true, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            GameObject prefab = await LoadPrefabAsync(packageName, prefabPath, isCache, progress, timing);
            GameObject instantiatedPrefab = null;
            if (isNeedDestroy)
            {
                instantiatedPrefab = Instantiate(prefab);
            }
            else
            {
                instantiatedPrefab = Instantiate(prefab, this.transform);
            }

            GameRoot.MainInstance.SetGameObjectTrans(instantiatedPrefab, GameObjectPos, GameObjectRota, GameObjectScal, isLocalPos, isLocalEulerAngles, true, transform, isRename);


            PECommon.Log("Prefab load Async. name:" + instantiatedPrefab.name + ". path:" + prefabPath + ",isCache:" + isCache);
            return instantiatedPrefab;
        }

        public async UniTask<GameObject> LoadPrefabAsync(string packageName, string prefabPath, bool isCache, IProgress<float> progress, PlayerLoopTiming timing)
        {
            GameObject prefab = null;
            _prefabHandleDict.TryGetValue(prefabPath, out AssetHandle handle);
            if (handle == null)
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetAsync<GameObject>(prefabPath);
                await handle.ToUniTask(progress, timing);
                prefab = handle.AssetObject as GameObject;

                if (isCache)
                {
                    if (!_prefabHandleDict.ContainsKey(prefabPath))
                    {
                        _prefabHandleDict.Add(prefabPath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            else
            {
                prefab = handle.AssetObject as GameObject;
            }
            return prefab;
        }

        public async UniTask<TextAsset> LoadCfgDataAsync(string packageName, string textAssetPath, bool isCache = false, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            TextAsset textAsset = null;
            _textAssetHandleDict.TryGetValue(textAssetPath, out AssetHandle handle);
            if (handle == null)
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetAsync<TextAsset>(textAssetPath);
                await handle.ToUniTask(progress, timing);
                textAsset = handle.AssetObject as TextAsset;
                if (isCache)
                {
                    if (!_textAssetHandleDict.ContainsKey(textAssetPath))
                    {
                        _textAssetHandleDict.Add(textAssetPath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            else
            {
                textAsset = handle.AssetObject as TextAsset;
            }

            return textAsset;
        }

        public async UniTask<Sprite> LoadSpriteAsync(string packageName, string spritePath, bool isCache = false, IProgress<float> progress = null, PlayerLoopTiming timing = PlayerLoopTiming.Update)
        {
            Sprite sprite = null;
            _spriteHandleDict.TryGetValue(spritePath, out AssetHandle handle);
            if (handle == null)
            {
                var package = YooAssets.GetPackage(packageName);
                handle = package.LoadAssetAsync<Sprite>(spritePath);
                await handle.ToUniTask(progress, timing);
                sprite = handle.AssetObject as Sprite;
                if (isCache)
                {
                    if (!_spriteHandleDict.ContainsKey(spritePath))
                    {
                        _spriteHandleDict.Add(spritePath, handle);
                    }
                }
                else
                {
                    GCAssetHandleTODO(handle);
                }
            }
            else
            {
                sprite = handle.AssetObject as Sprite;
            }

            return sprite;
        }

        #endregion

        #region UnloadAsset
        // 卸载所有引用计数为零的资源包。
        // 可以在切换场景之后调用资源释放方法或者写定时器间隔时间去释放。
        public void UnloadUnusedAssets(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            package.UnloadUnusedAssets();
        }

        // 尝试卸载指定的资源对象
        // 注意：如果该资源还在被使用，该方法会无效。
        public void TryUnloadUnusedAsset(string packageName, string path)
        {
            var package = YooAssets.GetPackage(packageName);
            package.TryUnloadUnusedAsset(path);
        }

        // 强制卸载所有资源包，该方法请在合适的时机调用。
        // 注意：Package在销毁的时候也会自动调用该方法。
        public void ForceUnloadAllAssets(string packageName)
        {
            var package = YooAssets.GetPackage(packageName);
            package.ForceUnloadAllAssets();
        }

        #endregion

        #region InitCfgs
        #region 随机名字
        private List<string> surnameLst = new List<string>();
        private List<string> manLst = new List<string>();
        private List<string> womanLst = new List<string>();
        private async void InitRDNameCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;
                    //ID作为判断的标准，获取当前属性，当ID不存在，则直接跳过，读取下一个节点列表的数据
                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    //拿到节点数据后，遍历节点内的每一个属性，并将它们保存到对应的List中
                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "surname":
                                surnameLst.Add(e.InnerText);
                                break;
                            case "man":
                                manLst.Add(e.InnerText);
                                break;
                            case "woman":
                                womanLst.Add(e.InnerText);
                                break;
                        }
                    }
                }
            }

        }

        public string GetRDNameCfg(bool man = true)
        {
            System.Random rd = new System.Random();
            string rdName = surnameLst[PETools.RDInt(0, surnameLst.Count - 1, rd)];
            if (man)
            {
                rdName += manLst[PETools.RDInt(0, manLst.Count - 1)];
            }
            else
            {
                rdName += womanLst[PETools.RDInt(0, womanLst.Count - 1)];
            }

            return rdName;
        }
        #endregion

        #region 地图
        private Dictionary<int, MapCfg> mapCfgDataDic = new Dictionary<int, MapCfg>();
        private async void InitMapCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    MapCfg mapCfg = new MapCfg
                    {
                        ID = ID,
                        monsterLst = new List<MonsterData>()
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "mapName":
                                mapCfg.mapName = e.InnerText;
                                break;
                            case "sceneName":
                                mapCfg.sceneName = e.InnerText;
                                break;
                            case "playerPath":
                                mapCfg.playerPath = e.InnerText;
                                break;
                            case "playerCamPath":
                                mapCfg.playerCamPath = e.InnerText;
                                break;
                            case "power":
                                mapCfg.power = int.Parse(e.InnerText);
                                break;
                            case "mainCamPos":
                                {
                                    string[] valArr = e.InnerText.Split(',');
                                    mapCfg.mainCamPos = new Vector3(float.Parse(valArr[0]), float.Parse(valArr[1]), float.Parse(valArr[2]));
                                }
                                break;
                            case "mainCamRote":
                                {
                                    string[] valArr = e.InnerText.Split(',');
                                    mapCfg.mainCamRote = new Vector3(float.Parse(valArr[0]), float.Parse(valArr[1]), float.Parse(valArr[2]));
                                }
                                break;
                            case "playerBornPos":
                                {
                                    string[] valArr = e.InnerText.Split(',');
                                    mapCfg.playerBornPos = new Vector3(float.Parse(valArr[0]), float.Parse(valArr[1]), float.Parse(valArr[2]));
                                }
                                break;
                            case "playerBornRote":
                                {
                                    string[] valArr = e.InnerText.Split(',');
                                    mapCfg.playerBornRote = new Vector3(float.Parse(valArr[0]), float.Parse(valArr[1]), float.Parse(valArr[2]));
                                }
                                break;
                            case "monsterLst":
                                {
                                    string[] valArr = e.InnerText.Split('#');
                                    for (int waveIndex = 0; waveIndex < valArr.Length; waveIndex++)
                                    {
                                        if (waveIndex == 0)
                                        {
                                            continue;
                                        }
                                        string[] tempArr = valArr[waveIndex].Split('|');
                                        for (int j = 0; j < tempArr.Length; j++)
                                        {
                                            if (j == 0)
                                            {
                                                continue;
                                            }
                                            string[] arr = tempArr[j].Split(',');
                                            MonsterData md = new MonsterData
                                            {
                                                ID = int.Parse(arr[0]),
                                                mWave = waveIndex,
                                                mIndex = j,
                                                mCfg = GetMonsterCfg(int.Parse(arr[0])),
                                                mBornPos = new Vector3(float.Parse(arr[1]), float.Parse(arr[2]), float.Parse(arr[3])),
                                                mBornRote = new Vector3(0, float.Parse(arr[4]), 0),
                                                mLevel = int.Parse(arr[5]),
                                                mMoveSpeed = float.Parse(arr[6])
                                            };
                                            mapCfg.monsterLst.Add(md);
                                        }
                                    }
                                }
                                break;
                            case "coin":
                                mapCfg.coin = int.Parse(e.InnerText);
                                break;
                            case "exp":
                                mapCfg.exp = int.Parse(e.InnerText);
                                break;
                            case "crystal":
                                mapCfg.crystal = int.Parse(e.InnerText);
                                break;
                        }
                    }
                    mapCfgDataDic.Add(ID, mapCfg);
                }
            }
        }
        public MapCfg GetMapCfg(int id)
        {
            MapCfg data;
            if (mapCfgDataDic.TryGetValue(id, out data))
            {
                return data;
            }
            return null;
        }
        #endregion

        #region 自动引导配置
        private Dictionary<int, AutoGuideCfg> guideTaskDic = new Dictionary<int, AutoGuideCfg>();
        private async void InitGuideCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    AutoGuideCfg autoGuideCfg = new AutoGuideCfg
                    {
                        ID = ID
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "npcID":
                                autoGuideCfg.npcID = int.Parse(e.InnerText);
                                break;
                            case "dilogArr":
                                autoGuideCfg.dilogArr = e.InnerText;
                                break;
                            case "actID":
                                autoGuideCfg.actID = int.Parse(e.InnerText);
                                break;
                            case "coin":
                                autoGuideCfg.coin = int.Parse(e.InnerText);
                                break;
                            case "exp":
                                autoGuideCfg.exp = int.Parse(e.InnerText);
                                break;
                        }
                    }
                    guideTaskDic.Add(ID, autoGuideCfg);
                }
            }
        }
        public AutoGuideCfg GetAutoGuideCfg(int id)
        {
            AutoGuideCfg agc = null;
            if (guideTaskDic.TryGetValue(id, out agc))
            {
                return agc;
            }
            return null;
        }
        #endregion

        #region 强化升级配置
        private Dictionary<int, Dictionary<int, StrongCfg>> strongDic = new Dictionary<int, Dictionary<int, StrongCfg>>();
        private async void InitStrongCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    StrongCfg sd = new StrongCfg
                    {
                        ID = ID
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        int val = int.Parse(e.InnerText);
                        switch (e.Name)
                        {
                            case "pos":
                                sd.pos = val;
                                break;
                            case "starlv":
                                sd.startlv = val;
                                break;
                            case "addhp":
                                sd.addhp = val;
                                break;
                            case "addhurt":
                                sd.addhurt = val;
                                break;
                            case "adddef":
                                sd.adddef = val;
                                break;
                            case "minlv":
                                sd.minlv = val;
                                break;
                            case "coin":
                                sd.coin = val;
                                break;
                            case "crystal":
                                sd.crystal = val;
                                break;
                        }
                    }

                    Dictionary<int, StrongCfg> dic = null;
                    //判断当前在该部位的字典是否存在
                    if (strongDic.TryGetValue(sd.pos, out dic))
                    {
                        //如果有则直接往字典增加数据项
                        dic.Add(sd.startlv, sd);
                    }
                    else
                    {
                        //如果没有，则需要先将该位置的字典new出来
                        dic = new Dictionary<int, StrongCfg>();
                        dic.Add(sd.startlv, sd);

                        //添加到strongDic中
                        strongDic.Add(sd.pos, dic);
                    }
                }
            }
        }
        //获取对应位置对应星级的属性
        public StrongCfg GetStrongCfg(int pos, int starlv)
        {
            StrongCfg sd = null;
            Dictionary<int, StrongCfg> dic = null;
            if (strongDic.TryGetValue(pos, out dic))
            {
                //判断字典中是否含有相应的星级
                if (dic.ContainsKey(starlv))
                {
                    sd = dic[starlv];
                }
            }
            return sd;
        }

        //获取某个星级包括前面所有星级在某个属性累加的和 
        public int GetPropAddValPreLv(int pos, int starlv, int type)
        {
            //获取对应位置所有的强化数据
            Dictionary<int, StrongCfg> posDic = null;
            int val = 0;
            if (strongDic.TryGetValue(pos, out posDic))
            {
                //根据星级和类型获取对应属性
                for (int i = 0; i < starlv; i++)
                {
                    StrongCfg sd;
                    if (posDic.TryGetValue(i, out sd))
                    {
                        //根据类型累加数值
                        switch (type)
                        {
                            case 1://hp
                                val += sd.addhp;
                                break;
                            case 2://hurt
                                val += sd.addhurt;
                                break;
                            case 3://def
                                val += sd.adddef;
                                break;
                        }
                    }
                }
            }
            return val;
        }
        #endregion

        #region 资源交易配置
        private Dictionary<int, BuyCfg> buyCfgDic = new Dictionary<int, BuyCfg>();
        private async void InitBuyCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    BuyCfg buyCfg = new BuyCfg
                    {
                        ID = ID
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "buyCostDiamondOnce":
                                buyCfg.buyCostDiamondOnce = int.Parse(e.InnerText);
                                break;
                            case "amountEachPurchase":
                                buyCfg.amountEachPurchase = int.Parse(e.InnerText);
                                break;
                        }
                    }
                    buyCfgDic.Add(ID, buyCfg);
                }
            }
        }
        public BuyCfg GetBuyCfg(int id)
        {
            BuyCfg bc = null;
            if (buyCfgDic.TryGetValue(id, out bc))
            {
                return bc;
            }
            return null;
        }
        #endregion

        #region 任务奖励配置
        private Dictionary<int, TaskRewardCfg> taskRewardDic = new Dictionary<int, TaskRewardCfg>();
        private async void InitTaskRewardCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    TaskRewardCfg trc = new TaskRewardCfg
                    {
                        ID = ID
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "taskName":
                                trc.taskName = e.InnerText;
                                break;
                            case "count":
                                trc.count = int.Parse(e.InnerText);
                                break;
                            case "exp":
                                trc.exp = int.Parse(e.InnerText);
                                break;
                            case "coin":
                                trc.coin = int.Parse(e.InnerText);
                                break;
                        }
                    }
                    taskRewardDic.Add(ID, trc);
                }
            }
        }
        public TaskRewardCfg GetTaskRewardCfg(int id)
        {
            TaskRewardCfg trc = null;
            if (taskRewardDic.TryGetValue(id, out trc))
            {
                return trc;
            }
            return null;
        }
        #endregion

        #region 全局NPC配置
        private Dictionary<int, NpcData> npcDic = new Dictionary<int, NpcData>();
        private async void InitNpcCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    NpcData nd = new NpcData
                    {
                        ID = ID
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "NPC_ResPath":
                                nd.npcResPath = e.InnerText;
                                break;
                            case "NPC_Transform_Position_X":
                                nd.NPC_Transform_Position_X = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Position_Y":
                                nd.NPC_Transform_Position_Y = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Position_Z":
                                nd.NPC_Transform_Position_Z = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Rotation_X":
                                nd.NPC_Transform_Rotation_X = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Rotation_Y":
                                nd.NPC_Transform_Rotation_Y = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Rotation_Z":
                                nd.NPC_Transform_Rotation_Z = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Scale_X":
                                nd.NPC_Transform_Scale_X = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Scale_Y":
                                nd.NPC_Transform_Scale_Y = float.Parse(e.InnerText);
                                break;
                            case "NPC_Transform_Scale_Z":
                                nd.NPC_Transform_Scale_Z = float.Parse(e.InnerText);
                                break;
                        }
                    }
                    npcDic.Add(ID, nd);
                }
            }
        }
        public NpcData GetNpcCfg(int id)
        {
            NpcData nd = null;
            if (npcDic.TryGetValue(id, out nd))
            {
                return nd;
            }
            return null;
        }
        #endregion

        #region 技能配置
        private Dictionary<int, SkillCfg> skillDic = new Dictionary<int, SkillCfg>();
        private async void InitSkillCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    SkillCfg sc = new SkillCfg
                    {
                        ID = ID,
                        skillMoveLst = new List<int>(),
                        skillActionLst = new List<int>(),
                        skillDamageLst = new List<int>()
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "skillName":
                                sc.skillName = e.InnerText;
                                break;
                            case "cdTime":
                                sc.cdTime = int.Parse(e.InnerText);
                                break;
                            case "skillTime":
                                sc.skillTime = int.Parse(e.InnerText);
                                break;
                            case "aniAction":
                                sc.aniAction = int.Parse(e.InnerText);
                                break;
                            case "fx":
                                sc.fx = e.InnerText;
                                break;
                            case "isCombo":
                                sc.isCombo = e.InnerText.Equals("1");
                                break;
                            case "isCollide":
                                sc.isCollide = e.InnerText.Equals("1");
                                break;
                            case "isBreak":
                                sc.isBreak = e.InnerText.Equals("1");
                                break;
                            case "dmgType":
                                if (e.InnerText.Equals("1"))
                                {
                                    sc.dmgType = DamageType.AD;
                                }
                                else if (e.InnerText.Equals("2"))
                                {
                                    sc.dmgType = DamageType.AP;
                                }
                                else
                                {
                                    PECommon.Log("DamageType Not Found !", PELogType.Error);
                                }
                                break;
                            case "skillMoveLst":
                                string[] skMoveArr = e.InnerText.Split('|');
                                for (int j = 0; j < skMoveArr.Length; j++)
                                {
                                    if (skMoveArr[j] != "")
                                    {
                                        sc.skillMoveLst.Add(int.Parse(skMoveArr[j]));
                                    }
                                }
                                break;
                            case "skillActionLst":
                                string[] skActionArr = e.InnerText.Split('|');
                                for (int j = 0; j < skActionArr.Length; j++)
                                {
                                    if (skActionArr[j] != "")
                                    {
                                        sc.skillActionLst.Add(int.Parse(skActionArr[j]));
                                    }
                                }
                                break;
                            case "skillDamageLst":
                                string[] skDamageArr = e.InnerText.Split('|');
                                for (int j = 0; j < skDamageArr.Length; j++)
                                {
                                    if (skDamageArr[j] != "")
                                    {
                                        sc.skillDamageLst.Add(int.Parse(skDamageArr[j]));
                                    }
                                }
                                break;
                        }
                    }
                    skillDic.Add(ID, sc);
                }
            }
        }
        public SkillCfg GetSkillCfg(int id)
        {
            SkillCfg sc = null;
            if (skillDic.TryGetValue(id, out sc))
            {
                return sc;
            }
            return null;
        }
        #endregion

        #region 技能位移配置
        private Dictionary<int, SkillMoveCfg> skillMoveDic = new Dictionary<int, SkillMoveCfg>();
        private async void InitSkillMoveCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    SkillMoveCfg smc = new SkillMoveCfg
                    {
                        ID = ID
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "delayTime":
                                smc.delayTime = int.Parse(e.InnerText);
                                break;
                            case "moveTime":
                                smc.moveTime = int.Parse(e.InnerText);
                                break;
                            case "moveDis":
                                smc.moveDis = float.Parse(e.InnerText);
                                break;
                        }
                    }
                    skillMoveDic.Add(ID, smc);
                }
            }
        }
        public SkillMoveCfg GetSkillMoveCfg(int id)
        {
            SkillMoveCfg smc = null;
            if (skillMoveDic.TryGetValue(id, out smc))
            {
                return smc;
            }
            return null;
        }
        #endregion

        #region 技能Action配置
        private Dictionary<int, SkillActionCfg> skillActionDic = new Dictionary<int, SkillActionCfg>();
        private async void InitSkillActionCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    SkillActionCfg sac = new SkillActionCfg
                    {
                        ID = ID
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "delayTime":
                                sac.delayTime = int.Parse(e.InnerText);
                                break;
                            case "radius":
                                sac.radius = float.Parse(e.InnerText);
                                break;
                            case "angle":
                                sac.angle = int.Parse(e.InnerText);
                                break;
                        }
                    }
                    skillActionDic.Add(ID, sac);
                }
            }
        }
        public SkillActionCfg GetSkillActionCfg(int id)
        {
            SkillActionCfg sac = null;
            if (skillActionDic.TryGetValue(id, out sac))
            {
                return sac;
            }
            return null;
        }
        #endregion

        #region 怪物属性配置
        private Dictionary<int, MonsterCfg> monsterCfgDataDic = new Dictionary<int, MonsterCfg>();
        private async void InitMonsterCfg(string path)
        {
            TextAsset xml = await LoadCfgDataAsync(Constants.ResourcePackgeName, path);
            if (!xml)
            {
                PECommon.Log("xml file:" + path + " not exist", PELogType.Error);
            }
            else
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml.text);

                XmlNodeList nodLst = doc.SelectSingleNode("root").ChildNodes;

                for (int i = 0; i < nodLst.Count; i++)
                {
                    XmlElement ele = nodLst[i] as XmlElement;

                    if (ele.GetAttributeNode("ID") == null)
                    {
                        continue;
                    }
                    int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                    MonsterCfg mc = new MonsterCfg
                    {
                        ID = ID,
                        bps = new BattleProps()
                    };

                    foreach (XmlElement e in nodLst[i].ChildNodes)
                    {
                        switch (e.Name)
                        {
                            case "mName":
                                mc.mName = e.InnerText;
                                break;
                            case "mType":
                                if (e.InnerText.Equals("1"))
                                {
                                    mc.mType = MonsterType.Normal;
                                }
                                else if (e.InnerText.Equals("2"))
                                {
                                    mc.mType = MonsterType.Boss;
                                }
                                else
                                {
                                    PECommon.Log("Monster Type Not Found !", PELogType.Error);
                                }
                                break;
                            case "isStop":
                                mc.isStop = e.InnerText.Equals("1"); //或者 int.Parse(e.InnerText) == 1;
                                break;
                            case "resPath":
                                mc.resPath = e.InnerText;
                                break;
                            case "hp":
                                mc.bps.hp = int.Parse(e.InnerText);
                                break;
                            case "skillID":
                                mc.skillID = int.Parse(e.InnerText);
                                break;
                            case "atkDis":
                                mc.atkDis = float.Parse(e.InnerText);
                                break;
                            case "ad":
                                mc.bps.ad = int.Parse(e.InnerText);
                                break;
                            case "ap":
                                mc.bps.ap = int.Parse(e.InnerText);
                                break;
                            case "addef":
                                mc.bps.addef = int.Parse(e.InnerText);
                                break;
                            case "apdef":
                                mc.bps.apdef = int.Parse(e.InnerText);
                                break;
                            case "dodge":
                                mc.bps.dodge = int.Parse(e.InnerText);
                                break;
                            case "pierce":
                                mc.bps.pierce = int.Parse(e.InnerText);
                                break;
                            case "critical":
                                mc.bps.critical = int.Parse(e.InnerText);
                                break;
                        }
                    }
                    monsterCfgDataDic.Add(ID, mc);
                }
            }
        }
        public MonsterCfg GetMonsterCfg(int id)
        {
            MonsterCfg data;
            if (monsterCfgDataDic.TryGetValue(id, out data))
            {
                return data;
            }
            return null;
        }
        #endregion
        #endregion
    }
}
