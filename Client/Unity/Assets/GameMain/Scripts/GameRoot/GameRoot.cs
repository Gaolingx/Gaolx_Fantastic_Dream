//功能：游戏启动入口，初始化各个业务系统

using HuHu;
using PEProtocol;
using UnityEngine;

namespace DarkGod.Main
{
    public class GameRoot : Singleton<GameRoot>
    {
        public bool isDontDestroyOnLoad = true;

        public GameState GameRootGameState { get; set; } = GameState.None;

        protected override void Awake()
        {
            base.Awake();

        }

        private void Start()
        {
            if (isDontDestroyOnLoad)
            {
                //我们不希望GameRoot及其子物体在切换场景时被销毁
                DontDestroyOnLoad(this);
            }

            EventMgr.OnGameEnterEvent.SendEventMessage();

            CleanUIRoot();
            InitGameRoot();
            GameStateEvent.MainInstance.OnGameExit += delegate { OnGameExit(); };

            PECommon.Log("Game Start...");
        }

        private void CleanUIRoot()
        {
            Transform canvas = transform.Find(Constants.Path_Canvas_Obj);

            if (canvas != null)
            {
                for (int i = 0; i < canvas.childCount; i++)
                {
                    canvas.GetChild(i).gameObject.SetActive(false);
                }
            }
        }

        //初始化各个系统和服务模块
        private void InitGameRoot()
        {
            MessageBox.MainInstance.ActiveDynamicWnd();

            //进入登录场景并加载相应UI
            LoginSys.MainInstance.EnterLogin();
        }

        public string GetHotfixVersion()
        {
            return Constants.HotfixBuildVersion;
        }

        private void OnGameExit()
        {
            UIController.Instance.OnClickExit();
        }

        public UIController GetUIController()
        {
            //return GameObject.Find(Constants.UIControllerRootName).GetComponent<UIController>();
            return UIController.Instance;
        }


        public PlayerData PlayerData { get; private set; }

        public void SetPlayerData(RspLogin data)
        {
            PlayerData = data.playerData;
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


        private void OnDisable()
        {

        }
    }
}
