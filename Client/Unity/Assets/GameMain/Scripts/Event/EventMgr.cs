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

            UniEvent.Initalize();
            AddListener();
        }

        private void AddListener()
        {
            _eventGroup.AddListener<OnGameEnterEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnGameExitEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnGamePauseEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnShowMessageBoxEvent>(OnHandleEventMessage);
            _eventGroup.AddListener<OnQualityLevelEvent>(OnHandleEventMessage);
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
                OnQualityLevelEvent events = message as OnQualityLevelEvent;
                QualitySvc.MainInstance.SavePrefsData(events.Value);
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
            public QualitySvc.PlayerPrefsData Value;

            public static void SendEventMessage(QualitySvc.PlayerPrefsData message)
            {
                var msg = new OnQualityLevelEvent();
                msg.Value = message;
                UniEvent.SendMessage(msg);
            }
        }

        // 定义游戏全局事件
        public Action OnGameEnter { get; set; }
        public Action OnGameExit { get; set; }
        public Action<bool> OnGamePause { get; set; }
        public BindableProperty<EntityPlayer> CurrentEPlayer { get; set; } = new BindableProperty<EntityPlayer>();

        private void OnDisable()
        {
            UniEvent.Destroy();
        }
    }
}
