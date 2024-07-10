//���ܣ���¼ע�����
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
        public Toggle btnRemember;  //��ס����ѡ��
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

            //��ȡ���ش洢���˺�����
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
                PlayerPrefs.SetString(PrefsKeyLoginAccount, _acct);
                PlayerPrefs.SetString(PrefsKeyLoginPassword, _pass);
                PlayerPrefs.SetInt(PrefsKeyLoginRemember, UIItemUtils.BoolToInt(btnRemember.isOn));

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
                GameRoot.AddTips("�˺Ż�����Ϊ��");
            }
        }

        public void ClicKNoticeBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            GameRoot.AddTips("�������ڿ�����...");
        }
    }
}
