//���ܣ�����������
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AutoGuideCfg : BaseData<AutoGuideCfg>
{
    public int npcID; //��������Ŀ��NPC������
    public string dilogArr; //�Ի�����
    public int actID; //Ŀ������ID
    public int coin; //�����Ľ��
    public int exp; //����

}
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

