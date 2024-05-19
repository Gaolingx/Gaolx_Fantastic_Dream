//功能：强化升级界面
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using PEProtocol;
using UnityEngine.EventSystems;

namespace DarkGod.Main
{
    public class StrongWnd : WindowRoot
    {
        #region UI Define
        public Image imgCurtPos; //当前选中位置的图片
        public Text txtStartLv; //当前星级数量
        public Transform starTransGrp; //星星父物体的Transform
        public Text propHP1; //血量
        public Text propHurt1; //伤害
        public Text propDef1; //防御
        public Text propHP2;
        public Text propHurt2;
        public Text propDef2;
        public Image propArr1; //箭头
        public Image propArr2;
        public Image propArr3;

        public Text txtNeedLv; //升级所需的最低等级
        public Text txtCostCoin; //消耗的金币
        public Text txtCostCrystal; //消耗的水晶

        public Transform costTransRoot;
        public Text txtCoin; //当前拥有的金币
        #endregion

        #region Data Area
        public Transform posBtnTrans;
        private Image[] imgs = new Image[6];
        private int currentIndex;
        private PlayerData pd;
        StrongCfg nextSd;

        #endregion

        protected override void InitWnd()
        {
            base.InitWnd();
            pd = GameRoot.Instance.PlayerData;

            //注册点击事件
            RegClickEvts();

            ClickPosItem(0);
        }

        private void RegClickEvts()
        {
            //获取每个子物体中的image组件
            for (int i = 0; i < posBtnTrans.childCount; i++)
            {
                Image img = posBtnTrans.GetChild(i).GetComponent<Image>();

                //添加点击事件
                OnClick(img.gameObject, (object args) =>
                {
                    ClickPosItem((int)args);
                    audioSvc.PlayUIAudio(Constants.UIClickBtn);
                }, i);
                imgs[i] = img;
            }
        }

        //根据点击位置，展示不同部位信息，需要点击时传入一个参数，根据参数显示相应数据信息
        private void ClickPosItem(int index)
        {
            PECommon.Log("Click Window:StrongWnd.Click Item:" + index);

            //遍历数组，当点击其中一个部位，其背景变为箭头，其余的变成平板
            for (int i = 0; i < imgs.Length; i++)
            {
                Transform trans = imgs[i].transform;

                currentIndex = index;
                //判断当前点击的图片和遍历的图片是否一致
                if (i == currentIndex)
                {
                    //如果相等，则用箭头显示
                    SetSprite(imgs[i], PathDefine.ItemArrorBG);
                    //设置位置
                    trans.localPosition = new Vector3(10, trans.localPosition.y, 0);
                    //设置尺寸
                    trans.GetComponent<RectTransform>().sizeDelta = new Vector2(250, 95);
                }
                else
                {
                    SetSprite(imgs[i], PathDefine.ItemPlatBG);
                    trans.localPosition = new Vector3(0, trans.localPosition.y, 0);
                    trans.GetComponent<RectTransform>().sizeDelta = new Vector2(220, 85);
                }
            }

            RefreshItem();
        }

        private void RefreshItem()
        {
            //金币
            SetText(txtCoin, pd.coin);
            switch (currentIndex)
            {
                case 0:
                    SetSprite(imgCurtPos, PathDefine.ItemToukui);
                    break;
                case 1:
                    SetSprite(imgCurtPos, PathDefine.ItemBody);
                    break;
                case 2:
                    SetSprite(imgCurtPos, PathDefine.ItemYaobu);
                    break;
                case 3:
                    SetSprite(imgCurtPos, PathDefine.ItemHand);
                    break;
                case 4:
                    SetSprite(imgCurtPos, PathDefine.ItemLeg);
                    break;
                case 5:
                    SetSprite(imgCurtPos, PathDefine.ItemFoot);
                    break;
                default:
                    break;
            }
            //星级
            SetText(txtStartLv, pd.strongArr[currentIndex] + "星级");

            int curtStarLv = pd.strongArr[currentIndex];
            for (int i = 0; i < starTransGrp.childCount; i++)
            {
                //遍历获取子物体的Image组件
                Image img = starTransGrp.GetChild(i).GetComponent<Image>();
                if (i < curtStarLv)
                {
                    //如果当前遍历的星级图片<当前星级，说明没达到该新级，显示空的星级图片
                    SetSprite(img, PathDefine.SpStar2);
                }
                else
                {
                    //否则显示实的星级图片
                    SetSprite(img, PathDefine.SpStar1);
                }
            }

            int nextStarLv = curtStarLv + 1;
            //当前星级所有属性加成
            //resSvc.GetPropAddValPreLv(当前部位, 当前星级, 类型)
            int sumAddHp = resSvc.GetPropAddValPreLv(currentIndex, nextStarLv, 1);
            int sumAddHurt = resSvc.GetPropAddValPreLv(currentIndex, nextStarLv, 2);
            int sumAddDef = resSvc.GetPropAddValPreLv(currentIndex, nextStarLv, 3);
            SetText(propHP1, "生命  +" + sumAddHp);
            SetText(propHurt1, "伤害  +" + sumAddHurt);
            SetText(propDef1, "防御  +" + sumAddDef);

            //获取下一星级需要的属性数值
            nextSd = resSvc.GetStrongCfg(currentIndex, nextStarLv);
            if (nextSd != null)
            {
                SetActive(propHP2);
                SetActive(propHurt2);
                SetActive(propDef2);

                SetActive(costTransRoot);
                SetActive(propArr1);
                SetActive(propArr2);
                SetActive(propArr3);

                SetText(propHP2, "强化后 +" + nextSd.addhp);
                SetText(propHurt2, "+" + nextSd.addhurt);
                SetText(propDef2, "+" + nextSd.adddef);

                SetText(txtNeedLv, "需要等级：" + nextSd.minlv);
                SetText(txtCostCoin, "需要消耗：      " + nextSd.coin);

                SetText(txtCostCrystal, nextSd.crystal + "/" + pd.crystal);
            }
            else
            {
                //当前星级升满后，不能进行强化（关闭花销部分、隐藏强化所获取的属性加成）
                SetActive(propHP2, false);
                SetActive(propHurt2, false);
                SetActive(propDef2, false);

                SetActive(costTransRoot, false);
                SetActive(propArr1, false);
                SetActive(propArr2, false);
                SetActive(propArr3, false);
            }
        }

        public void ClickCloseBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);
            SetWndState(false);
        }

        public void ClickStrongBtn()
        {
            audioSvc.PlayUIAudio(Constants.UIClickBtn);

            //客户端本地数据校验，减小服务器验证压力
            //判断是否星级已满
            if (pd.strongArr[currentIndex] < 10)
            {
                //判断级别是否足够强化
                if (pd.lv < nextSd.minlv)
                {
                    GameRoot.AddTips("角色等级不够");
                    return;
                }
                //各种资源...
                if (pd.coin < nextSd.coin)
                {
                    GameRoot.AddTips("金币数量不够");
                    return;
                }
                if (pd.crystal < nextSd.crystal)
                {
                    GameRoot.AddTips("水晶不够");
                    return;
                }

                //发送请求强化数据
                netSvc.SendMsg(new GameMsg
                {
                    cmd = (int)CMD.ReqStrong,
                    reqStrong = new ReqStrong
                    {
                        pos = currentIndex
                    }
                });
            }
            else
            {
                GameRoot.AddTips("星级已经升满");
            }
        }

        public void UpdateUI()
        {
            audioSvc.PlayUIAudio(Constants.FBItemEnter);
            ClickPosItem(currentIndex);
        }
    }
}
