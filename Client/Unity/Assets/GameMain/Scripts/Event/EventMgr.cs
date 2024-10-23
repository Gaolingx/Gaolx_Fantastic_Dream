using HuHu;
using System;

namespace DarkGod.Main
{
    public enum GameStateEventCode
    {
        GameStart,
        GamePause,
        GameContinue,
        GameStop
    }
    public class GameStateEventArgs : EventArgs
    {
        public GameStateEventArgs(GameStateEventCode gameStateEventCode)
        {
            GameStateEventCode = gameStateEventCode;
        }
        public GameStateEventCode GameStateEventCode { get; }
    }

    public class GameWindowShowMessage : EventArgs
    {
        public GameWindowShowMessage(string msg)
        {
            Message = msg;
        }
        public string Message { get; }
    }

    public class EventMgr : Singleton<EventMgr>
    {
        protected override void Awake()
        {
            base.Awake();

            _onGameStateEventHandler += C_OnGameStateOperationEvent;
            _onGameWindowShowMessageEventHandler += C_OnGameWindowShowMessageEvent;
        }

        // 定义游戏全局事件
        private event EventHandler<GameStateEventArgs> _onGameStateEventHandler;
        public Action OnGameEnter { get; set; }
        public Action<bool> OnGamePause { get; set; }
        public Action OnGameExit { get; set; }

        private void C_OnGameStateOperationEvent(object sender, GameStateEventArgs eventArgs)
        {
            if (eventArgs.GameStateEventCode == GameStateEventCode.GameStart)
            {
                OnGameEnter?.Invoke();
            }
            else if (eventArgs.GameStateEventCode == GameStateEventCode.GamePause)
            {
                OnGamePause?.Invoke(true);
            }
            else if (eventArgs.GameStateEventCode == GameStateEventCode.GameContinue)
            {
                OnGamePause.Invoke(false);
            }
            else if (eventArgs.GameStateEventCode == GameStateEventCode.GameStop)
            {
                OnGameExit?.Invoke();
            }
        }

        public void SendMessage_GameState(object sender, GameStateEventArgs eventArgs)
        {
            _onGameStateEventHandler?.Invoke(sender, eventArgs);
        }

        private EventHandler<GameWindowShowMessage> _onGameWindowShowMessageEventHandler;

        private void C_OnGameWindowShowMessageEvent(object sender, GameWindowShowMessage eventArgs)
        {
            GameRoot.MainInstance.dynamicWnd.AddTips(eventArgs.Message);
        }

        public void ShowMessageBox(object sender, GameWindowShowMessage eventArgs)
        {
            _onGameWindowShowMessageEventHandler?.Invoke(sender, eventArgs);
        }

        public BindableProperty<int> QualityLevel { get; set; } = new BindableProperty<int>();
        public BindableProperty<EntityPlayer> CurrentEPlayer { get; set; } = new BindableProperty<EntityPlayer>();

        private void OnDestroy()
        {
            _onGameStateEventHandler -= C_OnGameStateOperationEvent;
            _onGameWindowShowMessageEventHandler -= C_OnGameWindowShowMessageEvent;
        }
    }
}
