//功能：游戏启动入口，初始化各个业务系统
using PEProtocol;
using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using HuHu;

namespace DarkGod.Main
{
    public class GameRoot : Singleton<GameRoot>
    {
        public bool isDontDestroyOnLoad = true;

        public LoadingWnd loadingWnd;
        public DynamicWnd dynamicWnd;

        private StarterAssetsInputs starterAssetsInputs;

        BindableProperty<bool> pauseState = new BindableProperty<bool>();

        protected override void Awake()
        {
            base.Awake();
        }

        private void Start()
        {
            AddBindablePropertyData();

            if (isDontDestroyOnLoad)
            {
                //我们不希望GameRoot及其子物体在切换场景时被销毁
                DontDestroyOnLoad(this);
            }

            PECommon.Log("Game Start...");

            CleanUIRoot();

            InitGameRoot();
        }

        public void AddBindablePropertyData()
        {
            pauseState.OnValueChanged += OnUpdatePauseState;
        }

        public void RmvBindablePropertyData()
        {
            pauseState.OnValueChanged -= OnUpdatePauseState;
        }

        private void OnUpdatePauseState(bool state)
        {
            GetUIController()._isPause = state;

            if (starterAssetsInputs != null)
            {
                starterAssetsInputs.isPause = state;
            }

            if (state == true)
            {
                VFXManager.MainInstance.PauseVFX();
            }
            else
            {
                VFXManager.MainInstance.ResetVXF();
            }
        }

        private void CleanUIRoot()
        {
            Transform canvas = transform.Find("Canvas");
            for (int i = 0; i < canvas.childCount; i++)
            {
                canvas.GetChild(i).gameObject.SetActive(false);
            }
        }

        private void InitStarterAssetsInputs()
        {
            GameObject go = transform.Find(Constants.Path_PlayerInputs_Obj).gameObject;
            go.SetActive(true);
            starterAssetsInputs = go.GetComponent<StarterAssetsInputs>();
        }

        //初始化各个系统和服务模块
        private void InitGameRoot()
        {
            InitStarterAssetsInputs();

            //注：需要先初始化服务模块
            //服务模块初始化
            NetSvc net = GetComponent<NetSvc>();
            net.InitSvc();
            ResSvc res = GetComponent<ResSvc>();
            res.InitSvc();
            AudioSvc audio = GetComponent<AudioSvc>();
            audio.InitSvc();
            NpcSvc npcSvc = GetComponent<NpcSvc>();
            npcSvc.InitCfg();
            TimerSvc timer = GetComponent<TimerSvc>();
            timer.InitSvc();

            VFXManager fXManager = GetComponent<VFXManager>();
            fXManager.InitFX();


            //业务系统初始化
            LoginSys loginSys = GetComponent<LoginSys>();
            loginSys.InitSys();
            MainCitySys maincitySys = GetComponent<MainCitySys>();
            maincitySys.InitSys();
            FubenSys fubenSys = GetComponent<FubenSys>();
            fubenSys.InitSys();
            BattleSys battleSys = GetComponent<BattleSys>();
            battleSys.InitSys();

            dynamicWnd.SetWndState();
            //进入登录场景并加载相应UI
            loginSys.EnterLogin();

        }

        public string GetHotfixVersion()
        {
            return Constants.HotfixBuildVersion;
        }

        public StarterAssetsInputs GetStarterAssetsInputs()
        {
            return starterAssetsInputs;
        }

        public void EnableInputAction(bool state)
        {
            GetUIController()._isInputEnable = state;
        }

        public void PauseGameUI(bool state = true)
        {
            pauseState.Value = state;
        }

        public void ExitGame()
        {
            RmvBindablePropertyData();
            GetUIController().OnClickExit();
        }

        private void Update()
        {
            UIController uiController = GetUIController();
            if (uiController != null)
            {
                if (uiController._isInputEnable && !uiController._isPause && !uiController._isPressingAlt)
                {
                    starterAssetsInputs.canLook = true;
                }
                else
                {
                    starterAssetsInputs.canLook = false;
                }
            }
        }

