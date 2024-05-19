namespace DarkGod.Main
{
//���ܣ���½ע��ҵ��ϵͳ
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
        //�첽�ļ��ص�¼����
        //����ʾ���صĽ���
        resSvc.AsyncLoadScene(PathDefine.SceneLogin, () => {
            //��������Ժ��ٴ�ע���¼����
            loginWnd.SetWndState();
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
            //��������
            MainCitySys.Instance.EnterMainCity();
        }
        //�رյ�¼����
        loginWnd.SetWndState(false);
    }

    public void RspRename(GameMsg msg)
    {
        GameRoot.Instance.SetPlayerName(msg.rspRename.name);

        //��ת������������
        //�����ǵĽ���
        MainCitySys.Instance.EnterMainCity();

        //�رմ�������
        createWnd.SetWndState(false);
    }
}

}