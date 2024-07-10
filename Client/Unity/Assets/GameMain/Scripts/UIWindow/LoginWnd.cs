//功能：登录注册界面
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class LoginWnd : WindowRoot
    {
        public InputField iptAcct;
        public InputField iptPass;
        public Button btnEnter;
        public Button btnNotice;
        public Toggle btnRemember;  //记住密码选项
        public Text txtVersion;

        private string PrefsKeyLoginAccount = "Login_Account";
        private string PrefsKeyLoginPassword = "Login_Password";
        private string PrefsKeyLoginRemember = "Login_RememberPass";

        protected override void InitWnd()
        {
            base.InitWnd();

            SetHotfixVersionWnd();

            if (PlayerPrefs.HasKey(PrefsKeyLoginRemember))
            {
                btnRemember.isOn = UIItemUtils.IntToBool(PlayerPrefs.GetInt(PrefsKeyLoginRemember));
            }

            //获取本地存储的账号密码
            if (PlayerPrefs.HasKey(PrefsKeyLoginAccount) && PlayerPrefs.HasKey(PrefsKeyLoginPassword) && btnRemember.isOn == true)
            {
                iptAcct.text = PlayerPrefs.GetString(PrefsKeyLoginAccount);
                iptPass.text = PlayerPrefs.GetString(PrefsKeyLoginPassword);
            }
            else
            {
                iptAcct.text = "";
                iptPass.text = "";
            }
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
                PlayerPrefs.SetString(PrefsKeyLoginAccount, _acct);
                PlayerPrefs.SetString(PrefsKeyLoginPassword, _pass);
                PlayerPrefs.SetInt(PrefsKeyLoginRemember, UIItemUtils.BoolToInt(btnRemember.isOn));

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
                GameRoot.AddTips("账号或密码为空");
            }
        }

        public void ClicKNoticeBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            GameRoot.AddTips("功能正在开发中...");
        }
    }
}
