using Cysharp.Threading.Tasks;
using GameMain.Utils;
using Newtonsoft.Json;
using System;
using System.IO;
using UnityEngine.Networking;

namespace GameMain.Scripts
{
    public partial class GameRoot
    {
        private readonly string sc_version_url = "Ver.bytes";

        private async UniTask<string> CheckFiles(string configName)
        {
            SimpleLog.Log($"[GameRoot::CheckFiles] begin");
            HotfixConfig localCfg = null;
            var localConfigFile = Path.GetFullPath(Path.Combine(GameConfig.LocalPath, configName)).Replace('\\', '/');
            if (File.Exists(localConfigFile))
            {
                localCfg = JsonConvert.DeserializeObject<HotfixConfig>(File.ReadAllText(localConfigFile));
            }
            localCfg ??= new();

            var remoteConfigUrl = GameConfig.RemoteUrl + configName;
            var req = UnityWebRequest.Get(remoteConfigUrl);
            req.timeout = 60;
            await req.SendWebRequest();
            SimpleLog.Log($"[GameRoot::CheckFiles] fetch:{remoteConfigUrl}");
            if (req.result != UnityWebRequest.Result.Success)
            {
                SimpleLog.Log($"[GameRoot::CheckFiles] fetch:{remoteConfigUrl} error:{req.error}");
                return "更新资源出错 [Code:1]";
            }

            var aText = req.downloadHandler.text;
            if (string.IsNullOrEmpty(aText))
            {
                SimpleLog.Log($"[GameRoot::CheckFiles] fetch:{remoteConfigUrl} error:text is empty");
                return "更新资源出错 [Code:2]";
            }

            SimpleLog.Log($"[GameRoot::CheckFiles] fetch:{remoteConfigUrl} res:{aText}");

            var remoteConfig = JsonConvert.DeserializeObject<HotfixConfig>(aText);
            if (remoteConfig?.Items == null)
            {
                SimpleLog.Log($"[GameRoot::CheckFiles] fetch:{remoteConfigUrl} error:parse localCfg");
                return "更新资源出错 [Code:3]";
            }

            var willDownloadLength = 0u;
            var willDownload = localCfg.WillDownload(remoteConfig);
            foreach (var item in willDownload)
            {
                willDownloadLength += item.size;
            }

            var downloadedBytesLength = 0ul;
            foreach (var item in willDownload)
            {
                var tmpLength = 0ul;
                var localFile = Path.GetFullPath(Path.Combine(GameConfig.LocalPath, item.name)).Replace('\\', '/');
                var errMsg = await WebRequestUtility.WaitDownload(GameConfig.RemoteUrl + item.name, localFile, (downloadedBytes, downloadProgress) =>
                {
                    tmpLength = Math.Max(tmpLength, downloadedBytes);

                    var percent = (float)(tmpLength + downloadedBytesLength) / willDownloadLength;
                    SetProgress(percent, $"正在更新配置:{(int)(percent * 10000) * 0.01f:F2}%");
                }, 60);

                if (string.IsNullOrEmpty(errMsg))
                {
                    downloadedBytesLength += tmpLength;
                }
                else
                {
                    File.Delete(localFile);
                    return $"更新资源出错 [Code:3] {errMsg}";
                }
            }

            File.WriteAllText(localConfigFile, JsonConvert.SerializeObject(remoteConfig));

            SimpleLog.Log($"[GameRoot::CheckFiles] end");
            return null;
        }

        private async UniTask<bool> CheckResource()
        {
            SimpleLog.Log($"[GameRoot::CheckResource] begin");

            var hasError = false;
            var dirInfo = new DirectoryInfo(GameConfig.LocalPath);
            if (!dirInfo.Exists)
            {
                dirInfo.Create();
            }

            await UniTask.Yield();

#if !UNITY_EDITOR
            SimpleLog.Log($"[GameRoot::CheckResource] dirInfo:{dirInfo.FullName}");

            var errMsg = await CheckFiles(sc_version_url);
            if (!string.IsNullOrEmpty(errMsg))
            {
                hasError = true;
                DownloadError(errMsg);
            }
#endif

            SimpleLog.Log($"[GameRoot::CheckResource] end");
            return hasError;
        }
    }
}
