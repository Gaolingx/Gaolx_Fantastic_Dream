using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//加载进度界面
public class LoadingWnd : WindowRoot
{
    public Text txtTips;
    public Image imgFG;
    public Image imgPoint;
    public Text txtPrg;  //进度的百分比

    private float fgWidth;

    //初始化窗口（进度条归零）
    //随机弹出一条Tips
    protected override void InitWnd()
    {
        base.InitWnd();

        fgWidth = imgFG.GetComponent<RectTransform>().sizeDelta.x;

        SetText(txtTips, "这是一条游戏Tips");
        SetText(txtPrg, "0%");
        imgFG.fillAmount = 0;
        //计算进度条点的位置
        imgPoint.transform.localPosition = new Vector3(-570f, 0, 0);

    }

    //定义函数设置进度
    public void SetProgress(float prg)  //传入当前进度
    {
        SetText(txtPrg, (int)(prg * 100) + "%");  //将浮点数转换为百分比
        imgFG.fillAmount = prg;

        //算出当前进度条的位置
        float posX = prg * fgWidth - 570;
        imgPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, 0);
    }

}
