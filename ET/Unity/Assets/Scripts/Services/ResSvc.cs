using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//功能：资源加载服务
public class ResSvc : MonoBehaviour
{
    public static ResSvc Instance = null;
    public void InitSvc()
    {
        Instance = this;
        Debug.Log("Init ResSvc...");
    }

    private Action prgCB = null;  //更新的回调

    //异步的加载登录场景，需要显示进度条
    public void AsyncLoadScene(string sceneName)
    {
        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);

        
        prgCB = () => { float val = sceneAsync.progress;  //当前异步操作加载的进度
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
        //需要在Update方法中不停地访问val，然后将进度更新
        //将Action作为更新的回调，在Update中不停地调用Action，达到实时更新进度条的目的
        if(prgCB != null)
        {
            //如果判断不为空则调用该方法
            prgCB();
        }
    }
}
