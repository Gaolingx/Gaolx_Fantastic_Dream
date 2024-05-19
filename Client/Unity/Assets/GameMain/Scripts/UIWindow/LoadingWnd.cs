namespace DarkGod.Main
{
//���ؽ��Ƚ���
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LoadingWnd : WindowRoot
{
    public Text txtTips;
    public Image imgFG;
    public Image imgPoint;
    public Text txtPrg;  //���ȵİٷֱ�

    private float fgWidth;

    //��ʼ�����ڣ����������㣩
    //�������һ��Tips
    protected override void InitWnd()
    {
        base.InitWnd();

        fgWidth = imgFG.GetComponent<RectTransform>().sizeDelta.x;

        SetText(txtTips, "��סAlt�����ſ���ʾ���");
        SetText(txtPrg, "0%");
        imgFG.fillAmount = 0;
        //������������λ��
        imgPoint.transform.localPosition = new Vector3(-570f, 0, 0);

    }

    //���庯�����ý���
    public void SetProgress(float prg)  //���뵱ǰ����
    {
        SetText(txtPrg, (int)(prg * 100) + "%");  //��������ת��Ϊ�ٷֱ�
        imgFG.fillAmount = prg;

        //�����ǰ��������λ��
        float posX = prg * fgWidth - 570;
        imgPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, 0);
    }

}

}