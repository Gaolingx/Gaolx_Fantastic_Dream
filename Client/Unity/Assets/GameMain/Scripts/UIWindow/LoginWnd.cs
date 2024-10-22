//���ܣ���¼ע�����

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
        public Toggle btnRemember;  //��ס����ѡ��
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
        /// ���������Ϸ
        /// </summary>
        public void ClickEnterBtn()
        {
            audioSvc.PlayUIAudio(Constants.UILoginBtn);

            string _acct = iptAcct.text;
            string _pass = iptPass.text;
            if (_acct != "" && _pass != "")
            {
                //���±��ش洢���˺�����
                if (btnRemember.isOn)
                {
                    SavePrefsData();
                }

                //����������Ϣ�������¼
                GameMsg msg = new GameMsg
                {
                    cmd = (int)CMD.ReqLogin,
                    reqLogin = new ReqLogin
                    {
                        acct = _acct,
                        pass = _pass
                    }
                };
                //����������񣬷��Ͱ����˺������������Ϣ
                netSvc.SendMsg(msg);
            }
            else
            {
                EventMgr.MainInstance.ShowMessageBox(this, new("�˺Ż�����Ϊ��"));
            }
        }

        public void ClicKNoticeBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            EventMgr.MainInstance.ShowMessageBox(this, new("�������ڿ�����..."));
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
