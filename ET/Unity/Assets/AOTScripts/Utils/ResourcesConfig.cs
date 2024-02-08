using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GameMain.Utils
{
    [Serializable]
    public class ItemConfig
    {
        public string name = "";
        public string hash = "";
        public uint size = 0;
    }

    [Serializable]
    public class HotfixConfig
    {
        public List<ItemConfig> Items = new();

        [NonSerialized]
        private readonly Dictionary<string, (string, uint)> DictItems = new();

        private void UpdateData()
        {
            foreach (var item in Items)
            {
                DictItems.Add(item.name, (item.hash, item.size));
            }
        }

        public List<ItemConfig> FetchChanges(HotfixConfig remote)
        {
            var ret = new List<ItemConfig>();

            if (remote == null || remote.Items == null)
            {
                return Items;
            }

            if (DictItems.Count == 0)
            {
                UpdateData();
            }

            foreach (var item in remote.Items)
            {
                if (!DictItems.TryGetValue(item.name, out var val)
                    || string.IsNullOrEmpty(val.Item1) || val.Item1 != item.hash || val.Item2 != item.size)
                {
                    ret.Add(item);
                }
            }

            return ret;
        }

        public List<ItemConfig> WillDownload(HotfixConfig remote)
        {
            var ret = new List<ItemConfig>();

            if (remote == null || remote.Items == null)
            {
                return ret;
            }

            if (DictItems.Count == 0)
            {
                UpdateData();
            }
            foreach (var item in remote.Items)
            {
                var localFile = Path.GetFullPath(Path.Combine(GameConfig.LocalPath, item.name)).Replace('\\', '/');
                if (!DictItems.TryGetValue(item.name, out var val) || !File.Exists(localFile)
                    || string.IsNullOrEmpty(val.Item1) || val.Item1 != item.hash || val.Item2 != item.size)
                {
                    ret.Add(item);
                }
            }

            return ret;
        }
    }

    public class GameConfig
    {
        private static string _localPath = string.Empty;
        public static string LocalPath
        {
            get
            {
                if (string.IsNullOrEmpty(_localPath))
                {
                    _localPath = Path.GetFullPath(Application.persistentDataPath).Replace('\\', '/');
                    if (!_localPath.EndsWith('/'))
                    {
                        _localPath += '/';
                    }
                    SimpleLog.Log($"[GameConfig::LocalPath] localPath is {_localPath}");
                }

                return _localPath;
            }
        }

        private static string _remoteUrl = string.Empty;
        public static string RemoteUrl
        {
            get
            {
                if (string.IsNullOrEmpty(_remoteUrl))
                {
                    var platform = "win";
#if UNITY_IOS
                    platform = "ios";
#elif UNITY_ANDROID
                    platform = "android";
#endif
                    _remoteUrl = $"https://faceswap.mobi/game/{platform}/";
                }

                return _remoteUrl;
            }
        }
    }
}
