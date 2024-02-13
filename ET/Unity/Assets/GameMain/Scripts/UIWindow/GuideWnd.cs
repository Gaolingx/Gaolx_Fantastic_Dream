//功能：引导对话界面
using PEProtocol;
using UnityEngine;
using UnityEngine.UI;

public class GuideWnd : WindowRoot {
    public Text txtName; //名字
    public Text txtTalk; //显示的对话内容
    public Image imgIcon; //人物形象（立绘）

    private PlayerData pd;
    private AutoGuideCfg curtTaskData; //任务数据
    private string[] dialogArr; //对话文本
    private int index; //记录对话位置的索引

    protected override void InitWnd()
    {
        base.InitWnd();

        pd = GameRoot.Instance.PlayerData; //获取玩家数据
        curtTaskData = MainCitySys.Instance.GetCurtTaskData(); //获取任务数据
        dialogArr = curtTaskData.dilogArr.Split('#'); //切割对话文本
        index = 1;

        SetTalk();
    }

    //显示对话内容
    private void SetTalk()
    {
        string[] talkArr = dialogArr[index].Split('|');
        if (talkArr[0] == "0")
        {
            //自己
            SetSprite(imgIcon,PathDefine.SelfIcon);
            SetText(txtName, pd.name);
        }
        else
        {
            //对话NPC
            //根据不同的npcID显示不同的npc形象
            switch (curtTaskData.npcID)
            {
                case 0:
                    SetSprite(imgIcon, PathDefine.WiseManIcon);
                    SetText(txtName, Constants.CurtTaskData_NpcID_0_Name);
                    break;
                case 1:
                    SetSprite(imgIcon, PathDefine.GeneralIcon);
                    SetText(txtName, Constants.CurtTaskData_NpcID_1_Name);
                    break;
                case 2:
                    SetSprite(imgIcon, PathDefine.ArtisanIcon);
                    SetText(txtName, Constants.CurtTaskData_NpcID_2_Name);
                    break;
                case 3:
                    SetSprite(imgIcon, PathDefine.TraderIcon);
                    SetText(txtName, Constants.CurtTaskData_NpcID_3_Name);
                    break;
                default:
                    SetSprite(imgIcon, PathDefine.GuideIcon);
                    SetText(txtName, Constants.CurtTaskData_NpcID_Default_Name);
                    break;
            }
        }

        imgIcon.SetNativeSize();
        //将文本中"$name"的部分替换为玩家的名字
        SetText(txtTalk, talkArr[1].Replace("$name", pd.name));
    }


    public void ClickNextBtn()
    {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        //点击下一步按钮index增加1
        index += 1;
        //判断是否所有的对话显示完成
        if (index == dialogArr.Length)
        {
            //TODO 发送任务引导完成信息

            SetWndState(false);
        }
        else
        {
            SetTalk();
        }
    }
}