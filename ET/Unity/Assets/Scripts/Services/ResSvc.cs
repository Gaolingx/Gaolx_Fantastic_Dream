using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//���ܣ���Դ���ط���
public class ResSvc : MonoBehaviour
{
    public static ResSvc Instance = null;
    public void InitSvc()
    {
        Instance = this;
        Debug.Log("Init ResSvc...");
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
}
