using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ܣ���½ע��ҵ��ϵͳ
public class LoginSys : MonoBehaviour
{
    public void InitSys()
    {
        Debug.Log("Init LoginSys...");
    }
    /// <summary>
    /// �����¼����
    /// </summary>
    public void EnterLogin()
    {
        GameRoot.Instance.loadingWnd.gameObject.SetActive(true);
        GameRoot.Instance.loadingWnd.InitWnd();

        //�첽���ص�¼����
        ResSvc.Instance.AsyncLoadScene(Constants.SceneLogin);
        //�ڼ��صĹ����ж�̬��ʾ���ؽ���
        //��������Ժ��ٴ�ע���¼����
    }


}
