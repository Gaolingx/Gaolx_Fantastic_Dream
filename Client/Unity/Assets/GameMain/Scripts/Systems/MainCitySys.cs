//功能：主城业务系统
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using StarterAssets;
using UnityEngine.AI;
using UnityEngine.InputSystem;
using PEProtocol;
using UnityEngine.InputSystem.XR;
using UnityEngine.InputSystem.UI;

namespace DarkGod.Main
{
    public class MainCitySys : SystemRoot
    {
        public static MainCitySys Instance = null;

        public MainCityWnd maincityWnd;
        public InfoWnd infoWnd;
        public GuideWnd guideWnd;
        public StrongWnd strongWnd;
        public SettingsWnd settingsWnd;
        public ChatWnd chatWnd;
        public BuyWnd buyWnd;
        public TaskWnd taskWnd;
        public Transform CharCamTrans { get { return charCamTrans; } set { charCamTrans = value; } }

        private GameObject mainCityPlayer;
        //private PlayerController playerCtrl;
        private Transform charCamTrans;
        private AutoGuideCfg curtTaskData;
        private Transform[] npcPosTrans;
        private NavMeshAgent nav;
        private StarterAssetsInputs starterAssetsInputs;

        public override void InitSys()
        {
            base.InitSys();

            Instance = this;
            InitPlayerInput();
            PECommon.Log("Init MainCitySys...");
        }

        private void InitPlayerInput()
        {
            starterAssetsInputs = GameRoot.MainInstance.GetStarterAssetsInputs();
        }

        public void EnterMainCity()
        {
            //通过id获取主城配置后，加载场景
            MapCfg mapData = resSvc.GetMapCfg(Constants.MainCityMapID);
            //加载主城场景
            resSvc.AsyncLoadScene(Constants.ResourcePackgeName, mapData.sceneName, () =>
            {
                PECommon.Log("Init MainCitySys...");

                resSvc.UnloadUnusedAssets(Constants.ResourcePackgeName);

                // 加载游戏主角
                LoadPlayer(mapData);

                AssignAnimationIDs();

                // 加载NPC
                LoadNpcPrefab();

                //打开主城场景UI
                maincityWnd.SetWndState();

                // 初始化摇杆插件
                InitGamepad();

                //播放主城背景音乐
                List<string> auLst = new List<string>();
                auLst.Add(Constants.BGMainCity);
                audioSvc.StopBGMusic();
                audioSvc.PlayBGMusics(auLst);

                //获取主城NPCs的Transform
                GetMapNpcTransform();

                //设置人物展示相机
                InitCharCam();

                SetInputState(true);

                //设置游戏状态
                GameRoot.MainInstance.SetGameState(GameState.MainCity);

                PauseGameLogic(false);
            });

        }

        CinemachineVirtualCamera cinemachineVirtualCamera;
        private async void LoadVirtualCameraInstance(string virtualCameraPrefabPath, MapCfg mapData)
        {
            //相机初始化
            //首先要加载虚拟相机的预制件
            //设置实例化对象时候的位置、旋转
            Vector3 CM_player_Pos = mapData.mainCamPos;
            Vector3 CM_player_Rote = mapData.mainCamRote;
            GameObject CM_player = await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, virtualCameraPrefabPath, CM_player_Pos, CM_player_Rote, Vector3.one, false, true, true);

            if (CM_player != null)
            {

                // 获取虚拟相机预制件上的CinemachineVirtualCamera组件  
                cinemachineVirtualCamera = CM_player.GetComponent<CinemachineVirtualCamera>();

                //通过读取配置表设置CinemachineVirtualCamera相裁剪平面
                cinemachineVirtualCamera.m_Lens.FarClipPlane = Constants.CinemachineVirtualCameraFarClipPlane;
                cinemachineVirtualCamera.m_Lens.NearClipPlane = Constants.CinemachineVirtualCameraNearClipPlane;
            }
        }


        private async void LoadPlayerInstance(string playerPrefabPath, MapCfg mapData)
        {
            //玩家初始化
            //获取Prefab实例化的对象
            GameObject player = await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, playerPrefabPath, mapData.playerBornPos, mapData.playerBornRote, new Vector3(0.8f, 0.8f, 0.8f), false, true, true);

