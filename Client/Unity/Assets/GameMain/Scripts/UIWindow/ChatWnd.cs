//功能：聊天界面

using PEProtocol;
using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace DarkGod.Main
{
    public class ChatWnd : WindowRoot, IWindowRoot
    {
        public InputField iptChat;
        public Text txtChat;
        public Image imgWorld;
        public Image imgGuild;
        public Image imgFriend;

        public Button btnWord;
        public Button btnGuild;
        public Button btnFriend;
        public Button btnSend;
        public Button btnClose;

        private ChatType chatType; //0：世界，1：公会，2：好友
        private List<string> chatLst = new List<string>();

        protected override void InitWnd()
        {
            base.InitWnd();

            chatType = ChatType.ChatWorld;

            RefreshUI();
        }

        public void OnEnable()
        {
            btnWord.onClick.AddListener(delegate { ClickWorldBtn(); });
            btnGuild.onClick.AddListener(delegate { ClickGuildBtn(); });
            btnFriend.onClick.AddListener(delegate { ClickFriendBtn(); });
            btnSend.onClick.AddListener(delegate { ClickSendBtn(); });
            btnClose.onClick.AddListener(delegate { ClickCloseBtn(); });
        }

        //显示聊天信息
        public void AddChatMsg(string name, string chatTxt)
        {
            chatLst.Add(GetTextWithHexColor(name + "：", TextColorCode.Blue) + chatTxt);
            //显示聊天记录达到12后条删除最远的记录
            if (chatLst.Count > 12)
            {
                chatLst.RemoveAt(0);
            }
            //如果窗口没有激活，则不能刷新UI，否则会出现空引用异常
            if (GetWndState())
            {
                RefreshUI();
            }

        }

        private void RefreshUI()
        {
            switch (chatType)
            {
                case ChatType.ChatWorld:
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
                case ChatType.ChatGuild:
                    //TODO 公会
                    SetText(txtChat, "尚未加入公会");
                    SetSprite(imgWorld, PathDefine.ChatWndBtn2);
                    SetSprite(imgGuild, PathDefine.ChatWndBtn1);
                    SetSprite(imgFriend, PathDefine.ChatWndBtn2);
                    break;
                case ChatType.ChatFriend:
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

        private bool canSend = true;
        public void ClickSendBtn()
        {
            if (!canSend)
            {
                MsgBox.MainInstance.ShowMessageBox("聊天消息每5秒钟才能发送一条");
                return;
            }
            if (iptChat.text != null && iptChat.text != "" && iptChat.text != " ")
            {
                if (iptChat.text.Length > Constants.TextMaxLength)
                {
                    MsgBox.MainInstance.ShowMessageBox("输入信息不能超过" + Constants.TextMaxLength + "个字");
                }
                else
                {
                    //发送网络消息到服务器
                    GameMsg msg = new GameMsg
                    {
                        cmd = (int)CMD.SndChat,
                        sndChat = new SndChat
                        {
                            chat = iptChat.text
                        }
                    };
                    //发送消息后清空显示
                    iptChat.text = "";
                    netSvc.SendMsg(msg);

                    canSend = false;

                    //开启计时任务，5秒后将canSend改为true
                    timerSvc.AddTimeTask((int tid) => { canSend = true; }, Constants.SndMsgWaitForSeconds, PETimeUnit.Second);
                }
            }
            else
            {
                MsgBox.MainInstance.ShowMessageBox("尚未输入聊天信息");
            }
        }

        public void ClickWorldBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            chatType = ChatType.ChatWorld;
            RefreshUI();
        }
        public void ClickGuildBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            chatType = ChatType.ChatGuild;
            RefreshUI();
        }
        public void ClickFriendBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            chatType = ChatType.ChatFriend;
            RefreshUI();
        }
        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            chatType = 0;
            SetWndState(false);
        }

        public void OnDisable()
        {
            btnWord.onClick.RemoveAllListeners();
            btnGuild.onClick.RemoveAllListeners();
            btnFriend.onClick.RemoveAllListeners();
            btnSend.onClick.RemoveAllListeners();
            btnClose.onClick.RemoveAllListeners();
        }
    }
}