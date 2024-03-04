//功能：聊天界面

using PEProtocol;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


public class ChatWnd : WindowRoot {
    public InputField iptChat;
    public Text txtChat;
    public Image imgWorld;
    public Image imgGuild;
    public Image imgFriend;

    private int chatType;
    private List<string> chatLst = new List<string>();

    protected override void InitWnd() {
        base.InitWnd();

        chatType = 0;

        RefreshUI();
    }

    private void RefreshUI()
    {
        switch (chatType)
        {
            case 0:
                //世界
                string chatMsg = "";
                //实现换行
                for (int i = 0; i < chatLst.Count; i++)
                {
                    chatMsg += chatLst[i] + "\n";
                }
                SetText(txtChat, chatMsg);

                //按钮显示控制
                SetSprite(imgWorld, PathDefine.ChatWndBtn1);
                SetSprite(imgGuild, PathDefine.ChatWndBtn2);
                SetSprite(imgFriend, PathDefine.ChatWndBtn2);
                break;
            case 1:
                //TODO 公会
                SetText(txtChat, "尚未加入公会");
                SetSprite(imgWorld, PathDefine.ChatWndBtn2);
                SetSprite(imgGuild, PathDefine.ChatWndBtn1);
                SetSprite(imgFriend, PathDefine.ChatWndBtn2);
                break;
            case 2:
                //TODO 好友
                SetText(txtChat, "暂无好友信息");
                SetSprite(imgWorld, PathDefine.ChatWndBtn2);
                SetSprite(imgGuild, PathDefine.ChatWndBtn2);
                SetSprite(imgFriend, PathDefine.ChatWndBtn1);
                break;
            default:
                break;
        }
    }

    public void ClickSendBtn() {

    }
    public void ClickWorldBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
    }
    public void ClickGuildBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
    }
    public void ClickFriendBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);
    }
    public void ClickCloseBtn() {
        audioSvc.PlayUIAudio(Constants.UIClickBtn);

        SetWndState(false);
    }
}