//功能：CancellationTokenSource 管理器

using System.Collections.Generic;
using System.Threading;

namespace DarkGod.Main
{
    public class CancellationTokenSourceMgr : GameBlackboard<CancellationTokenSourceMgr>
    {
        protected override void Init()
        {
            base.Init();
        }

        public void SetCtsValue(CtsType ctsType, CancellationTokenSource cts)
        {
            SetGameData(ctsType.ToString(), cts);
        }

        public CancellationTokenSource GetCtsValue(CtsType ctsType)
        {
            return GetGameData<CancellationTokenSource>(ctsType.ToString());
        }
    }
}
