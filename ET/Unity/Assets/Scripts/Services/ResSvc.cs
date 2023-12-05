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
    //异步的加载登录场景，需要显示进度条
    public void AsyncLoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
