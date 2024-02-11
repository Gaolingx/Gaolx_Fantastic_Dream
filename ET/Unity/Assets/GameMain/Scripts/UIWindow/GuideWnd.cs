//功能：引导对话界面
using PEProtocol;
using UnityEngine;
using UnityEngine.UI;

public class GuideWnd : WindowRoot {
    public Text txtName;
    public Text txtTalk;
    public Image imgIcon;

    private PlayerData pd;
    private AutoGuideCfg curtTaskData;
    private string[] dialogArr;
    private int index;

    protected override void InitWnd()
    {
        base.InitWnd();
    }

}