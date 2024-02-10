//���ܣ����õ��࣬���ڳ�������
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ע�⣺ҵ����ص����ݡ����õ�����Ƶ�������䶯��Ϊ�˷��������������Ҫֱ��д���ڴ����У���������һ�����ã���ͳһ�ĵط������޸�
public class Constants
{
    //AutoGuideNPC
    public const int NPCWiseMan = 0;
    public const int NPCGeneral = 1;
    public const int NPCArtisan = 2;
    public const int NPCTrader = 3;


    //��������/ID
    public const string SceneLogin = "SceneLogin";
    public const int MainCityMapID = 10000;
    //public const string SceneMainCity = "SceneMainCity";
    

    //������Ч����
    public const string BGLogin = "bgLogin";
    public const string BGMainCity = "bgMainCity";

    //��¼��ť��Ч
    public const string UILoginBtn = "uiLoginBtn";

    //����UI�����Ч
    public const string UIClickBtn = "uiClickBtn";
    public const string UIExtenBtn = "uiExtenBtn";
    public const string UIOpenPage = "uiOpenPage";

    //UI����·������
    public const string openMCmenuAniClipName = "OpenMCMenu";
    public const string closeMCmenuAniClipName = "CloseMCMenu";


    //��Ļ��׼���
    public const int ScreenStandardWidth = 1920;
    public const int ScreenStandardHeight = 1080;

    //ҡ�˵��׼����
    public const int ScreenOPDis = 90;


    //��ϲ���
    public const int BlendIdle = 0;
    public const int BlendWalk = 1;

    //��ɫ�ƶ��ٶ�
    public const float PlayerMoveSpeed = 2.0f;
    //��ɫ�����ٶ�
    public const float PlayerSprintSpeed = 5.335f;
    //�����ƶ��ٶ�
    public const int MonsterMoveSpeed = 4;

    //�˶�ƽ�����ٶ�
    public const float AccelerSpeed = 5;

    //CinemachineVirtualCamera����Ŀ��ı�ǩ
    public const string CinemachineVirtualCameraFollowGameObjectWithTag = "PlayerCamRoot";
    //CinemachineVirtualCamera�ü�ƽ��
    public const float CinemachineVirtualCameraNearClipPlane = 0.2f;
    public const float CinemachineVirtualCameraFarClipPlane = 15000;

    //��ɫչʾ�������
    public const string CharShowCamWithTag = "CharShowCam";
    public const float CharShowCamDistanceOffset = 3.8f;
    public const float CharShowCamHeightOffset = 1.2f;
    
    //�����Ϣҳģ����ק�ٶ�
    public const float OnDragCharRoateSpeed = 0.4f;

    //��ұ�ǩ
    public const string CharPlayerWithTag = "Player";

    //GamePad����
    public const string GamepadBind_StarterAssetsInputs_Joysticks = "UI_Canvas_StarterAssetsInputs_Joysticks";

    //�Զ�����ͼ������
    public const int DefaultGuideBtnIconID = -1;

    //MainCityMap����
    public const string MapRootGameObjectWithTag = "MapRoot";

    //NavMesh����
    public const float NavNpcDst = 2.5f;
    public const float PlayerMoveSpeedNav = 5;

}
