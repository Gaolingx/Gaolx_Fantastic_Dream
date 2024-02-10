//功能：公用的类，用于常量配置
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//注意：业务相关的数据、配置等由于频繁发生变动，为了方便调整，尽量不要直接写死在代码中，而是做成一个配置，在统一的地方进行修改
public class Constants
{
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
    public const float NavNpcDst = 2.5f;
    public const float PlayerMoveSpeedNav = 5;

}
