//���ܣ���½ע��ҵ��ϵͳ
using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public class LoginSys : SystemRoot<LoginSys>
    {
        public static LoginSys Instance = null;

        public LoginWnd loginWnd;
        public CreateWnd createWnd;

        protected override void Awake()
        {
            base.Awake();

            GameRoot.MainInstance.OnGameEnter += InitSys;
        }

        public override void InitSys()
        {
            base.InitSys();

            Instance = MainInstance;
            PECommon.Log("Init LoginSys...");
        }

        /// <summary>
        /// �����¼����
        /// </summary>
        public void EnterLogin()
        {
            //�첽�ļ��ص�¼����
            //����ʾ���صĽ���
            resSvc.AsyncLoadScene(Constants.ResourcePackgeName, PathDefine.SceneLogin, () =>
            {
                //��������Ժ��ٴ�ע���¼����
                loginWnd.SetWndState();
                List<string> auLst = new List<string> { Constants.BGLogin };
                audioSvc.StopBGMusic();
                audioSvc.PlayBGMusics(auLst, 3f);
            });
            GameRoot.MainInstance.SetGameState(GameState.Login);
        }

        public void RspLogin(GameMsg msg)
        {
            MsgBox.MainInstance.ShowMessageBox("��¼�ɹ�");
            GameRoot.MainInstance.SetPlayerData(msg.rspLogin);

            if (msg.rspLogin.playerData.name == "")
            {
                //�򿪽�ɫ��������
                createWnd.SetWndState();
            }
            else
            {
                //��������
                MainCitySys.Instance.EnterMainCity();
            }
            //�رյ�¼����
            loginWnd.SetWndState(false);
        }

        public void RspRename(GameMsg msg)
        {
            GameRoot.MainInstance.SetPlayerName(msg.rspRename.name);

            //��ת������������
            //�����ǵĽ���
            MainCitySys.Instance.EnterMainCity();

            //�رմ�������
            createWnd.SetWndState(false);
        }

        private void OnDestroy()
        {
            GameRoot.MainInstance.OnGameEnter -= InitSys;
        }
    }
}
