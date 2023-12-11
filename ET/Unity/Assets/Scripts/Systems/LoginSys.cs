using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ܣ���½ע��ҵ��ϵͳ
public class LoginSys : SystemRoot
{
    public static LoginSys Instance = null;
    public LoginWnd loginWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        Debug.Log("Init LoginSys...");
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
            GameRoot.AddTips("Load Done");
            GameRoot.AddTips("Load Done 2");
        });
        
    }

}
