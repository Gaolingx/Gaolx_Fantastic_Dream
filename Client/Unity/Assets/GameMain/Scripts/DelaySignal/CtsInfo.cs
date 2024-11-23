using System.Threading;

namespace DarkGod.Main
{
    public class CtsInfo
    {
        public int id;

        public CancellationTokenSource cts;

        public CancellationToken Token => cts.Token;

        public bool IsCancellationRequested
        {
            get
            {
                if (cts != null)
                {
                    return cts.Token.IsCancellationRequested;
                }
                return false;
            }
        }

        public void Cancel()
        {
            if (cts != null)
            {
                if (!IsCancellationRequested)
                {
                    cts.Cancel();
                }
                cts.Dispose();
                cts = null;
            }
        }

        public void Dispose()
        {
            cts?.Dispose();
            cts = null;
        }
    }
}
