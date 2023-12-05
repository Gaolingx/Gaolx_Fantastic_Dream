using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ܣ���Ϸ������ڣ���ʼ������ҵ��ϵͳ
public class GameRoot : MonoBehaviour
{
    public static GameRoot Instance;

    public LoadingWnd loadingWnd = null; 
    private void Start()
    {
        Instance = this;
        //���ǲ�ϣ��GameRoot�������������л�����ʱ������
        DontDestroyOnLoad(this);
        Debug.Log("Game Start...");

        Init();

    }
    //��ʼ������ϵͳ�ͷ���ģ��
    private void Init()
    {
        //ע����Ҫ�ȳ�ʼ������ģ��
        //����ģ���ʼ��
        ResSvc res = GetComponent<ResSvc>();
        res.InitSvc();

        //ҵ��ϵͳ��ʼ��
        LoginSys login = GetComponent<LoginSys>();
        login.InitSys();

        //�����¼������������ӦUI
        login.EnterLogin();
    }
}
