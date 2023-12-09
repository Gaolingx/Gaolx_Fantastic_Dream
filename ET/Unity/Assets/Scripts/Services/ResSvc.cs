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

    //异步的加载场景，需要显示进度条
    public void AsyncLoadScene(string sceneName, Action loaded)
    {
        GameRoot.Instance.loadingWnd.SetWndState();
        

        AsyncOperation sceneAsync = SceneManager.LoadSceneAsync(sceneName);
        prgCB = () => { float val = sceneAsync.progress;  //当前异步操作加载的进度
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
        //需要在Update方法中不停地访问val，然后将进度更新
        //将Action作为更新的回调，在Update中不停地调用Action，达到实时更新进度条的目的
        if(prgCB != null)
        {
            //如果判断不为空则调用该方法
            prgCB();
        }
    }

    //定义一个字典，存储当前加载的Audio，与后面的cache有关系
    private Dictionary<string, AudioClip> adDic = new Dictionary<string, AudioClip>();
    public AudioClip LoadAudio(string path, bool cache = false)
    {
        //音乐加载
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
