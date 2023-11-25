using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace GameMain.Scripts
{
    public partial class GameRoot
    {
        [SerializeField]
        private Image ProgressBar;

        [SerializeField]
        private Text ProgressCount;

        [SerializeField]
        private GameObject PopBoxRoot;

        [SerializeField]
        private Text PopBoxMsg;

        [SerializeField]
        private Button ExitBtn;

        private void InitUI()
        {
            ProgressCount.text = "连接服务器中....";
            ProgressBar.fillAmount = 0;

            PopBoxRoot.SetActive(false);

            ExitBtn.onClick.AddListener(() =>
            {
                Application.Quit();
            });
        }

        private void DownloadError(string msg)
        {
            PopBoxRoot.SetActive(true);
            PopBoxMsg.text = msg;
        }

        private void SetProgress(float percent, string txt)
        {
            ProgressCount.text = txt;
            ProgressBar.fillAmount = percent;
        }
    }
}
