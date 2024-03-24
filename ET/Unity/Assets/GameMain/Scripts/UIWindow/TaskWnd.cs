//功能：任务奖励界面


using PEProtocol;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TaskWnd : WindowRoot {
    public Transform scrollTrans;

    private PlayerData pd = null;
    private List<TaskRewardData> trdLst = new List<TaskRewardData>();

    protected override void InitWnd()
    {
        base.InitWnd();

        pd = GameRoot.Instance.PlayerData;
        RefreshUI();
    }

    public void RefreshUI() {
        trdLst.Clear();

        List<TaskRewardData> todoLst = new List<TaskRewardData>();
        List<TaskRewardData> doneLst = new List<TaskRewardData>();

        //数据格式：1|0|0
        for (int i = 0; i < pd.taskArr.Length; i++)
        {
            //分割字符串
            string[] taskInfo = pd.taskArr[i].Split('|');
            TaskRewardData trd = new TaskRewardData
            {
                ID = int.Parse(taskInfo[0]),
                prgs = int.Parse(taskInfo[1]),
                taked = taskInfo[2].Equals("1") //注意数据类型
            };

            if (trd.taked)
            {
                doneLst.Add(trd);
            }
            else
            {
                todoLst.Add(trd);
            }
        }

        //按照任务完成度排序
        trdLst.AddRange(todoLst);
        trdLst.AddRange(doneLst);

        //刷新前删除所有子物体，避免每次打开重复生成
        for (int i = 0; i < scrollTrans.childCount; i++)
        {
            Destroy(scrollTrans.GetChild(i).gameObject);
        }

        //将排序完的trdLst分别实例化Prefab
        for (int i = 0; i < trdLst.Count; i++)
        {
            GameObject go = resSvc.LoadPrefab(PathDefine.TaskItemPrefab);
            go.transform.SetParent(scrollTrans);
            GameRoot.Instance.SetGameObjectTrans(go, Vector3.zero, Vector3.zero, Vector3.one);
            go.name = "taskItem_" + i;

            TaskRewardData trd = trdLst[i];
            TaskRewardCfg trf = resSvc.GetTaskRewardCfg(trd.ID);

            //通过父物体的transform查找
            SetText(GetTrans(go.transform, "txtName"), trf.taskName);
            SetText(GetTrans(go.transform, "txtPrg"), trd.prgs + "/" + trf.count);
            SetText(GetTrans(go.transform, "txtExp"), "奖励：    经验" + trf.exp);
            SetText(GetTrans(go.transform, "txtCoin"), "金币" + trf.coin);
            Image imgPrg = GetTrans(go.transform, "prgBar/prgVal").GetComponent<Image>();
            float prgVal = trd.prgs * 1.0f / trf.count;
            imgPrg.fillAmount = prgVal;

            Button btnTake = GetTrans(go.transform, "btnTake").GetComponent<Button>();
            //lambda表达式用于传参，知道玩家点击的到底是哪个任务的领取按钮
            //btnTake.onClick.AddListener(ClickTakeBtn);
            btnTake.onClick.AddListener(() => {
                ClickTakeBtn(go.name);
            });

            Transform transComp = GetTrans(go.transform, "imgComp");
            if(trd.taked)
            {
                btnTake.interactable = false; //奖励被领取则不能交互
                SetActive(transComp);
            }
            else
            {
                SetActive(transComp, false);
                //交互性考虑没有完成的情况
                if(trd.prgs >= trf.count)
                {
                    btnTake.interactable = true; //达到指定次数，可领取奖励
                }
                else
                {
                    btnTake.interactable = false;
                }
            }


        }
    }

    private void ClickTakeBtn(string name)
    {
        Debug.Log("Name:" + name);

        string[] nameArr = name.Split('_');
        int index = int.Parse(nameArr[1]);
        GameMsg msg = new GameMsg
        {
            cmd = (int)CMD.ReqTakeTaskReward,
            reqTakeTaskReward = new ReqTakeTaskReward
            {
                rid = trdLst[index].ID
            }
        };

        netSvc.SendMsg(msg);

        TaskRewardCfg trc = resSvc.GetTaskRewardCfg(trdLst[index].ID);
        int coin = trc.coin;
        int exp = trc.exp;
        GameRoot.AddTips(GetTextWithHexColor("获得奖励：", TextColorCode.Blue) + GetTextWithHexColor(" 金币 +" + coin + " 经验 +" + exp, TextColorCode.Green));
    }

    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }
}