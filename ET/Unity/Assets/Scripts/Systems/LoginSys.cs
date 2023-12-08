using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ܣ���½ע��ҵ��ϵͳ
public class LoginSys : MonoBehaviour
{
    public static LoginSys Instance = null;
    public LoginWnd loginWnd;

    public void InitSys()
    {
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
        ResSvc.Instance.AsyncLoadScene(Constants.SceneLogin, () => {
            //��������Ժ��ٴ�ע���¼����
            loginWnd.SetWndState();
        });
        
    }

}
