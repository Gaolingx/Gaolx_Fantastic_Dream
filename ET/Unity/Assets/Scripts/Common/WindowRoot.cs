using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//功能：UI界面基类
public class WindowRoot : MonoBehaviour
{
    protected ResSvc resSvc = null;
    protected AudioSvc audioSvc = null;

    public void SetWndState(bool isActive = true)
    {
        if(gameObject.activeSelf != isActive)
        {
            //如果传入的参数与当前状态不相同则设置为目标状态
            SetActive(gameObject, isActive);
        }
        if(isActive)
        {
            InitWnd();
        }
        else
        {
            ClearWnd();
        }
    }
    protected virtual void InitWnd()
    {
        resSvc = ResSvc.Instance;
        audioSvc = AudioSvc.Instance;
    }
    protected virtual void ClearWnd()
    {
        resSvc = null;
        audioSvc = null;
    }

    #region Tool Functions

    protected void SetActive(GameObject go, bool isActive = true)
    {
        go.SetActive(isActive);
    }
    protected void SetActive(Transform trans, bool state = true)
    {
        trans.gameObject.SetActive(state);
    }
    protected void SetActive(RectTransform rectTrans, bool state = true)
    {
        rectTrans.gameObject.SetActive(state);
    }
    protected void SetActive(Image img, bool state = true)
    {
        img.transform.gameObject.SetActive(state);
    }
    protected void SetActive(Text txt, bool state = true)
    {
        txt.transform.gameObject.SetActive(state);
    }

    protected void SetText(Text txt, string context = "NaN")
    {
        txt.text = context;
    }
    protected void SetText(Transform trans, int num = 0)
    {
        SetText(trans.GetComponent<Text>(), num);
    }
    protected void SetText(Transform trans, string context = "")
    {
        SetText(trans.GetComponent<Text>(), context);
    }
    protected void SetText(Text txt, int num = 0)
    {
        SetText(txt, num.ToString());
    }

    #endregion
}
