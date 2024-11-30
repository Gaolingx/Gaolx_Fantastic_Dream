using DarkGod.Tools;
using HuHu;
using System;

namespace DarkGod.Main
{
    public class GameStateEvent : Singleton<GameStateEvent>
    {
        /// <summary>
        /// 定义游戏全局事件
        /// </summary>
        public Action OnGameEnter { get; set; }
        public Action OnGameExit { get; set; }
        public Action<bool> OnGamePause { get; set; }
        public BindableProperty<EntityPlayer> CurrentEPlayer { get; set; } = new BindableProperty<EntityPlayer>();
    }
}
