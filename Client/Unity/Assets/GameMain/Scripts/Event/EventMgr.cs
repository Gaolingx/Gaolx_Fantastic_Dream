// 功能：事件管理器

using HuHu;
using PEProtocol;
using UniFramework.Event;
using static DarkGod.Main.QualitySvc;

namespace DarkGod.Main
{
    public class EventMgr : Singleton<EventMgr>
    {
        private readonly EventGroup _eventGroup = new EventGroup();

        protected override void Awake()
        {
            base.Awake();

            InitMgr();
        }

        private void OnEnable()
        {
            AddListener();
        }

        public void InitMgr()
        {
            UniEvent.Initalize();

            PECommon.Log("Init EventMgr...");
        }

        private void AddListener()
        {
            _eventGroup.AddListener<OnGameEnterEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnGameExitEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnGamePauseEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnShowMessageBoxEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnQualityLevelEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnSoundVolumeChangedEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnLoginInfoChangedEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnEntityPlayerChangedEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnReceiveNetworkMessageEvent>(OnHandleEventMessage);
        }

        private void RemoveListener()
        {
            _eventGroup.RemoveAllListener();
        }

        #region Event Message
        public class OnGameEnterEvent : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new OnGameEnterEvent();
                UniEvent.SendMessage(msg);
            }
        }

        public class OnGamePauseEvent : IEventMessage
        {
            public bool data = false;
            public static void SendEventMessage(bool state)
            {
                var msg = new OnGamePauseEvent();
                msg.data = state;
                UniEvent.SendMessage(msg);
            }
        }

        public class OnGameExitEvent : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new OnGameExitEvent();
                UniEvent.SendMessage(msg);
            }
        }

        public class OnShowMessageBoxEvent : IEventMessage
        {
            public string data;
            public static void SendEventMessage(string message)
            {
                var msg = new OnShowMessageBoxEvent();
                msg.data = message;
                UniEvent.SendMessage(msg);
            }
        }

        public class OnQualityLevelEvent : IEventMessage
        {
            public PlayerPrefsData data;
            public static void SendEventMessage(PlayerPrefsData data)
            {
                var msg = new OnQualityLevelEvent();
                msg.data = data;
                UniEvent.SendMessage(msg);
            }
        }

        public class OnSoundVolumeChangedEvent : IEventMessage
        {
            public PlayerPrefsData2 data;
            public static void SendEventMessage(PlayerPrefsData2 data)
            {
                var msg = new OnSoundVolumeChangedEvent();
                msg.data = data;
                UniEvent.SendMessage(msg);
            }
        }

        public class OnLoginInfoChangedEvent : IEventMessage
        {
            public PlayerPrefsData3 data;
            public static void SendEventMessage(PlayerPrefsData3 data)
            {
                var msg = new OnLoginInfoChangedEvent();
                msg.data = data;
                UniEvent.SendMessage(msg);
            }
        }

        public class OnEntityPlayerChangedEvent : IEventMessage
        {
            public EntityPlayer data;
            public static void SendEventMessage(EntityPlayer entity)
            {
                var msg = new OnEntityPlayerChangedEvent();
                msg.data = entity;
                UniEvent.SendMessage(msg);
            }
        }

        public class OnReceiveNetworkMessageEvent : IEventMessage
        {
            public GameMsg data;
            public static void SendEventMessage(GameMsg msg)
            {
                var eventMsg = new OnReceiveNetworkMessageEvent();
                eventMsg.data = msg;
                UniEvent.SendMessage(eventMsg);
            }
        }

        #endregion

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is OnGameEnterEvent)
            {
                GameStateEvent.MainInstance.OnGameEnter?.Invoke();
            }
            else if (message is OnGameExitEvent)
            {
                GameStateEvent.MainInstance.OnGameExit?.Invoke();
            }
            else if (message is OnGamePauseEvent)
            {
                OnGamePauseEvent events = message as OnGamePauseEvent;
                GameStateEvent.MainInstance.OnGamePause?.Invoke(events.data);
            }
            else if (message is OnShowMessageBoxEvent)
            {
                OnShowMessageBoxEvent events = message as OnShowMessageBoxEvent;
                MessageBox.MainInstance.ShowMessage(events.data);
            }
            else if (message is OnQualityLevelEvent)
            {
                OnQualityLevelEvent events = message as OnQualityLevelEvent;
                QualitySvc.MainInstance.SavePlayerData(events.data);
            }
            else if (message is OnSoundVolumeChangedEvent)
            {
                OnSoundVolumeChangedEvent events = message as OnSoundVolumeChangedEvent;
                QualitySvc.MainInstance.SavePlayerData2(events.data);
            }
            else if (message is OnLoginInfoChangedEvent)
            {
                OnLoginInfoChangedEvent events = message as OnLoginInfoChangedEvent;
                QualitySvc.MainInstance.SavePlayerData3(events.data);
            }
            else if (message is OnEntityPlayerChangedEvent)
            {
                OnEntityPlayerChangedEvent events = message as OnEntityPlayerChangedEvent;
                GameStateEvent.MainInstance.CurrentEPlayer.Value = events.data;
            }
            else if (message is OnReceiveNetworkMessageEvent)
            {
                OnReceiveNetworkMessageEvent events = message as OnReceiveNetworkMessageEvent;
                NetMsgHandler(events.data);
            }
            else
            {
                PECommon.Log($"Error:EventMessage Not Found. MessageType:{message.GetType().Name}", PELogType.Error);
            }
        }

        private void NetMsgHandler(GameMsg msg)
        {
            switch ((CMD)msg.cmd)
            {
                case CMD.RspLogin:
                    LoginSys.MainInstance.RspLogin(msg);
                    break;
                case CMD.RspRename:
                    LoginSys.MainInstance.RspRename(msg);
                    break;
                case CMD.RspGuide:
                    MainCitySys.MainInstance.RspGuide(msg);
                    break;
                case CMD.RspStrong:
                    MainCitySys.MainInstance.RspStrong(msg);
                    break;
                case CMD.PshChat:
                    MainCitySys.MainInstance.PshChat(msg);
                    break;
                case CMD.RspBuy:
                    MainCitySys.MainInstance.RspBuy(msg);
                    break;
                case CMD.PshPower:
                    MainCitySys.MainInstance.PshPower(msg);
                    break;
                case CMD.RspTakeTaskReward:
                    MainCitySys.MainInstance.RspTakeTaskReward(msg);
                    break;
                case CMD.PshTaskPrgs:
                    MainCitySys.MainInstance.PshTaskPrgs(msg);
                    break;
                case CMD.RspFBFight:
                    FubenSys.MainInstance.RspFBFight(msg);
                    break;
                case CMD.RspFBFightEnd:
                    BattleSys.MainInstance.RspFightEnd(msg);
                    break;
            }
        }

        private void OnDisable()
        {
            RemoveListener();
        }

        private void OnDestroy()
        {
            UniEvent.Destroy();
        }
    }
}
