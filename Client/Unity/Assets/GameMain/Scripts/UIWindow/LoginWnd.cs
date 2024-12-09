//功能：登录注册界面

using Newtonsoft.Json;
using PEProtocol;
using TMPro;
using UnityEngine.UI;
using static DarkGod.Main.QualitySvc;

namespace DarkGod.Main
{
    public class LoginWnd : WindowRoot, IWindowRoot
    {
        public TMP_InputField iptAcct;
        public TMP_InputField iptPass;
        public Button btnEnter;
        public Button btnNotice;
        public Toggle btnRemember;  //记住密码选项
        public TMP_Text txtVersion;

        private const string prefsKey_LoginWnd = "prefsKey_LoginWnd";

        private void LoadPrefsData()
        {
            if (playerPrefsSvc.CheckPlayerPrefsHasKey(prefsKey_LoginWnd))
            {
                var json = playerPrefsSvc.LoadFromPlayerPrefs(prefsKey_LoginWnd);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData3>(json);

                btnRemember.isOn = saveData.isRemember;
                iptAcct.text = saveData.Login_Account;
                iptPass.text = saveData.Login_Password;
            }
        }

        protected override void InitWnd()
        {
            base.InitWnd();

            LoadPrefsData();
            SetHotfixVersionWnd();
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
                PlayerPrefsData3 playerPrefs = new PlayerPrefsData3();
                //更新本地存储的账号密码
                if (btnRemember.isOn)
                {
                    playerPrefs.Login_Password = _pass;
                    playerPrefs.Login_Account = _acct;
                }
                else
                {
                    playerPrefs.Login_Password = "";
                    playerPrefs.Login_Account = "";
                }

                playerPrefs.isRemember = btnRemember.isOn;
                EventMgr.OnLoginInfoChangedEvent.SendEventMessage(playerPrefs);

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
                EventMgr.OnShowMessageBoxEvent.SendEventMessage(new("账号或密码为空"));
            }
        }

        public void ClicKNoticeBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            EventMgr.OnShowMessageBoxEvent.SendEventMessage(new("功能正在开发中..."));
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
