//���ܣ�����ҵ��ϵͳ
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainCitySys : SystemRoot
{
    public static MainCitySys Instance = null;

    public MainCityWnd maincityWnd;
    public GameObject PlayerCameraRoot;

    public override void InitSys()
    {
        base.InitSys();

        Instance = this;
        PECommon.Log("Init MainCitySys...");
    }

    public void EnterMainCity()
    {
        //ͨ��id��ȡ�������ú󣬼��س���
        MapCfg mapData = resSvc.GetMapCfgData(Constants.MainCityMapID);
        //�������ǳ���
        resSvc.AsyncLoadScene(mapData.sceneName, () =>
        {
            PECommon.Log("Init MainCitySys...");

            // ������Ϸ����
            LoadPlayer(mapData);

            //�����ǳ���UI
            maincityWnd.SetWndState();

            //�������Ǳ�������
            audioSvc.PlayBGMusic(Constants.BGMainCity);

            //TODO ��������չʾ���

        });
    }

    private void LoadPlayer(MapCfg mapData)
    {
        //��ȡPrefabʵ�����Ķ���
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnCityPlayerPrefab, true);
        //��ʼ�����λ��
        player.transform.position = mapData.playerBornPos;
        player.transform.localEulerAngles = mapData.playerBornRote;
        player.transform.localScale = new Vector3(1.5f,1.5f,1.5f);

        //�����ʼ��
        //��ȡ������PlayerCameraRoot���������
        PlayerCameraRoot = GameObject.Find("PlayerCameraRoot");
        PlayerCameraRoot.transform.position = mapData.mainCamPos;
        PlayerCameraRoot.transform.localEulerAngles = mapData.mainCamRote;

        
    }
}
