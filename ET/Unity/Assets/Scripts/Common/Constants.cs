//功能：公用的类，用于常量配置
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Constants
{
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
    public const int PlayerMoveSpeed = 8;
    //怪物移动速度
    public const int MonsterMoveSpeed = 4;

    //运动平滑加速度
    public const float AccelerSpeed = 5;

    //CinemachineVirtualCamera跟随目标的标签
    public const string CinemachineVirtualCameraFollowGameObjectWithTag = "PlayerCamRoot";
    //CinemachineVirtualCamera裁剪平面
    public const float CinemachineVirtualCameraNearClipPlane = 0.2f;
    public const float CinemachineVirtualCameraFarClipPlane = 15000;

    //GamePad配置
    public const string GamePadBind_Player = "Player_Avatar_March_7th Variant(Clone)";
    public const string GamepadBind_StarterAssetsInputs_Joysticks = "UI_Canvas_StarterAssetsInputs_Joysticks";

}
