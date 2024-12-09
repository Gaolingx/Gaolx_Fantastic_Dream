// 功能：事件管理器

using HuHu;
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
        }

        private void RemoveListener()
        {
            _eventGroup.RemoveAllListener();
        }

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
            public bool state = false;
            public static void SendEventMessage(bool state)
            {
                var msg = new OnGamePauseEvent();
                msg.state = state;
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
            public string message;
            public static void SendEventMessage(string message)
            {
                var msg = new OnShowMessageBoxEvent();
                msg.message = message;
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
            public EntityPlayer Value;
            public static void SendEventMessage(EntityPlayer entity)
            {
                var msg = new OnEntityPlayerChangedEvent();
                msg.Value = entity;
                UniEvent.SendMessage(msg);
            }
        }

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
                GameStateEvent.MainInstance.OnGamePause?.Invoke(events.state);
            }
            else if (message is OnShowMessageBoxEvent)
            {
                OnShowMessageBoxEvent events = message as OnShowMessageBoxEvent;
                MessageBox.MainInstance.ShowMessage(events.message);
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
                GameStateEvent.MainInstance.CurrentEPlayer.Value = events.Value;
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
