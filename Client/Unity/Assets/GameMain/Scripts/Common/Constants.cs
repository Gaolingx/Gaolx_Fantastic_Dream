namespace DarkGod.Main
{
//���ܣ����õ��࣬���ڳ�������
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ע�⣺ҵ����ص����ݡ����õ�����Ƶ�������䶯��Ϊ�˷��������������Ҫֱ��д���ڴ����У���������һ�����ã���ͳһ�ĵط������޸�
public enum TextColorCode
{
    White,
    Red,
    Green,
    Blue,
    Yellow
}

//�˺�����
public enum DamageType
{
    None,
    AD = 1,//�����˺�
    AP = 2//ħ���˺�
}

//ʵ������
public enum EntityType
{
    None,
    Player,
    Monster
}

public class Constants
{
    #region ColorHex Code
    //������ɫ
    public const string ColorWhiteHex = "#FFFFFF";
    public const string ColorRedHex = "#FF0000FF";
    public const string ColorGreenHex = "#00FF00FF";
    public const string ColorBlueHex = "#00B4FFFF";
    public const string ColorYellowHex = "#FFFF00FF";
    #endregion

    //Hotfix
    public const string HotfixBuildVersion = "1.0.0";

    //Package
    public const string ResourcePackgeName = "DefaultPackage";

    //Dynamic Wnd
    public const string TipsAniClipName = "TipsShowAni";

    //AutoGuideNPC
    public const int NPCWiseMan = 0;
    public const int NPCGeneral = 1;
    public const int NPCArtisan = 2;
    public const int NPCTrader = 3;


    //��������/ID
    public const int MainCityMapID = 10000;
    //public const string SceneMainCity = "SceneMainCity";


    //������Ч����
    public const string BGLogin = "bgLogin";
    public const string BGMainCity = "bgMainCity";
    public const string BGHuangYe = "bgHuangYe";

    //��¼��ť��Ч
    public const string UILoginBtn = "uiLoginBtn";

    //����UI�����Ч
    public const string UIClickBtn = "uiClickBtn";
    public const string UIExtenBtn = "uiExtenBtn";
    public const string UIOpenPage = "uiOpenPage";
    public const string FBItemEnter = "fbitem";

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
    public const float AccelerHPSpeed = 0.3f;

    //PlayerFollowCamera��ǩ
    public const string PlayerFollowCameraWithTag = "PlayerFollowCam";
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
    public const string Path_PlayerInputs_Obj = "Canvas/PlayerInputs";
    public const string Path_Joysticks_MainCitySys = "Canvas/UI_Canvas_StarterAssetsInputs_Joysticks";
    public const string Path_Joysticks_BattleSys = "Canvas/UI_Canvas_StarterAssetsInputs_Joysticks";

    //�Զ�����ͼ������
    public const int DefaultGuideBtnIconID = -1;

    //MainCityMap����
    public const string MapRootGameObjectWithTag = "MapRoot";

    //NavMesh����
    public const float NavNpcDst = 1.5f;
    public const float PlayerMoveSpeedNav = 2;

    //DialogueWnd����
    public const string CurtTaskData_NpcID_0_Name = "����";
    public const string CurtTaskData_NpcID_1_Name = "����";
    public const string CurtTaskData_NpcID_2_Name = "����";
    public const string CurtTaskData_NpcID_3_Name = "����";
    public const string CurtTaskData_NpcID_Default_Name = "Сܿ";

    //Settings�������
    public const string BGAudioGameObjectName = "BGAudio";
    public const string UIAudioGameObjectName = "UIAudio";

    //actID����
    public const int CurtTaskDataActID_0 = 0;
    public const int CurtTaskDataActID_1 = 1;
    public const int CurtTaskDataActID_2 = 2;
    public const int CurtTaskDataActID_3 = 3;
    public const int CurtTaskDataActID_4 = 4;
    public const int CurtTaskDataActID_5 = 5;

    //�����������
    public const int TextMaxLength = 12;
    public const float SndMsgWaitForSeconds = 5.0f;

    //�����������
    public const int BuyTypePower = 0;
    public const int MakeTypeCoin = 1;
    public const int BuyCostDiamondOnce = 10;

    //npcID����
    public const int NpcTypeID_0 = 0;
    public const int NpcTypeID_1 = 1;
    public const int NpcTypeID_2 = 2;
    public const int NpcTypeID_3 = 3;

    //Battle Mgr����
    public const string MapRootGOTag = "MapRoot";

    //Mar_7th_00 ������������
    public const int State_Mar7th00_Blend_Skill_01 = 2;
    public const int State_Mar7th00_Blend_CantControl = 10;

    //Action������������Ӧanimator��
    public const int ActionDefault = -1;
    public const int ActionBorn = 0;
    public const int ActionDie = 100;
    public const int ActionHit = 101;

    //����ID����
    public const int SkillID_Mar7th00_skill01 = 101;
    public const int SkillID_Mar7th00_skill02 = 102;
    public const int SkillID_Mar7th00_skill03 = 103;
    public const int SkillID_Mar7th00_normalAtk01 = 111;

    //��������
    //1.�չ�������Ч������λ��ms��
    public const int ComboSpace01 = 2500;
    public static int[] comboArr01 = { 111, 112, 113, 114, 115 };

    //EventSystem����
    public const string EventSystemGOName = "EventSystem";

    //Monster����
    public const int ActiveMonsterDelayTime = 500; //��λ��ms
    public const int StateIdleMonsterDelayTime = 1000;
    public const int StateBornMonsterDurationTime = 500;
    public const int StateDieMonsterAnimTime = 5000;

    //AI Logic
    public const float MonsterCheckTime = 2.0f;
    public const float MonsterAtkTime = 2.0f;

    //Collide
    public const int MonsterCollideLayer = 16;
    public const int PlayerCollideLayer = 13;
}

}