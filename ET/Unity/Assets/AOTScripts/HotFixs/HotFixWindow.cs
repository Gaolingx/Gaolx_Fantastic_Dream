using TMPro;
using UnityEngine;
using UnityEngine.UI;

//Developer: SangonomiyaSakunovi

public class HotFixWindow : MonoBehaviour
{
    public GameObject _hotfixPanel;
    public TMP_Text _tips;
    public TMP_Text _hotfixInfo;
    public Image _loadingProgressFG;
    public Image _loadingProgressPoint;
    public TMP_Text _loadingProgressText;
    public Button _confirmButton;
    public Button _cancelButton;

    private float _loadingProgressFGWidth;
    private float _loadingProgressPointYPos;

    public void OpenHotFixPanel()
    {
        InitWindow();
        _hotfixPanel.SetActive(true);
    }

    private void InitWindow()
    {
        _loadingProgressFGWidth = _loadingProgressFG.GetComponent<RectTransform>().sizeDelta.x;
        _loadingProgressPointYPos = _loadingProgressPoint.GetComponent<RectTransform>().sizeDelta.y;
        _loadingProgressText.text = "0%";
        _loadingProgressFG.fillAmount = 0;
        _loadingProgressPoint.transform.localPosition = new Vector3(-_loadingProgressFGWidth / 2, _loadingProgressPointYPos, 0);
        _confirmButton.onClick.AddListener(OnConfirmButtonClick);
        _cancelButton.onClick.AddListener(OnCancelButtonClick);
    }

    public void OnConfirmButtonClick()
    {
        SetTips("�������ظ���");
        HotFixService.Instance.RunHotFix();
    }

    public void OnCancelButtonClick()
    {
        SetTips("�ڲ��Է��У�ȡ�����޷�������Ϸ��Ŷ");
    }

    public void SetHotFixInfoText(long totalDownloadBytes)
    {
        long totalUploadMB = totalDownloadBytes / 1048576;
        string text = "��ǰ��Ҫ���ظ���" + totalUploadMB + "MB���ң�\n�Ƿ������\n��������Wifi�����½��У�";
        _hotfixInfo.text = text;
    }

    public void SetTips(string text)
    {
        _tips.text = text;
    }

    public void SetLoadingProgress(float loadingProgress)
    {
        _loadingProgressText.text = (int)(loadingProgress * 100) + "%";
        _loadingProgressFG.fillAmount = loadingProgress;
        float positionLoadingProgressPoint = loadingProgress * _loadingProgressFGWidth - _loadingProgressFGWidth / 2;
        _loadingProgressPoint.transform.localPosition = new Vector3(positionLoadingProgressPoint, _loadingProgressPointYPos, 0);
    }
}
