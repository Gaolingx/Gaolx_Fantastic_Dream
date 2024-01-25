//功能：公用的类，用于常量配置
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Constants
{
    //场景名称
    public const string SceneLogin = "SceneLogin";
    public const string SceneMainCity = "SceneMainCity";

    //背景音效名称
    public const string BGLogin = "bgLogin";
    public const string BGMainCity = "bgMainCity";

    //登录按钮音效
    public const string UILoginBtn = "uiLoginBtn";

    //常规UI点击音效
    public const string UIClickBtn = "uiClickBtn";
    public const string UIExtenBtn = "uiExtenBtn";


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

}
