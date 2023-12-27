using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ܣ���½ע��ҵ��ϵͳ
public class LoginSys : SystemRoot
{
    public static LoginSys Instance = null;
    public LoginWnd loginWnd;
    public CreateWnd createWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init LoginSys...");
    }
    /// <summary>
    /// �����¼����
    /// </summary>
    public void EnterLogin()
    {
        //�첽���ص�¼����
        //�ڼ��صĹ����ж�̬��ʾ���ؽ���
        resSvc.AsyncLoadScene(Constants.SceneLogin, () => {
            //��������Ժ��ٴ�ע���¼����
            loginWnd.SetWndState();
            //���ŵ�¼������Ч
            audioSvc.PlayBGMusic(Constants.BGLogin);

        });
        
    }

    public void RspLogin(GameMsg msg)
    {
        GameRoot.AddTips("��¼�ɹ�");
        GameRoot.Instance.SetPlayerData(msg.rspLogin);

        if (msg.rspLogin.playerData.name == "")
        {
            //�򿪽�ɫ��������
            createWnd.SetWndState();
        }
        else
        {
            //��������TODO
        }
        //�رյ�¼����
        loginWnd.SetWndState(false);
    }
}
