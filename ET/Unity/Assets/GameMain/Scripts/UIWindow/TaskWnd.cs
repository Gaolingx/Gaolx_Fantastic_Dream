//功能：任务奖励界面


using PEProtocol;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class TaskWnd : WindowRoot {
    public Transform scrollTrans;

    private PlayerData pd = null;
    private List<TaskRewardData> trdLst = new List<TaskRewardData>();

    protected override void InitWnd() {
        base.InitWnd();

        pd = GameRoot.Instance.PlayerData;
        RefreshUI();
    }

    public void RefreshUI() {
        trdLst.Clear();

        List<TaskRewardData> todoLst = new List<TaskRewardData>();
        List<TaskRewardData> doneLst = new List<TaskRewardData>();

        //数据格式：1|0|0
        for(int i = 0;i<pd.taskArr.Length;i++)
        {
            //分割字符串
            string[] taskInfo = pd.taskArr[i].Split('|');
            TaskRewardData trd = new TaskRewardData
            {
                ID = int.Parse(taskInfo[0]),
                prgs = int.Parse(taskInfo[1]),
                taked = taskInfo[2].Equals("1") //注意数据类型
            };

            //判断奖励是否被领取
            if(trd.taked)
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


        }
    }

    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
        SetWndState(false);
    }
}