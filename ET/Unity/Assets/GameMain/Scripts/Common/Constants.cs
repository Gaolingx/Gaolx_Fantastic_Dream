//���ܣ����õ��࣬���ڳ�������
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//ע�⣺ҵ����ص����ݡ����õ�����Ƶ�������䶯��Ϊ�˷��������������Ҫֱ��д���ڴ����У���������һ�����ã���ͳһ�ĵط������޸�
public enum TxtColor
{
    Red,
    Green,
    Blue,
    Yellow
}


public class Constants
{
    //������ɫ
    private const string ColorRed = "<color=#FF0000FF>";
    private const string ColorGreen = "<color=#00FF00FF>";
    private const string ColorBlue = "<color=#00B4FFFF>";
    private const string ColorYellow = "<color=#FFFF00FF>";
    private const string ColorEnd = "</color>";

    public static string txtColor(string str,TxtColor color)
    {
        string result = "";
        switch(color)
        {
            case TxtColor.Red:
                result = ColorRed + str + ColorEnd;
                break;
            case TxtColor.Green:
                result = ColorGreen + str + ColorEnd;
                break;
            case TxtColor.Blue:
                result = ColorBlue + str + ColorEnd;
                break;
            case TxtColor.Yellow:
                result = ColorYellow + str + ColorEnd;
                break;
        }
        return result;
    }


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
    public const string GamepadBind_StarterAssetsInputs_Joysticks = "UI_Canvas_StarterAssetsInputs_Joysticks";

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

}
