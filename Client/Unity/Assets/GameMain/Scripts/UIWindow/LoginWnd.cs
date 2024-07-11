//���ܣ���¼ע�����
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static DarkGod.Main.PlayerPrefsSvc;

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

        protected override void InitWnd()
        {
            base.InitWnd();

            SetHotfixVersionWnd();

            btnRemember.isOn = PlayerPrefsSvc.MainInstance.GetLoginItem().isRemember;
            iptAcct.text = PlayerPrefsSvc.MainInstance.GetLoginItem().account;
            iptPass.text = PlayerPrefsSvc.MainInstance.GetLoginItem().password;
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
                LoginItem loginItem = new LoginItem
                {
                    account = _acct,
                    password = _pass,
                    isRemember = btnRemember.isOn
                };
                PlayerPrefsSvc.MainInstance.SetGetLoginItem(loginItem);

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
