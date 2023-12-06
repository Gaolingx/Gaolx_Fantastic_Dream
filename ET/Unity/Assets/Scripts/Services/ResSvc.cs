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

    //�첽�ļ��ص�¼��������Ҫ��ʾ������
    public void AsyncLoadScene(string sceneName)
    {
        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);

        
        prgCB = () => { float val = sceneAsync.progress;  //��ǰ�첽�������صĽ���
            GameRoot.Instance.loadingWnd.SetProgress(val);

            if(val ==1)
            {
                prgCB = null;
                sceneAsync = null;
                GameRoot.Instance.loadingWnd.gameObject.SetActive(false);
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
}
