//���ܣ���ɫ��������

using PEProtocol;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class CreateWnd : WindowRoot, IWindowRoot
    {
        public InputField iptName;
        public Button btnRand;
        public Button btnEnter;

        protected override void InitWnd()
        {
            base.InitWnd();

            //��ʾһ���������
            iptName.text = configSvc.GetRDNameCfg(false);
        }

        public void OnEnable()
        {
            btnRand.onClick.AddListener(delegate { ClickRandBtn(); });
            btnEnter.onClick.AddListener(delegate { ClickEnterBtn(); });
        }

        public void ClickRandBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            string rdName = configSvc.GetRDNameCfg(false);
            iptName.text = rdName;
        }

        public void ClickEnterBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            if (iptName.text != "")
            {
                //�����������ݵ�����������¼����
                GameMsg msg = new GameMsg
                {
                    cmd = (int)CMD.ReqRename,
                    reqRename = new ReqRename
                    {
                        name = iptName.text
                    }
                };
                netSvc.SendMsg(msg);
            }
            else
            {
                EventMgr.MainInstance.ShowMessageBox(this, new("��ǰ���ֲ����Ϲ淶"));
            }
        }

        public void OnDisable()
        {
            btnRand.onClick.RemoveAllListeners();
            btnEnter.onClick.RemoveAllListeners();
        }

        public void ClickCloseBtn()
        {

        }
    }
}
