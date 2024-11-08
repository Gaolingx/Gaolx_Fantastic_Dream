//功能：登陆注册业务系统

using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class LoginSys : SystemRoot<LoginSys>
    {
        public LoginWnd loginWnd;
        public CreateWnd createWnd;

        protected override void Awake()
        {
            base.Awake();

            EventMgr.MainInstance.OnGameEnter += delegate { InitSys(); };
        }

        public override void InitSys()
        {
            base.InitSys();

            PECommon.Log("Init LoginSys...");
        }

        /// <summary>
        /// 进入登录场景
        /// </summary>
        public void EnterLogin()
        {
            //异步的加载登录场景
            //并显示加载的进度
            resSvc.AsyncLoadScene(Constants.ResourcePackgeName, PathDefine.SceneLogin, () =>
            {
                //加载完成以后再打开注册登录界面
                loginWnd.SetWndState();
                List<string> auLst = new List<string> { Constants.BGLogin };
                audioSvc.StopBGMusic();
                audioSvc.PlayBGMusics(auLst, 3f);
            });
            GameRoot.MainInstance.GameRootGameState = GameState.Login;
        }

        public void RspLogin(GameMsg msg)
        {
            EventMgr.MainInstance.ShowMessageBox(this, new("登录成功"));
            GameRoot.MainInstance.SetPlayerData(msg.rspLogin);

            if (msg.rspLogin.playerData.name == "")
            {
                //打开角色创建界面
                createWnd.SetWndState();
            }
            else
            {
                //进入主城
                MainCitySys.MainInstance.EnterMainCity();
            }
            //关闭登录界面
            loginWnd.SetWndState(false);
        }

        public void RspRename(GameMsg msg)
        {
            GameRoot.MainInstance.SetPlayerName(msg.rspRename.name);

            //跳转场景进入主城
            //打开主城的界面
            MainCitySys.MainInstance.EnterMainCity();

            //关闭创建界面
            createWnd.SetWndState(false);
        }

        private void OnDisable()
        {
            EventMgr.MainInstance.OnGameEnter -= delegate { InitSys(); };
        }
    }
}
