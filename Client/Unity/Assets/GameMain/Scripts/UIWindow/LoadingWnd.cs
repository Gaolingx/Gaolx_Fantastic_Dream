//功能：加载进度界面

using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace DarkGod.Main
{
    public class LoadingWnd : WindowRoot, IWindowRoot
    {
        public TMP_Text txtTips;
        public Image imgFG;
        public Image imgPoint;
        public TMP_Text txtPrg;  //进度的百分比
        public Animation tipsAnimation;

        private float fgWidth;

        //初始化窗口（进度条归零）
        //随机弹出一条Tips
        protected override async void InitWnd()
        {
            base.InitWnd();

            fgWidth = imgFG.GetComponent<RectTransform>().sizeDelta.x;

            SetText(txtTips, "按住Alt键不放可显示光标");
            SetText(txtPrg, "0%");
            imgFG.fillAmount = 0;
            //计算进度条点的位置
            imgPoint.transform.localPosition = new Vector3(-570f, 0, 0);

            //弹出tips
            await DelaySignalManager.MainInstance.Delay(TimeSpan.FromSeconds(0.5));
            if (tipsAnimation != null)
            {
                tipsAnimation.Play();
            }
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

        public void OnEnable()
        {

        }

        public void OnDisable()
        {

        }

        public void ClickCloseBtn()
        {

        }
    }
}
