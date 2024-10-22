//功能：登录注册界面

using Newtonsoft.Json;
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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

        private const string prefsKey_LoginWnd = "prefsKey_LoginWnd";

        [HideInInspector]
        [System.Serializable]
        private class PlayerPrefsData
        {
            public bool isRemember;
            public string Login_Account;
            public string Login_Password;
        }

        private void LoadPrefsData()
        {
            if (playerPrefsSvc.CheckPlayerPrefsHasKey(prefsKey_LoginWnd))
            {
                var json = playerPrefsSvc.LoadFromPlayerPrefs(prefsKey_LoginWnd);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);

                btnRemember.isOn = saveData.isRemember;
                iptAcct.text = saveData.Login_Account;
                iptPass.text = saveData.Login_Password;
            }
            else
            {
                btnRemember.isOn = true;
                iptAcct.text = "";
                iptPass.text = "";
            }
        }

        private void SavePrefsData()
        {
            var saveData = new PlayerPrefsData();

            saveData.isRemember = btnRemember.isOn;
            saveData.Login_Account = iptAcct.text;
            saveData.Login_Password = iptPass.text;

            playerPrefsSvc.SaveByPlayerPrefs(prefsKey_LoginWnd, saveData);
        }

        protected override void InitWnd()
        {
            base.InitWnd();

            SetHotfixVersionWnd();
            LoadPrefsData();
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
                if (btnRemember.isOn)
                {
                    SavePrefsData();
                }

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
                EventMgr.MainInstance.ShowMessageBox(this, new("账号或密码为空"));
            }
        }

        public void ClicKNoticeBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            EventMgr.MainInstance.ShowMessageBox(this, new("功能正在开发中..."));
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
