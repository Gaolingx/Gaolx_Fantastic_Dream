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


    //��ʼ�����ڣ����������㣩
    //�������һ��Tips
    public void InitWnd()
    {
        txtTips.text = "���ǵ�һ����ϷTips";
        txtPrg.text = "0%";
        imgFG.fillAmount = 0;
        //������������λ��
        imgPoint.transform.localPosition = new Vector3(-570f, 0, 0);

    }

    //���庯�����ý���
    public void SetProgress(float prg)  //���뵱ǰ����
    {

    }

}
