//���ܣ���Դ���ط���
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

        PECommon.Log("Init ResSvc...");
    }

    private Action prgCB = null;  //���µĻص�

    //�첽�ļ��س�������Ҫ��ʾ������
    public void AsyncLoadScene(string sceneName, Action loaded)
    {
        GameRoot.Instance.loadingWnd.SetWndState();


        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        prgCB = () =>
        {
            float val = sceneAsync.progress;  //��ǰ�첽�������صĽ���
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
        //��Ҫ��Update�����в�ͣ�ط���val��Ȼ�󽫽��ȸ���
        //��Action��Ϊ���µĻص�����Update�в�ͣ�ص���Action���ﵽʵʱ���½�������Ŀ��
        if (prgCB != null)
        {
            //����жϲ�Ϊ������ø÷���
            prgCB();
        }
    }

    //����һ���ֵ䣬�洢��ǰ���ص�Audio��������cache�й�ϵ
    private Dictionary<string, AudioClip> adDic = new Dictionary<string, AudioClip>();
    public AudioClip LoadAudio(string path, bool iscache = false)
    {
        //���ּ���
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
    //��ȡPrefab����
    public GameObject LoadPrefab(string path, bool iscache = false)
    {
        GameObject prefab = null;
        if (!goDic.TryGetValue(path, out prefab))
        {
            //û�л������Resources����
            prefab = Resources.Load<GameObject>(path);
            if (iscache)
            {
                goDic.Add(path, prefab);
            }
        }

        //prefab������ɺ��ʵ����
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
    #region �������
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

    public string GetRDNameData(bool man = true)
    {
        System.Random rd = new System.Random();
        string rdName = surnameLst[PETools.RDInt(0, surnameLst.Count - 1)];
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
    public MapCfg GetMapCfgData(int id)
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
    public AutoGuideCfg GetAutoGuideData(int id)
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
    public StrongCfg GetStrongData(int pos, int starlv)
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

    //��ȡǰ���Ǽ��ۼӵ����� 
    public int GetPropAddValPreLv(int pos, int starlv, int type)
    {
        Dictionary<int, StrongCfg> posDic = null;
        int val = 0;
        if (strongDic.TryGetValue(pos, out posDic))
        {
            for (int i = 0; i < starlv; i++)
            {
                StrongCfg sd;
                if (posDic.TryGetValue(i, out sd))
                {
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
    #endregion
}