            if (player != null)
            {
                //获取player导航组件
                nav = player.GetComponent<NavMeshAgent>();

                ThirdPersonController controller = player.GetComponent<ThirdPersonController>();

                controller.PlayerInput = starterAssetsInputs.gameObject.GetComponent<PlayerInput>();
                controller.StarterAssetsInputs = starterAssetsInputs;

                controller.SetMoveMode(true);
                controller.MoveSpeed = Constants.PlayerMoveSpeed;
                controller.SprintSpeed = Constants.PlayerSprintSpeed;
                controller.playerFollowVirtualCamera = cinemachineVirtualCamera;

                GameRoot.MainInstance.SetAudioListener(player.GetComponent<AudioListener>(), true, false);

                cinemachineVirtualCamera.Follow = player.transform.Find(Constants.CinemachineVirtualCameraFollowGameObjectWithTag);
                mainCityPlayer = player;
            }
        }

        private void LoadPlayer(MapCfg mapData)
        {
            LoadVirtualCameraInstance(PathDefine.AssissnCityCharacterCameraPrefab, mapData);
            LoadPlayerInstance(PathDefine.AssissnCityPlayerPrefab, mapData);
        }

        private void LoadNpcPrefab()
        {
            NpcSvc.MainInstance.LoadMapNpc(Constants.NpcTypeID_0);
            NpcSvc.MainInstance.LoadMapNpc(Constants.NpcTypeID_1);
            NpcSvc.MainInstance.LoadMapNpc(Constants.NpcTypeID_2);
            NpcSvc.MainInstance.LoadMapNpc(Constants.NpcTypeID_3);

        }

        private void InitGamepad()
        {
            Transform GamePadTrans = transform.Find(Constants.Path_Joysticks_MainCitySys);
            if (GamePadTrans != null)
            {
                GamePadTrans.gameObject.SetActive(true);
                UICanvasControllerInput uICanvasControllerInput = GamePadTrans.GetComponent<UICanvasControllerInput>();

                uICanvasControllerInput.starterAssetsInputs = starterAssetsInputs;
            }
        }

        public void PauseGameLogic(bool isPause)
        {
            GameRoot.MainInstance.PauseGameUI(isPause);
        }


        //原方案
        public void SetMoveDir(Vector2 dir)
        {
            StopNavTask();
            /*
            //设置动画
            if (dir == Vector2.zero)
            {
                playerCtrl.SetBlend(Constants.BlendIdle);
            }
            else
            {
                playerCtrl.SetBlend(Constants.BlendWalk);
            }
            //设置方向
            playerCtrl.Dir = dir;
            */
        }

        #region Enter FubenSys
        public void EnterFuben()
        {
            StopNavTask();
            FubenSys.Instance.EnterFuben();
        }
        #endregion

        #region Task Wnd
        public void OpenTaskRewardWnd()
        {
            StopNavTask();
            taskWnd.SetWndState();
        }
        public void RspTakeTaskReward(GameMsg msg)
        {
            RspTakeTaskReward data = msg.rspTakeTaskReward;
            GameRoot.MainInstance.SetPlayerDataByTask(data);

            taskWnd.RefreshUI();
            maincityWnd.RefreshUI();
        }
        public void PshTaskPrgs(GameMsg msg)
        {
            PshTaskPrgs data = msg.pshTaskPrgs;
            GameRoot.MainInstance.SetPlayerDataByTaskPsh(data);

            if (taskWnd.GetWndState())
            {
                taskWnd.RefreshUI();
            }
        }
        #endregion

        #region Buy Wnd
        public void OpenBuyWnd(int type)
        {
            StopNavTask();
            buyWnd.SetBuyType(type);
            buyWnd.SetWndState();
        }
        public void RspBuy(GameMsg msg)
        {
            RspBuy rspBuydata = msg.rspBuy;
            //更新玩家数据到GameRoot中
            GameRoot.MainInstance.SetPlayerDataByBuy(rspBuydata);
            GameRoot.AddTips("购买成功");

            //更新主城界面
            maincityWnd.RefreshUI();
            //关闭购买窗口
            buyWnd.SetWndState(false);

            if (msg.pshTaskPrgs != null)
            {
                GameRoot.MainInstance.SetPlayerDataByTaskPsh(msg.pshTaskPrgs);
                if (taskWnd.GetWndState())
                {
                    taskWnd.RefreshUI();
                }
            }
        }
        #endregion

        #region Power Handle
        public void PshPower(GameMsg msg)
        {
            PshPower data = msg.pshPower;
            GameRoot.MainInstance.SetPlayerDataByPower(data);
            if (maincityWnd.GetWndState())
            {
                maincityWnd.RefreshUI();
            }

        }
        #endregion

        #region Chat Wnd
        public void OpenChatWnd()
        {
            StopNavTask();
            chatWnd.SetWndState();
        }
        public void PshChat(GameMsg msg)
        {
            chatWnd.AddChatMsg(msg.pshChat.name, msg.pshChat.chat);
        }
        #endregion

        #region Strong Wnd
        public void OpenStrongWnd()
        {
            StopNavTask();
            strongWnd.SetWndState();
        }

        public void RspStrong(GameMsg msg)
        {
            //计算升级前的战力
            int zhanliPre = PECommon.GetFightByProps(GameRoot.MainInstance.PlayerData);
            //更新玩家属性数据
            GameRoot.MainInstance.SetPlayerDataByStrong(msg.rspStrong);
            //升级后战力
            int zhanliNow = PECommon.GetFightByProps(GameRoot.MainInstance.PlayerData);
            //升级后的反馈
            GameRoot.AddTips(WindowRoot.GetTextWithHexColor("战力提升 " + (zhanliNow - zhanliPre), TextColorCode.Blue));

            //刷新强化和主城界面
            strongWnd.UpdateUI();
            maincityWnd.RefreshUI();
        }
        #endregion

        #region Info Wnd
        public void OpenInfoWnd()
        {
            StopNavTask();

            //设置人物展示相机相对位置（主角）、旋转
            if (charCamTrans != null)
            {
                charCamTrans.localPosition = mainCityPlayer.transform.position + mainCityPlayer.transform.forward * Constants.CharShowCamDistanceOffset + new Vector3(0, Constants.CharShowCamHeightOffset, 0);
                charCamTrans.localEulerAngles = new Vector3(0, 180 + mainCityPlayer.transform.localEulerAngles.y, 0);
                charCamTrans.localScale = Vector3.one;
                charCamTrans.gameObject.SetActive(true);
                infoWnd.SetWndState();
                SetInputState(false);
            }
        }

        public void CloseInfoWnd()
        {
            if (charCamTrans != null)
            {
                charCamTrans.gameObject.SetActive(false);
                infoWnd.SetWndState(false);
                SetInputState(true);
            }
        }

        private float startRoate = 0;
        public void SetStartRoate()
        {
            startRoate = mainCityPlayer.transform.localEulerAngles.y;
        }

        public void SetPlayerRoate(float roate)
        {
            mainCityPlayer.transform.localEulerAngles = new Vector3(0, startRoate + roate, 0);
        }
        #endregion

        private void GetMapNpcTransform()
        {
            GameObject map = GameObject.FindGameObjectWithTag(Constants.MapRootGameObjectWithTag);
            MainCityMap mainCityMap = map.GetComponent<MainCityMap>();
            npcPosTrans = mainCityMap.NpcPosTrans;
        }

        private void InitCharCam()
        {
            if (charCamTrans == null)
            {
                charCamTrans = GameObject.FindGameObjectWithTag(Constants.CharShowCamWithTag).transform;
                charCamTrans.gameObject.SetActive(false);
            }
            else
            {
                charCamTrans.gameObject.SetActive(false);
            }
        }

        #region Guide Wnd
        private bool isNavGuide = false;
        public void RunTask(AutoGuideCfg agc)
        {
            if (agc != null)
            {
                curtTaskData = agc;
            }

            nav.enabled = true;
            //解析任务数据
            //判断是否需要寻路（找到npc）
            if (curtTaskData.npcID != -1)
            {
                float dis = Vector3.Distance(mainCityPlayer.transform.position, npcPosTrans[agc.npcID].position); //此处的npcID与配置表guide定义的npcID一一对应
                                                                                                                  //判断当前游戏主角与目标npc之间的距离
                if (dis < Constants.NavNpcDst)
                {
                    Debug.Log("已到达目标附近，导航自动取消");
                    //找到目标npc，停止导航
                    isNavGuide = false;
                    nav.isStopped = true;
                    SetPlayerStopInNavTask(mainCityPlayer);
                    nav.enabled = false;
                    SetInputState(true);

                    OpenGuideWnd();
                }
                else
                {
                    Debug.Log("NavMesh导航启动，自动寻路中...");
                    //未找到目标npc，启动导航
                    isNavGuide = true;
                    nav.enabled = true; //激活导航组件
                    nav.speed = Constants.PlayerMoveSpeedNav; //导航速度
                    nav.SetDestination(npcPosTrans[agc.npcID].position); //设置导航目标点
                    SetInputState(false);
                    SetPlayerMoveInNavTask(mainCityPlayer);
                }
            }
            else
            {
                OpenGuideWnd();
            }
        }

        private void Update()
        {
            if (isNavGuide)
            {
                IsArriveNavPos();
                //playerCtrl.SetCam();
            }

            if (starterAssetsInputs != null)
            {
                if (starterAssetsInputs.isPause && GameRoot.MainInstance.GetGameState() == GameState.MainCity)
                {
                    StopNavTask();
                    OpenSettingsWnd();
                }
            }
        }

        private void IsArriveNavPos()
        {
            float dis = Vector3.Distance(mainCityPlayer.transform.position, npcPosTrans[curtTaskData.npcID].position);
            if (dis < Constants.NavNpcDst)
            {
                Debug.Log("已经到达目的地，导航结束！");
                isNavGuide = false;
                nav.isStopped = true;
                SetPlayerStopInNavTask(mainCityPlayer);
                nav.enabled = false;
                SetInputState(true);

                OpenGuideWnd();
            }
        }

        public void StopNavTask()
        {
            if (isNavGuide)
            {
                Debug.Log("因为导航中途执行其他操作，导航中断！");
                isNavGuide = false;

                nav.isStopped = true;
                SetPlayerStopInNavTask(mainCityPlayer);
                nav.enabled = false;
                SetInputState(true);
            }
        }

        private int _animIDSpeed;
        private int _animIDMotionSpeed;
        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash(Constants.AniID_Mar7th00_Blend_Speed);
            _animIDMotionSpeed = Animator.StringToHash(Constants.AniID_Mar7th00_Blend_MotionSpeed);
        }

        private void SetPlayerMoveInNavTask(GameObject gameObject)
        {
            if (gameObject != null)
            {
                gameObject.GetComponent<ThirdPersonController>().isMoveByController = false;
                gameObject.GetComponent<Animator>().SetFloat(_animIDSpeed, 2f);
                gameObject.GetComponent<Animator>().SetFloat(_animIDMotionSpeed, 1f);
            }
        }

        private void SetPlayerStopInNavTask(GameObject gameObject)
        {
            if (gameObject != null)
            {
                gameObject.GetComponent<ThirdPersonController>().isMoveByController = true;
                gameObject.GetComponent<Animator>().SetFloat(_animIDSpeed, 0f);
                gameObject.GetComponent<Animator>().SetFloat(_animIDMotionSpeed, 1f);
            }
        }

        private void OpenGuideWnd()
        {
            guideWnd.SetWndState();
        }

        public AutoGuideCfg GetCurtTaskData()
        {
            return curtTaskData;
        }
        #endregion

        #region Input
        public void SetInputState(bool stateInputAction)
        {
            GameRoot.MainInstance.EnableInputAction(stateInputAction);
        }
        #endregion

        #region Guide Wnd
        public void OpenSettingsWnd()
        {
            PauseGameLogic(true);
            if (settingsWnd.GetWndState() != true)
            {
                audioSvc.PlayUIAudio(Constants.UIClickBtn);
                settingsWnd.SetWndState(true);
            }
        }

        public void RspGuide(GameMsg msg)
        {
            RspGuide data = msg.rspGuide;

            GameRoot.AddTips(WindowRoot.GetTextWithHexColor("任务奖励 金币+" + curtTaskData.coin + "  经验+" + curtTaskData.exp, TextColorCode.Blue));

            //读取任务actionID，进行相应操作
            switch (curtTaskData.actID)
            {
                case Constants.CurtTaskDataActID_0:
                    //与智者对话
                    break;
                case Constants.CurtTaskDataActID_1:
                    //进入副本
                    EnterFuben();
                    break;
                case Constants.CurtTaskDataActID_2:
                    //进入强化界面
                    OpenStrongWnd();
                    break;
                case Constants.CurtTaskDataActID_3:
                    //进入体力购买
                    OpenBuyWnd(Constants.BuyTypePower);
                    break;
                case Constants.CurtTaskDataActID_4:
                    //进入金币铸造
                    OpenBuyWnd(Constants.MakeTypeCoin);
                    break;
                case Constants.CurtTaskDataActID_5:
                    //进入世界聊天
                    OpenChatWnd();
                    break;
            }

            GameRoot.MainInstance.SetPlayerDataByGuide(data);
            maincityWnd.RefreshUI();
        }
        #endregion
    }
}
