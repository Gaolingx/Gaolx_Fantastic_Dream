using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using HuHu;

namespace DarkGod.Main
{
    public class DelaySignalManager : Singleton<DelaySignalManager>
    {
        private List<CtsInfo> ctsInfos = new List<CtsInfo>();

        private int Id;

        public CtsInfo CreatCts()
        {
            CancellationTokenSource cts = new CancellationTokenSource();
            CtsInfo ctsInfo = new CtsInfo
            {
                cts = cts,
                id = Id
            };
            Id++;
            ctsInfos.Add(ctsInfo);
            return ctsInfo;
        }

        public void CancelAllTask()
        {
            ctsInfos.ForEach(delegate (CtsInfo info)
            {
                info.Cancel();
            });
            ctsInfos.Clear();
        }

        public void CancelTask(int id)
        {
            foreach (CtsInfo ctsInfo in ctsInfos)
            {
                if (ctsInfo.id == id)
                {
                    ctsInfo.Cancel();
                    break;
                }
            }
        }

        public void CancelTask(CtsInfo ctsInfo)
        {
            ctsInfo?.Cancel();
        }

        public void DisposeCts(CtsInfo ctsInfo)
        {
            ctsInfo.Dispose();
            ctsInfos.Remove(ctsInfo);
        }

        public void Dispose()
        {
            Id = 0;
            ctsInfos.Clear();
            OnDestroyInstance();
        }

        public async UniTask<bool> Delay(TimeSpan delayTimeSpan, Action cancelAction = null)
        {
            CtsInfo cts = CreatCts();
            if (cancelAction != null)
            {
                cts.Token.Register(cancelAction);
            }
            bool result = await UniTask.Delay(delayTimeSpan, ignoreTimeScale: false, PlayerLoopTiming.Update, cts.Token).SuppressCancellationThrow();
            DisposeCts(cts);
            return result;
        }

        public async UniTask<bool> Delay(int millisecondsDelay, Action cancelAction = null)
        {
            CtsInfo cts = CreatCts();
            if (cancelAction != null)
            {
                cts.Token.Register(cancelAction);
            }
            bool result = await UniTask.Delay(millisecondsDelay, ignoreTimeScale: false, PlayerLoopTiming.Update, cts.Token).SuppressCancellationThrow();
            DisposeCts(cts);
            return result;
        }

        public async UniTask<bool> WaitWhile(Func<bool> predicate, Action cancelAction = null)
        {
            CtsInfo cts = CreatCts();
            if (cancelAction != null)
            {
                cts.Token.Register(cancelAction);
            }
            bool result = await UniTask.WaitWhile(predicate, PlayerLoopTiming.Update, cts.Token).SuppressCancellationThrow();
            DisposeCts(cts);
            return result;
        }

        public async UniTask<bool> WaitUntil(Func<bool> predicate, Action cancelAction = null)
        {
            CtsInfo cts = CreatCts();
            if (cancelAction != null)
            {
                cts.Token.Register(cancelAction);
            }
            bool result = await UniTask.WaitUntil(predicate, PlayerLoopTiming.Update, cts.Token).SuppressCancellationThrow();
            DisposeCts(cts);
            return result;
        }

        public async UniTask<bool> WhenAll(params UniTask[] tasks)
        {
            CtsInfo cts = CreatCts();
            bool result = await UniTask.WhenAll(tasks).AttachExternalCancellation(cts.Token).SuppressCancellationThrow();
            DisposeCts(cts);
            return result;
        }
    }
}
