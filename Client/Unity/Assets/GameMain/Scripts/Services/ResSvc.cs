namespace DarkGod.Main
{
//���ܣ���Դ���ط���
using Cysharp.Threading.Tasks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;
using YooAsset;


public class ResSvc : MonoBehaviour
{
    public static ResSvc Instance = null;

    private ResourcePackage _yooAssetResourcePackage;

    public void InitSvc()
    {
        Instance = this;
        _yooAssetResourcePackage = YooAssets.GetPackage(Constants.ResourcePackgeName);

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
        //����ֵ䣬����key��ͻ
        skillDic.Clear();
        InitSkillCfg(PathDefine.SkillCfg);
        skillMoveDic.Clear();
        InitSkillMoveCfg(PathDefine.SkillMoveCfg);
        skillActionDic.Clear();
        InitSkillActionCfg(PathDefine.SkillActionCfg);

        PECommon.Log("Reset Skill Cfgs Done.");
    }

    private Action prgCB = null;  //���µĻص�

    private SceneOperationHandle sceneHandle;
    //�첽�ļ��س�������Ҫ��ʾ������
    public void AsyncLoadScene(string sceneName, Action loaded)
    {
        GameRoot.Instance.loadingWnd.SetWndState();

        StartCoroutine(LoadSceneAsync(sceneName));
        prgCB = () =>
        {
            float val = sceneHandle.Progress;  //��ǰ�첽�������صĽ���
            GameRoot.Instance.loadingWnd.SetProgress(val);

            if (val == 1)
            {
                if (loaded != null)
                {
                    loaded();
                }
                prgCB = null;
                sceneHandle = null;
                GameRoot.Instance.loadingWnd.SetWndState(false);

            }
        };

    }

    IEnumerator LoadSceneAsync(string location)
    {
        var sceneMode = UnityEngine.SceneManagement.LoadSceneMode.Single;
        bool suspendLoad = false;
        sceneHandle = _yooAssetResourcePackage.LoadSceneAsync(location, sceneMode, suspendLoad);
        yield return sceneHandle;
    }
    private void Update()
    {
        //��Ҫ��Update�����в�ͣ�ط���val��Ȼ�󽫽��ȸ���
        //��Action��Ϊ���µĻص�����Update�в�ͣ�ص���Action���ﵽʵʱ���½�������Ŀ��
        if (prgCB != null)
        {
            //����жϲ�Ϊ������ø÷���
            prgCB();
        }
    }


    public async UniTask<AudioClip> LoadAudioClipAsync(string path, bool iscache = false)
    {
        //���ּ���
        AudioClip audioClip = null;
        AssetOperationHandle handle = _yooAssetResourcePackage.LoadAssetAsync<AudioClip>(path);
        await handle.Task;
        audioClip = handle.AssetObject as AudioClip;

        return audioClip;
    }

    //��ȡPrefab����
    public GameObject LoadPrefab(string path, bool iscache = false)
    {
        AssetOperationHandle prefabHandle = null;
        prefabHandle = _yooAssetResourcePackage.LoadAssetSync<GameObject>(path);

        GameObject go = null;
        //prefab������ɺ��ʵ����
        go = prefabHandle.InstantiateSync();

        PECommon.Log("Prefab load. name:" + go.name + ". path:" + path);
        return go;
    }

    public async UniTask<TextAsset> LoadCfgDataAsync(string path)
    {
        TextAsset textAsset = null;
        AssetOperationHandle handle = _yooAssetResourcePackage.LoadAssetAsync<TextAsset>(path);
        await handle.Task;
        textAsset = handle.AssetObject as TextAsset;

        return textAsset;
    }

    public async UniTask<Sprite> LoadSpriteAsync(string path, bool iscache = false)
    {
        Sprite sp = null;
        AssetOperationHandle handle = _yooAssetResourcePackage.LoadAssetAsync<Sprite>(path);
        await handle.Task;
        sp = handle.AssetObject as Sprite;

        return sp;
    }

    #region InitCfgs
    #region �������
    private List<string> surnameLst = new List<string>();
    private List<string> manLst = new List<string>();
    private List<string> womanLst = new List<string>();
    private async void InitRDNameCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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
                //ID��Ϊ�жϵı�׼����ȡ��ǰ���ԣ���ID�����ڣ���ֱ����������ȡ��һ���ڵ��б������
                if (ele.GetAttributeNode("ID") == null)
                {
                    continue;
                }
                int ID = Convert.ToInt32(ele.GetAttributeNode("ID").InnerText);
                //�õ��ڵ����ݺ󣬱����ڵ��ڵ�ÿһ�����ԣ��������Ǳ��浽��Ӧ��List��
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

    #region ��ͼ
    private Dictionary<int, MapCfg> mapCfgDataDic = new Dictionary<int, MapCfg>();
    private async void InitMapCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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

    #region �Զ���������
    private Dictionary<int, AutoGuideCfg> guideTaskDic = new Dictionary<int, AutoGuideCfg>();
    private async void InitGuideCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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

    #region ǿ����������
    private Dictionary<int, Dictionary<int, StrongCfg>> strongDic = new Dictionary<int, Dictionary<int, StrongCfg>>();
    private async void InitStrongCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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
                //�жϵ�ǰ�ڸò�λ���ֵ��Ƿ����
                if (strongDic.TryGetValue(sd.pos, out dic))
                {
                    //�������ֱ�����ֵ�����������
                    dic.Add(sd.startlv, sd);
                }
                else
                {
                    //���û�У�����Ҫ�Ƚ���λ�õ��ֵ�new����
                    dic = new Dictionary<int, StrongCfg>();
                    dic.Add(sd.startlv, sd);

                    //��ӵ�strongDic��
                    strongDic.Add(sd.pos, dic);
                }
            }
        }
    }
    //��ȡ��Ӧλ�ö�Ӧ�Ǽ�������
    public StrongCfg GetStrongCfg(int pos, int starlv)
    {
        StrongCfg sd = null;
        Dictionary<int, StrongCfg> dic = null;
        if (strongDic.TryGetValue(pos, out dic))
        {
            //�ж��ֵ����Ƿ�����Ӧ���Ǽ�
            if (dic.ContainsKey(starlv))
            {
                sd = dic[starlv];
            }
        }
        return sd;
    }

    //��ȡĳ���Ǽ�����ǰ�������Ǽ���ĳ�������ۼӵĺ� 
    public int GetPropAddValPreLv(int pos, int starlv, int type)
    {
        //��ȡ��Ӧλ�����е�ǿ������
        Dictionary<int, StrongCfg> posDic = null;
        int val = 0;
        if (strongDic.TryGetValue(pos, out posDic))
        {
            //�����Ǽ������ͻ�ȡ��Ӧ����
            for (int i = 0; i < starlv; i++)
            {
                StrongCfg sd;
                if (posDic.TryGetValue(i, out sd))
                {
                    //���������ۼ���ֵ
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

    #region ��Դ��������
    private Dictionary<int, BuyCfg> buyCfgDic = new Dictionary<int, BuyCfg>();
    private async void InitBuyCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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

    #region ����������
    private Dictionary<int, TaskRewardCfg> taskRewardDic = new Dictionary<int, TaskRewardCfg>();
    private async void InitTaskRewardCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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

    #region ȫ��NPC����
    private Dictionary<int, NpcData> npcDic = new Dictionary<int, NpcData>();
    private async void InitNpcCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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

    #region ��������
    private Dictionary<int, SkillCfg> skillDic = new Dictionary<int, SkillCfg>();
    private async void InitSkillCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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
                                PECommon.Log("dmgType ERROR", PELogType.Error);
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

    #region ����λ������
    private Dictionary<int, SkillMoveCfg> skillMoveDic = new Dictionary<int, SkillMoveCfg>();
    private async void InitSkillMoveCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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

    #region ����Action����
    private Dictionary<int, SkillActionCfg> skillActionDic = new Dictionary<int, SkillActionCfg>();
    private async void InitSkillActionCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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

    #region ������������
    private Dictionary<int, MonsterCfg> monsterCfgDataDic = new Dictionary<int, MonsterCfg>();
    private async void InitMonsterCfg(string path)
    {
        TextAsset xml = await LoadCfgDataAsync(path);
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