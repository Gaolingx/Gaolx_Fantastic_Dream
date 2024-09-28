//功能：UI窗口接口基类，这意味着继承了该接口的必须实现其中的方法

namespace DarkGod.Main
{
    public interface IWindowRoot
    {
        void OnEnable();

        void OnDisable();

        void ClickCloseBtn();
    }
}
