//���ܣ�����������
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCfg : BaseData<MapCfg>
{
    public string mapName; //��ͼ����
    public string sceneName; //��������
    public Vector3 mainCamPos; //���λ��
    public Vector3 mainCamRote; //�����ת
    public Vector3 playerBornPos; //��ҳ���λ��
    public Vector3 playerBornRote;
}


public class BaseData<T>
{
    public int ID;
}

