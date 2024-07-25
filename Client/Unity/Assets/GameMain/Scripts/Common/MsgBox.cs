using HuHu;

namespace DarkGod.Main
{
    public class MsgBox : Singleton<MsgBox>
    {
        protected override void Awake()
        {
            base.Awake();
        }

        public void ShowMessageBox(string message)
        {
            GameRoot.MainInstance.dynamicWnd.AddTips(message);
        }
    }
}