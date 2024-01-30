//���ܣ�����ҵ��ϵͳ
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

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
        //��ҳ�ʼ��
        //��ȡPrefabʵ�����Ķ���
        GameObject player = resSvc.LoadPrefab(PathDefine.AssissnCityPlayerPrefab, true);
        //��ʼ�����λ��
        player.transform.position = mapData.playerBornPos;
        player.transform.localEulerAngles = mapData.playerBornRote;
        player.transform.localScale = new Vector3(1.0f,1.0f,1.0f);

        //�����ʼ��
        //����Ҫ�������������Ԥ�Ƽ�
        GameObject CM_player = resSvc.LoadPrefab(PathDefine.AssissnCityCharacterCameraPrefab, true);
        //����ʵ��������ʱ���λ�á���ת
        Vector3 CM_player_Pos = mapData.mainCamPos;
        Quaternion CM_player_Rote = Quaternion.Euler(mapData.mainCamRote);
        //ʵ��������
        GameObject CM_player_instance = Instantiate(CM_player,CM_player_Pos,CM_player_Rote);

        // ��ȡ�������Ԥ�Ƽ��ϵ�CinemachineVirtualCamera���  
        CinemachineVirtualCamera cinemachineVirtualCamera = CM_player_instance.GetComponent<CinemachineVirtualCamera>();  
        //���ˣ�����Ӧ�û�ȡ����Ԥ�Ƽ������cinemachineVirtualCamera����Ը�ܻ�ȡ�������
        
        //���ˣ��������ڿ��Զ�Ԥ�Ƽ������ȡ����cinemachineVirtualCamera������в�����...>_<

        // ����CinemachineVirtualCamera�ĸ���Ŀ��Ϊ��ǩΪ"PlayerCamRoot"����Ϸ�����transform
        cinemachineVirtualCamera.Follow = GameObject.FindGameObjectWithTag("PlayerCamRoot").transform;



    }
}