        public UIController GetUIController()
        {
            return GameObject.Find(Constants.UIControllerGOName).GetComponent<UIController>();
        }

        public void SetVsyncState(bool state)
        {
            if (state == true)
            {
                GetUIController().VSyncSettings = 1;
            }
            else
            {
                GetUIController().VSyncSettings = 0;
            }
        }

        public static void AddTips(string tips)
        {
            GameRoot.MainInstance.dynamicWnd.AddTips(tips);
        }

        private PlayerData _playerData = null;
        public PlayerData PlayerData { get { return _playerData; } }
        public void SetPlayerData(RspLogin data)
        {
            _playerData = data.playerData;
        }

        public void SetPlayerName(string name)
        {
            PlayerData.name = name;
        }

        public void SetPlayerDataByGuide(RspGuide data)
        {
            PlayerData.coin = data.coin;
            PlayerData.lv = data.lv;
            PlayerData.exp = data.exp;
            PlayerData.guideid = data.guideid;
        }

        public void SetPlayerDataByStrong(RspStrong data)
        {
            PlayerData.coin = data.coin;
            PlayerData.crystal = data.crystal;
            PlayerData.hp = data.hp;
            PlayerData.ad = data.ad;
            PlayerData.ap = data.ap;
            PlayerData.addef = data.addef;
            PlayerData.apdef = data.apdef;

            PlayerData.strongArr = data.strongArr;
        }

        public void SetPlayerDataByBuy(RspBuy data)
        {
            PlayerData.diamond = data.diamond;
            PlayerData.coin = data.coin;
            PlayerData.power = data.power;
        }

        public void SetPlayerDataByPower(PshPower data)
        {
            PlayerData.power = data.power;
        }

        public Transform SetGameObjectTrans(GameObject obj, Vector3 GameObjectPos, Vector3 GameObjectRota, Vector3 GameObjectScal, bool isLocalPos = true, bool isLocalEulerAngles = true, bool isSetParent = false, Transform rootObjTrans = null, bool isReplaceName = false)
        {
            if (isReplaceName)
            {
                obj.name = obj.name.Replace("(Clone)", "");
            }

            if (isSetParent)
            {
                obj.transform.SetParent(rootObjTrans);
            }

            if (isLocalPos)
            {
                obj.transform.localPosition = GameObjectPos;
            }
            else
            {
                obj.transform.position = GameObjectPos;
            }

            if (isLocalEulerAngles)
            {
                obj.transform.localEulerAngles = GameObjectRota;
            }
            else
            {
                obj.transform.eulerAngles = GameObjectRota;
            }

            obj.transform.localScale = GameObjectScal;

            Transform GOTrans = obj.transform;
            return GOTrans;
        }

        public void SetPlayerDataByTask(RspTakeTaskReward data)
        {
            PlayerData.coin = data.coin;
            PlayerData.lv = data.lv;
            PlayerData.exp = data.exp;
            PlayerData.taskArr = data.taskArr;
        }

        public void SetPlayerDataByTaskPsh(PshTaskPrgs data)
        {
            PlayerData.taskArr = data.taskArr;
        }

        public void SetPlayerDataByFBStart(RspFBFight data)
        {
            PlayerData.power = data.power;
        }

        public void SetPlayerDataByFBEnd(RspFBFightEnd data)
        {
            PlayerData.coin = data.coin;
            PlayerData.lv = data.lv;
            PlayerData.exp = data.exp;
            PlayerData.crystal = data.crystal;
            PlayerData.fuben = data.fuben;
        }

        private GameState gameState = GameState.None;
        public void SetGameState(GameState state)
        {
            gameState = state;
        }
        public GameState GetGameState()
        {
            return gameState;
        }

        public void SetAudioListener(AudioListener playerAudioListener, bool statePlayer, bool stateGameRoot)
        {
            Transform gameRoot = transform.Find("/GameRoot");
            if (gameRoot != null)
            {
                gameRoot.gameObject.GetComponent<AudioListener>().enabled = stateGameRoot;
            }
            if (playerAudioListener != null)
            {
                playerAudioListener.enabled = statePlayer;
            }
        }

    }
}
