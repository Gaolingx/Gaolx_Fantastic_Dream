//功能：计时服务

using HuHu;

namespace DarkGod.Main
{
    public class TimerSvc : Singleton<TimerSvc>
    {
        private PETimer pt;

        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        public void InitSvc()
        {
            pt = new PETimer();

            //设置定时器日志输出
            pt.SetLog((string info) =>
            {
                PECommon.Log(info);
            });
            PECommon.Log("Init TimerSvc...");
        }

        //定时任务检测
        public void Update()
        {
            if (pt != null)
            {
                pt.Update();
            }
        }

        //增加定时任务
        public int AddTimeTask(System.Action<int> callback, double delay, PETimeUnit timeUnit = PETimeUnit.Millisecond, int count = 1)
        {
            return pt.AddTimeTask(callback, delay, timeUnit, count);
        }

        public double GetNowTime()
        {
            return pt.GetMillisecondsTime();
        }

        public void DelTask(int tid)
        {
            pt.DeleteTimeTask(tid);
        }

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitSvc(); };
        }
    }
}
