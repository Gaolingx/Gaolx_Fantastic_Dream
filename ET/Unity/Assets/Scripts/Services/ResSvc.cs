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
    //�첽�ļ��ص�¼��������Ҫ��ʾ������
    public void AsyncLoadScene(string sceneName)
    {
        SceneManager.LoadSceneAsync(sceneName);
    }
}
