using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ܣ���Ϸ������ڣ���ʼ������ҵ��ϵͳ
public class GameRoot : MonoBehaviour
{
    private void Start()
    {
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
