using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//���ؽ��Ƚ���
public class LoadingWnd : MonoBehaviour
{
    public Text txtTips;
    public Image imgFG;
    public Image imgPoint;
    public Text txtPrg;  //���ȵİٷֱ�

    private float fgWidth;

    //��ʼ�����ڣ����������㣩
    //�������һ��Tips
    public void InitWnd()
    {
        fgWidth = imgFG.GetComponent<RectTransform>().sizeDelta.x;
        txtTips.text = "����һ����ϷTips";
        txtPrg.text = "0%";
        imgFG.fillAmount = 0;
        //������������λ��
        imgPoint.transform.localPosition = new Vector3(-570f, 0, 0);

    }

    //���庯�����ý���
    public void SetProgress(float prg)  //���뵱ǰ����
    {
        txtPrg.text = (int)(prg * 100) + "%";  //��������ת��Ϊ�ٷֱ�
        imgFG.fillAmount=prg;

        //�����ǰ��������λ��
        float posX = prg * fgWidth - 570;
        imgPoint.GetComponent<RectTransform>().anchoredPosition = new Vector2(posX, 0);
    }

}
