// 功能：事件管理器

using HuHu;
using System;
using UniFramework.Event;
using UnityEngine;

namespace DarkGod.Main
{
    public class GameWindowShowMessage
    {
        public GameWindowShowMessage(string msg)
        {
            Message = msg;
        }
        public string Message { get; }
    }

    public class EventMgr : Singleton<EventMgr>
    {
        private readonly EventGroup _eventGroup = new EventGroup();

        protected override void Awake()
        {
            base.Awake();

            InitMgr();
        }

        public void InitMgr()
        {
            UniEvent.Initalize();
            AddListener();

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
            _eventGroup.AddListener<OnEntityPlayerChangedEvent>(OnHandleEventMessage);
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

        public void ShowMessageBox(object sender, GameWindowShowMessage eventArgs)
        {
            Debug.Log($"Sender:{sender.GetType().FullName},ShowMessage:{eventArgs.Message}");
            OnShowMessageBoxEvent.SendEventMessage(eventArgs.Message);
        }

        public class OnQualityLevelEvent : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new OnQualityLevelEvent();
                UniEvent.SendMessage(msg);
            }
        }

        public class OnSoundVolumeChangedEvent : IEventMessage
        {
            public static void SendEventMessage()
            {
                var msg = new OnSoundVolumeChangedEvent();
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
        /// 定义游戏全局事件
        /// </summary>
        public Action OnGameEnter { get; set; }
        public Action OnGameExit { get; set; }
        public Action<bool> OnGamePause { get; set; }
        public BindableProperty<EntityPlayer> CurrentEPlayer { get; set; } = new BindableProperty<EntityPlayer>();

        /// <summary>
        /// 接收事件
        /// </summary>
        private void OnHandleEventMessage(IEventMessage message)
        {
            if (message is OnGameEnterEvent)
            {
                OnGameEnter?.Invoke();
            }
            else if (message is OnGameExitEvent)
            {
                OnGameExit?.Invoke();
            }
            else if (message is OnGamePauseEvent)
            {
                OnGamePauseEvent events = message as OnGamePauseEvent;
                OnGamePause?.Invoke(events.state);
            }
            else if (message is OnShowMessageBoxEvent)
            {
                OnShowMessageBoxEvent events = message as OnShowMessageBoxEvent;
                MessageBox.MainInstance.ShowMessage(events.message);
            }
            else if (message is OnQualityLevelEvent)
            {
                QualitySvc.MainInstance.SavePlayerData();
            }
            else if (message is OnSoundVolumeChangedEvent)
            {
                QualitySvc.MainInstance.SavePlayerData2();
            }
            else if (message is OnEntityPlayerChangedEvent)
            {
                OnEntityPlayerChangedEvent events = message as OnEntityPlayerChangedEvent;
                CurrentEPlayer.Value = events.Value;
            }
        }

        private void OnDestroy()
        {
            UniEvent.Destroy();
        }
    }
}
