//功能：公用的类，用于常量配置
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//注意：业务相关的数据、配置等由于频繁发生变动，为了方便调整，尽量不要直接写死在代码中，而是做成一个配置，在统一的地方进行修改
public enum TxtColor
{
    Red,
    Green,
    Blue,
    Yellow
}


public class Constants
{
    //文字颜色
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


    //场景名称/ID
    public const string SceneLogin = "SceneLogin";
    public const int MainCityMapID = 10000;
    //public const string SceneMainCity = "SceneMainCity";
    

    //背景音效名称
    public const string BGLogin = "bgLogin";
    public const string BGMainCity = "bgMainCity";

    //登录按钮音效
    public const string UILoginBtn = "uiLoginBtn";

    //常规UI点击音效
    public const string UIClickBtn = "uiClickBtn";
    public const string UIExtenBtn = "uiExtenBtn";
    public const string UIOpenPage = "uiOpenPage";
    public const string FBItemEnter = "fbitem";

    //UI动画路径配置
    public const string openMCmenuAniClipName = "OpenMCMenu";
    public const string closeMCmenuAniClipName = "CloseMCMenu";


    //屏幕标准宽高
    public const int ScreenStandardWidth = 1920;
    public const int ScreenStandardHeight = 1080;

    //摇杆点标准距离
    public const int ScreenOPDis = 90;


    //混合参数
    public const int BlendIdle = 0;
    public const int BlendWalk = 1;

    //角色移动速度
    public const float PlayerMoveSpeed = 2.0f;
    //角色奔跑速度
    public const float PlayerSprintSpeed = 5.335f;
    //怪物移动速度
    public const int MonsterMoveSpeed = 4;

    //运动平滑加速度
    public const float AccelerSpeed = 5;

    //PlayerFollowCamera标签
    public const string PlayerFollowCameraWithTag = "PlayerFollowCam";
    //CinemachineVirtualCamera跟随目标的标签
    public const string CinemachineVirtualCameraFollowGameObjectWithTag = "PlayerCamRoot";
    //CinemachineVirtualCamera裁剪平面
    public const float CinemachineVirtualCameraNearClipPlane = 0.2f;
    public const float CinemachineVirtualCameraFarClipPlane = 15000;

    //角色展示相机配置
    public const string CharShowCamWithTag = "CharShowCam";
    public const float CharShowCamDistanceOffset = 3.8f;
    public const float CharShowCamHeightOffset = 1.2f;
    
    //玩家信息页模型拖拽速度
    public const float OnDragCharRoateSpeed = 0.4f;

    //玩家标签
    public const string CharPlayerWithTag = "Player";

    //GamePad配置
    public const string GamepadBind_StarterAssetsInputs_Joysticks = "UI_Canvas_StarterAssetsInputs_Joysticks";

    //自动任务图标配置
    public const int DefaultGuideBtnIconID = -1;

    //MainCityMap配置
    public const string MapRootGameObjectWithTag = "MapRoot";

    //NavMesh配置
    public const float NavNpcDst = 1.5f;
    public const float PlayerMoveSpeedNav = 2;

    //DialogueWnd配置
    public const string CurtTaskData_NpcID_0_Name = "智者";
    public const string CurtTaskData_NpcID_1_Name = "将军";
    public const string CurtTaskData_NpcID_2_Name = "工匠";
    public const string CurtTaskData_NpcID_3_Name = "商人";
    public const string CurtTaskData_NpcID_Default_Name = "小芸";

    //Settings面板配置
    public const string BGAudioGameObjectName = "BGAudio";
    public const string UIAudioGameObjectName = "UIAudio";

    //actID配置
    public const int CurtTaskDataActID_0 = 0;
    public const int CurtTaskDataActID_1 = 1;
    public const int CurtTaskDataActID_2 = 2;
    public const int CurtTaskDataActID_3 = 3;
    public const int CurtTaskDataActID_4 = 4;
    public const int CurtTaskDataActID_5 = 5;

    //聊天界面配置
    public const int TextMaxLength = 12;

}
