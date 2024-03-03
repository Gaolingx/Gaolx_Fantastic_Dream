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