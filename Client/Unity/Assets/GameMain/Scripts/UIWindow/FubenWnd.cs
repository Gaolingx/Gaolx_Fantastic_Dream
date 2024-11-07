//功能：副本选择界面

using PEProtocol;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class FubenWnd : WindowRoot, IWindowRoot
    {
        public List<Button> fbBtnArr;
        public Button btnClose;

        public Transform pointerTrans;

        private PlayerData pd;

        protected override void InitWnd()
        {
            base.InitWnd();

            GameRoot.MainInstance.PauseGameUIAction?.Invoke(true);

            pd = GameRoot.MainInstance.PlayerData;
            RefreshUI();
        }

        public void OnEnable()
        {
            btnClose.onClick.AddListener(delegate { ClickCloseBtn(); });
        }

        public void RefreshUI()
        {
            int fbid = pd.fuben;
            //根据当前副本进度控制图标显示（只显示当前待完成副本的图标）
            for (int i = 0; i < fbBtnArr.Count; i++)
            {
                int j = i;
                fbBtnArr[i].onClick.AddListener(delegate { ClickTaskBtn(10001 + j); });

                if (i < fbid % 10000)
                {
                    SetActive(fbBtnArr[i]);
                    if (i == fbid % 10000 - 1)
                    {
                        pointerTrans.SetParent(fbBtnArr[i].transform);
                        pointerTrans.localPosition = new Vector3(25, 100, 0);
                    }
                }
                else
                {
                    SetActive(fbBtnArr[i], false);
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
                EventMgr.MainInstance.ShowMessageBox(this, new("体力值不足"));
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
            GameRoot.MainInstance.PauseGameUIAction?.Invoke(false);
            SetWndState(false);
        }

        public void OnDisable()
        {
            btnClose.onClick.RemoveAllListeners();
            for (int i = 0; i < fbBtnArr.Count; i++)
            {
                fbBtnArr[i].onClick.RemoveAllListeners();
            }
        }
    }
}