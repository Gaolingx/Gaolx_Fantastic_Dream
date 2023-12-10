using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//���ܣ�ҵ��ϵͳ����
public class SystemRoot : MonoBehaviour
{
    protected ResSvc resSvc;
    protected AudioSvc audioSvc;

    public virtual void InitSys()
    {
        resSvc = ResSvc.Instance;
        audioSvc = AudioSvc.Instance;
    }
}
