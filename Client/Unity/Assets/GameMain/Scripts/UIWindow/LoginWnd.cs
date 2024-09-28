//���ܣ���¼ע�����
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
        public Toggle btnRemember;  //��ס����ѡ��
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
                PlayerPrefsUtil.Set("Login_Account", _acct);
                PlayerPrefsUtil.Set("Login_Password", _pass);
                PlayerPrefsUtil.Set("Login_RememberPass", btnRemember.isOn);

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
                MsgBox.MainInstance.ShowMessageBox("�˺Ż�����Ϊ��");
            }
        }

        public void ClicKNoticeBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            MsgBox.MainInstance.ShowMessageBox("�������ڿ�����...");
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
