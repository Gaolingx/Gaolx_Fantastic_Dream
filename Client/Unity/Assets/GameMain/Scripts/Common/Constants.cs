//功能：公用的类，用于常量配置
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//注意：业务相关的数据、配置等由于频繁发生变动，为了方便调整，尽量不要直接写死在代码中，而是做成一个配置，在统一的地方进行修改
namespace DarkGod.Main
{
    public enum GameState
    {
        None,
        Login,
        MainCity,
        FBFight
    }

    public enum ChatType
    {
        None,
        ChatWorld,
        ChatGuild,
        ChatFriend
    }

    public enum TextColorCode
    {
        White,
        Red,
        Green,
        Blue,
        Yellow
    }

    //伤害类型
    public enum DamageType
    {
        None,
        AD = 1,//物理伤害
        AP = 2//魔法伤害
    }

    //实体类型
    public enum EntityType
    {
        None,
        Player,
        Monster
    }

    public enum EntityState
    {
        None,
        BatiState, //霸体状态：不可控制（技能不会被中断且不会进入Hit状态），但可受到伤害
        //etc..
    }

    public enum MonsterType
    {
        None,
        Normal = 1,
        Boss = 2
    }

    public enum CtsType
    {
        Nome,
        PlayBGM
    }

    public class Constants
    {
        #region ColorHex Code
        //文字颜色
        public const string ColorWhiteHex = "#FFFFFF";
        public const string ColorRedHex = "#FF0000FF";
        public const string ColorGreenHex = "#00FF00FF";
        public const string ColorBlueHex = "#00B4FFFF";
        public const string ColorYellowHex = "#FFFF00FF";

        #endregion

        // GameRoot Config
        public const string Path_GameRoot_Obj = "/GameRoot";
        public const string Path_Canvas_Obj = "Canvas";

        //Hotfix
        public const string HotfixBuildVersion = "Release_CN_1.1.0";

        //Package
        public const string ResourcePackgeName = "DefaultPackage";

        //Dynamic Wnd
        public const string TipsAniClipName = "TipsShowAni";

        //AutoGuideNPC
        public const int NPCWiseMan = 0;
        public const int NPCGeneral = 1;
        public const int NPCArtisan = 2;
        public const int NPCTrader = 3;


        //场景名称/ID
        public const int MainCityMapID = 10000;
        //public const string SceneMainCity = "SceneMainCity";


        //背景音效名称
        public const string BGLogin = "bgLogin";
        public const string BGMainCity = "bgMainCity";
        public const string BGHuangYe = "bgHuangYe";
        public const string BGBattle01 = "1c19a603d769341c0285eeb548f3f9a0_567248410615622045";

        //登录按钮音效
        public const string UILoginBtn = "uiLoginBtn";

        //常规UI点击音效
        public const string UIClickBtn = "uiClickBtn";
        public const string UIExtenBtn = "uiExtenBtn";
        public const string UIOpenPage = "uiOpenPage";
        public const string FBItemEnter = "fbitem";

        public const string FBLose = "fblose";
        public const string FBLogoEnter = "fbwin";

        //角色相关音效
        public const string Audio_Mar7th00_Hit = "assassin_Hit";

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
        public const float AccelerHPSpeed = 0.3f;

        //PlayerFollowCamera标签
        public const string PlayerFollowCameraWithTag = "PlayerFollowCam";
        //CinemachineVirtualCamera跟随目标的标签
        public const string CinemachineVirtualCameraFollowGameObjectWithTag = "PlayerCameraRoot";
        //CinemachineVirtualCamera裁剪平面
        public const float CinemachineVirtualCameraNearClipPlane = 0.2f;
        public const float CinemachineVirtualCameraFarClipPlane = 15000;

        //角色展示相机配置
        public const string CharShowCamWithTag = "CharShowCam";
        public const float CharShowCamDistanceOffset = 2.8f;
        public const float CharShowCamHeightOffset = 0.8f;

        //玩家信息页模型拖拽速度
        public const float OnDragCharRoateSpeed = 0.4f;

        //玩家标签
        public const string CharPlayerWithTag = "Player";

        //GamePad配置
        public const string Path_PlayerInputs_Obj = "Canvas/PlayerInputs";
        public const string Path_Joysticks_Obj = "Canvas/UI_Canvas_StarterAssetsInputs_Joysticks";

        //自动任务图标配置
        public const int DefaultGuideBtnIconID = -1;

        //MainCityMap配置
        public const string MapRootGameObjectWithTag = "MapRoot";

        //NavMesh配置
        public const float NavNpcDst = 0.5f;
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
        public const float SndMsgWaitForSeconds = 5.0f;

        //购买相关配置
        public const int BuyTypePower = 0;
        public const int MakeTypeCoin = 1;
        public const int BuyCostDiamondOnce = 10;

        //npcID配置
        public const int NpcTypeID_0 = 0;
        public const int NpcTypeID_1 = 1;
        public const int NpcTypeID_2 = 2;
        public const int NpcTypeID_3 = 3;

        //Mar_7th_00 动画参数配置
        public const string AniID_Mar7th00_Blend_Speed = "Speed";
        public const string AniID_Mar7th00_Blend_MotionSpeed = "MotionSpeed";

        //Action触发参数（对应animator）
        public const int ActionDefault = -1;
        public const int ActionBorn = 0;
        public const int ActionDie = 100;
        public const int ActionHit = 101;

        //技能ID配置
        public const int SkillID_Mar7th00_skill01 = 101;
        public const int SkillID_Mar7th00_skill02 = 102;
        public const int SkillID_Mar7th00_skill03 = 103;
        public const int SkillID_Mar7th00_normalAtk01 = 111;

        //连招配置
        //1.普攻连招有效间隔（单位：ms）
        public const int ComboSpace01 = 2500;
        public static int[] comboArr01 = { 111, 112, 113, 114, 115 };

        //Monster配置
        public const int ActiveMonsterDelayTime = 500; //单位：ms
        public const int StateIdleMonsterDelayTime = 1000;
        public const int StateBornMonsterDurationTime = 500;
        public const int StateDieMonsterAnimTime = 5000;

        //AI Logic
        public const float MonsterCheckTime = 2.0f;
        public const float MonsterAtkTime = 2.0f;

        //Collide
        public const int MonsterCollideLayer = 16;
        public const int PlayerCollideLayer = 13;

        //FSM
        public const float HitAniLengthOffset = 0.0f; //这个值不建议在此修改
    }
}
