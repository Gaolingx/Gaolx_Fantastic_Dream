//功能：副本选择界面

using PEProtocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class FubenWnd : WindowRoot
    {
        public List<Button> fbBtnArr;
        public Button btnClose;

        public Transform pointerTrans;

        private PlayerData pd;

        protected override void InitWnd()
        {
            base.InitWnd();

            pd = GameRoot.MainInstance.PlayerData;

            btnClose.onClick.AddListener(delegate { ClickCloseBtn(); });

            RefreshUI();
        }

        public void RefreshUI()
        {
            int fbid = pd.fuben;
            //根据当前副本进度控制图标显示（只显示当前待完成副本的图标）
            for (int i = 0; i < fbBtnArr.Count; i++)
            {
                fbBtnArr[i].onClick.AddListener(delegate { ClickTaskBtn((10001 - fbBtnArr.Count) + i); });
                if (i < fbid % 10000)
                {
                    SetActive(fbBtnArr[i].gameObject);
                    if (i == fbid % 10000 - 1)
                    {
                        pointerTrans.SetParent(fbBtnArr[i].transform);
                        pointerTrans.localPosition = new Vector3(25, 100, 0);
                    }
                }
                else
                {
                    SetActive(fbBtnArr[i].gameObject, false);
                }
            }
        }

        public void ClickTaskBtn(int clickFbid)
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            //检查体力是否足够
            int powerInMission = configSvc.GetMapCfg(clickFbid).power;
            if (powerInMission > pd.power)
            {
                MsgBox.MainInstance.ShowMessageBox("体力值不足");
            }
            else
            {
                //请求服务器，开始副本战斗
                netSvc.SendMsg(new GameMsg
                {
                    cmd = (int)CMD.ReqFBFight,
                    reqFBFight = new ReqFBFight
                    {
                        fbid = clickFbid
                    }
                });
            }
        }

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            SetWndState(false);
        }

        private void OnDisable()
        {
            btnClose.onClick.RemoveAllListeners();
            for (int i = 0; i < fbBtnArr.Count; i++)
            {
                fbBtnArr[i].onClick.RemoveAllListeners();
            }
        }
    }
}