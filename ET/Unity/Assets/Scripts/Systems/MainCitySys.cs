//���ܣ�����ҵ��ϵͳ
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init MainCitySys...");
    }

    public void EnterMainCity()
    {
        //�������ǳ���
        resSvc.AsyncLoadScene(Constants.SceneMainCity, () =>
        {
            PECommon.Log("Init MainCitySys...");

            //TODO ������Ϸ����

            //�����ǳ���UI
            maincityWnd.SetWndState();

            //�������Ǳ�������
            audioSvc.PlayBGMusic(Constants.BGMainCity);

            //TODO ��������չʾ���

        });
    }
}
