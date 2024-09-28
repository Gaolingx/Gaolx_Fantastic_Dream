//功能：登录注册界面
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using XiHUtil;

namespace DarkGod.Main
{
    public class LoginWnd : WindowRoot, IWindowRoot
    {
        public InputField iptAcct;
        public InputField iptPass;
        public Button btnEnter;
        public Button btnNotice;
        public Toggle btnRemember;  //记住密码选项
        public Text txtVersion;

        protected override void InitWnd()
        {
            base.InitWnd();

            SetHotfixVersionWnd();

            btnRemember.isOn = (bool)playerPrefsSvc.GetLoginItem("Login_RememberPass");
            iptAcct.text = (string)playerPrefsSvc.GetLoginItem("Login_Password");
            iptPass.text = (string)playerPrefsSvc.GetLoginItem("Login_Password");
        }

        public void OnEnable()
        {
            btnEnter.onClick.AddListener(delegate { ClickEnterBtn(); });
            btnNotice.onClick.AddListener(delegate { ClicKNoticeBtn(); });
        }

        private void SetHotfixVersionWnd()
        {
            txtVersion.text = GameRoot.MainInstance.GetHotfixVersion();
        }

        /// <summary>
        /// 点击进入游戏
        /// </summary>
        public void ClickEnterBtn()
        {
            audioSvc.PlayUIAudio(Constants.UILoginBtn);

            string _acct = iptAcct.text;
            string _pass = iptPass.text;
            if (_acct != "" && _pass != "")
            {
                //更新本地存储的账号密码
                PlayerPrefsUtil.Set("Login_Account", _acct);
                PlayerPrefsUtil.Set("Login_Password", _pass);
                PlayerPrefsUtil.Set("Login_RememberPass", btnRemember.isOn);

                //发送网络消息，请求登录
                GameMsg msg = new GameMsg
                {
                    cmd = (int)CMD.ReqLogin,
                    reqLogin = new ReqLogin
                    {
                        acct = _acct,
                        pass = _pass
                    }
                };
                //调用网络服务，发送包含账号密码的网络消息
                netSvc.SendMsg(msg);
            }
            else
            {
                MsgBox.MainInstance.ShowMessageBox("账号或密码为空");
            }
        }

        public void ClicKNoticeBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            MsgBox.MainInstance.ShowMessageBox("功能正在开发中...");
        }

        public void OnDisable()
        {
            btnEnter.onClick.RemoveAllListeners();
            btnNotice.onClick.RemoveAllListeners();
        }

        public void ClickCloseBtn()
        {

        }
    }
}
