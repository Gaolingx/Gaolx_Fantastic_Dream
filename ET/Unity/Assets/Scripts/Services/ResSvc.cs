using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

//���ܣ���Դ���ط���
public class ResSvc : MonoBehaviour
{
    public static ResSvc Instance = null;
    public void InitSvc()
    {
        Instance = this;
        InitRDNameCfg();

        PECommon.Log("Init ResSvc...");
    }

    private Action prgCB = null;  //���µĻص�

    //�첽�ļ��س�������Ҫ��ʾ������
    public void AsyncLoadScene(string sceneName, Action loaded)
    {
        GameRoot.Instance.loadingWnd.SetWndState();
        

        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        prgCB = () => { float val = sceneAsync.progress;  //��ǰ�첽�������صĽ���
            GameRoot.Instance.loadingWnd.SetProgress(val);

            if(val ==1)
            {
                if(loaded != null)
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
        if(prgCB != null)
        {
            //����жϲ�Ϊ������ø÷���
            prgCB();
        }
    }

    //����һ���ֵ䣬�洢��ǰ���ص�Audio��������cache�й�ϵ
    private Dictionary<string, AudioClip> adDic = new Dictionary<string, AudioClip>();
    public AudioClip LoadAudio(string path, bool cache = false)
    {
        //���ּ���
        AudioClip au = null;
        if(!adDic.TryGetValue(path, out au))
        {
            au = Resources.Load<AudioClip>(path);
            if(cache)
            {
                adDic.Add(path, au);
            }
        }
        return au;

    }

    #region InitCfgs
    private List<string> surnameLst = new List<string>();
    private List<string> manLst = new List<string>();
    private List<string> womanLst = new List<string>();
    private void InitRDNameCfg()
    {
        TextAsset xml = Resources.Load<TextAsset>(PathDefine.RDNameCfg);
        if (!xml)
        {
            PECommon.Log("xml file:" + PathDefine.RDNameCfg + " not exist", PELogType.Error);
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
}
