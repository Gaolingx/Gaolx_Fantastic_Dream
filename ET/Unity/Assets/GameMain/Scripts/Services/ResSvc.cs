//功能：资源加载服务
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;


public class ResSvc : MonoBehaviour
{
    public static ResSvc Instance = null;
    public void InitSvc()
    {
        Instance = this;
        InitRDNameCfg(PathDefine.RDNameCfg);
        InitMapCfg(PathDefine.MapCfg);
        InitGuideCfg(PathDefine.GuideCfg);
        InitStrongCfg(PathDefine.StrongCfg);
        InitBuyCfg(PathDefine.BuyCfg);
        InitTaskRewardCfg(PathDefine.TaskRewardCfg);
        InitNpcCfg(PathDefine.NpcCfg);

        InitSkillCfg(PathDefine.SkillCfg);
        InitSkillMoveCfg(PathDefine.SkillMoveCfg);

        PECommon.Log("Init ResSvc...");
    }

    public void ResetSkillCfgs()
    {
        //清空字典，避免key冲突
        skillDic.Clear();
        InitSkillCfg(PathDefine.SkillCfg);
        skillMoveDic.Clear();
        InitSkillMoveCfg(PathDefine.SkillMoveCfg);

        PECommon.Log("Reset Skill Cfgs Done.");
    }

    private Action prgCB = null;  //更新的回调

    //异步的加载场景，需要显示进度条
    public void AsyncLoadScene(string sceneName, Action loaded)
    {
        GameRoot.Instance.loadingWnd.SetWndState();


        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        prgCB = () =>
        {
            float val = sceneAsync.progress;  //当前异步操作加载的进度
            GameRoot.Instance.loadingWnd.SetProgress(val);

            if (val == 1)
            {
                if (loaded != null)
                {
                    loaded();
                }
                prgCB = null;
                sceneAsync = null;
                GameRoot.Instance.loadingWnd.SetWndState(false);

            }
        };

    }
    private void Update()
    {
        //需要在Update方法中不停地访问val，然后将进度更新
        //将Action作为更新的回调，在Update中不停地调用Action，达到实时更新进度条的目的
        if (prgCB != null)
        {
            //如果判断不为空则调用该方法
            prgCB();
        }
    }

    //定义一个字典，存储当前加载的Audio，与后面的cache有关系
    private Dictionary<string, AudioClip> adDic = new Dictionary<string, AudioClip>();
    public AudioClip LoadAudio(string path, bool iscache = false)
    {
        //音乐加载
        AudioClip au = null;
        if (!adDic.TryGetValue(path, out au))
        {
            au = Resources.Load<AudioClip>(path);
            if (iscache)
            {
                adDic.Add(path, au);
            }
        }
        return au;

    }

    private Dictionary<string, GameObject> goDic = new Dictionary<string, GameObject>();
    //获取Prefab的类
    public GameObject LoadPrefab(string path, bool iscache = false)
    {
        GameObject prefab = null;
        if (!goDic.TryGetValue(path, out prefab))
        {
            //没有缓存则从Resources加载
            prefab = Resources.Load<GameObject>(path);
            if (iscache)
            {
                goDic.Add(path, prefab);
            }
        }

        //prefab加载完成后的实例化
        GameObject go = null;
        if (prefab != null)
        {
            go = Instantiate(prefab);
        }
        return go;
    }

    private Dictionary<string, Sprite> spDic = new Dictionary<string, Sprite>();
    public Sprite LoadSprite(string path, bool cache = false)
    {
        Sprite sp = null;
        if (!spDic.TryGetValue(path, out sp))
        {
            sp = Resources.Load<Sprite>(path);
            if (cache)
            {
                spDic.Add(path, sp);
            }
        }
        return sp;
    }

    #region InitCfgs
    #region 随机名字
    private List<string> surnameLst = new List<string>();
    private List<string> manLst = new List<string>();
    private List<string> womanLst = new List<string>();
    private void InitRDNameCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
    private void InitMapCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
                    ID = ID
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
    private void InitGuideCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
    private void InitStrongCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
    private void InitBuyCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
    private void InitTaskRewardCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
    private void InitNpcCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
    private void InitSkillCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
                    skillMoveLst = new List<int>()
                };

                foreach (XmlElement e in nodLst[i].ChildNodes)
                {
                    switch (e.Name)
                    {
                        case "skillName":
                            sc.skillName = e.InnerText;
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
    private void InitSkillMoveCfg(string path)
    {
        TextAsset xml = Resources.Load<TextAsset>(path);
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
    #endregion
}
