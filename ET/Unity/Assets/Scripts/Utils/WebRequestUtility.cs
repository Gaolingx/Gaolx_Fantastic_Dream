using Cysharp.Threading.Tasks;
using GameMain.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;

namespace GameMain.Utils
{
    public class WebRequestUtility
    {
        private class IgnoreHttps : CertificateHandler
        {
            protected override bool ValidateCertificate(byte[] certificateData)
            {
                return true;
            }
        }

        public static void Get(string url, Action<string, string> callback)
        {
            GameRoot.Instance.StartCoroutine(WaitGet(url, (err, msg) =>
        {
            var m = "";
            if (msg != null && msg.Length > 0)
            {
                m = System.Text.Encoding.UTF8.GetString(msg);
            }
            callback?.Invoke(err, m);
        }));
        }

        public static (string, string) GetSync(string url)
        {
            (string, string) ret = default;
            var itor = WaitGet(url, (err, msg) =>
            {
                if (msg != null && msg.Length > 0)
                {
                    ret = (err, System.Text.Encoding.UTF8.GetString(msg));
                }
                else
                {
                    ret = (err, null);
                }

            });
            while (itor.MoveNext()) { }

            return ret;
        }

        public static (string, byte[]) GetBytes(string url)
        {
            (string, byte[]) ret = default;

            var itor = WaitGetBytes(url, (err, bytes) =>
            {
                ret = (err, bytes);
            });
            while (itor.MoveNext()) { }

            return ret;
        }

        public static void GetBytesAsync(string url, Action<string, byte[]> callback)
        {
            GameRoot.Instance.StartCoroutine(WaitGetBytes(url, callback));
        }

        private static IEnumerator WaitGetBytes(string url, Action<string, byte[]> callback, int timeout = 5)
        {
#if UNITY_IPHONE
        if (url.Contains(Application.streamingAssetsPath))
        {
            url = "file://" + url;
        }
#endif
            using var web = UnityWebRequest.Get(url);
            web.timeout = timeout;
            var req = web.SendWebRequest();
            while (!req.isDone)
            {
                yield return null;
            }

            if (!string.IsNullOrEmpty(req.webRequest.error))
            {
                callback?.Invoke(req.webRequest.error, null);
                yield break;
            }

            var ret = req.webRequest.downloadHandler.data;
            if (ret == null || ret.Length == 0)
            {
                callback?.Invoke($"req {url} ret empty!!!", null);
                yield break;
            }

            callback?.Invoke(null, ret);

        }

        public static IEnumerator WaitGet(string url, Action<string, byte[]> callback, int timeout = 5)
        {
            if (!url.Contains("file://") && !url.Contains("https:") && !url.Contains("http"))
            {
                url = $"file://{url}";
            }

            using var web = UnityWebRequest.Get(url);
            web.timeout = timeout;

            //if (url.Contains("https:"))
            //{
            //    web.certificateHandler = new IgnoreHttps();
            //}

            var req = web.SendWebRequest();
            while (!req.isDone)
            {
                yield return null;
            }

            if (!string.IsNullOrEmpty(req.webRequest.error))
            {
                callback?.Invoke(req.webRequest.error, null);
                yield break;
            }

            var ret = req.webRequest.downloadHandler.data;
            if (ret == null || ret.Length == 0)
            {
                callback?.Invoke($"req {url} ret empty!!!", null);
                yield break;
            }

            callback?.Invoke(null, ret);
        }

        public static string DownloadSync(string url, string dstName)
        {
            var ret = "";
            var itor = WaitDownload(url, dstName, err =>
            {
                ret = err;
            }, null);
            while (itor.MoveNext()) { }

            return ret;
        }

        public static void Download(string url, string dstName, Action<string> callback, Action<ulong, float> progress)
        {
            GameRoot.Instance.StartCoroutine(WaitDownload(url, dstName, callback, progress));
        }

        public static async UniTask<string> WaitDownload(string url, string dstName, Action<ulong, float> progress, int timeout = 5)
        {
            if (File.Exists(dstName))
            {
                File.Delete(dstName);
            }
            PathUtility.EnsureExistFileDirectory(dstName);

            SimpleLog.Log($"WaitDownload:{url} to:{dstName}");
            if (!url.Contains("file://") && !url.Contains("https:") && !url.Contains("http"))
            {
                url = $"file://{url}";
            }

            using var web = UnityWebRequest.Get(url);
            web.timeout = timeout;
            if (url.Contains("https:"))
            {
                web.certificateHandler = new IgnoreHttps();
            }

            web.downloadHandler = new DownloadHandlerFile(dstName);
            var req = web.SendWebRequest();
            while (!req.isDone)
            {
                progress?.Invoke(web.downloadedBytes, web.downloadProgress);
                await UniTask.Yield();
            }

            progress?.Invoke(web.downloadedBytes, 1f);
            return req.webRequest.error;
        }

        public static IEnumerator WaitDownload(string url, string dstName, Action<string> callback, Action<ulong, float> progress, int timeout = 5)
        {
            if (File.Exists(dstName))
            {
                File.Delete(dstName);
            }
            PathUtility.EnsureExistFileDirectory(dstName);

            SimpleLog.Log($"WaitDownload:{url} to:{dstName}");
            if (!url.Contains("file://") && !url.Contains("https:") && !url.Contains("http"))
            {
                url = $"file://{url}";
            }

            using var web = UnityWebRequest.Get(url);
            web.timeout = timeout;
            if (url.Contains("https:"))
            {
                web.certificateHandler = new IgnoreHttps();
            }

            web.downloadHandler = new DownloadHandlerFile(dstName);
            var req = web.SendWebRequest();
            while (!req.isDone)
            {
                progress?.Invoke(web.downloadedBytes, web.downloadProgress);
                yield return null;
            }

            if (!string.IsNullOrEmpty(req.webRequest.error))
            {
                callback?.Invoke(req.webRequest.error);
                yield break;
            }

            progress?.Invoke(web.downloadedBytes, 1f);

            callback?.Invoke(null);
        }

        public static Coroutine DownloadFileAsync(string url, string dstName, Action<string> callback,
            Action<ulong, float> progress, int timeout = 5)
        {
            return GameRoot.Instance.StartCoroutine(DownloadFile(url, dstName, callback, progress, timeout));
        }

        public static string DownloadFile(string url, string dstName)
        {
            var ret = "";
            var itor = DownloadFile(url, dstName, err =>
            {
                ret = err;
            }, null);
            while (itor.MoveNext()) { }

            return ret;
        }

        public static IEnumerator DownloadFile(string url, string dstName, Action<string> callback,
            Action<ulong, float> progress, int timeout = 5)
        {
            if (File.Exists(dstName))
            {
                File.Delete(dstName);
            }

            var dir = Path.GetDirectoryName(dstName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            //Debug.Log($"WaitDownload:{url} to:{dstName}");
            using var web = UnityWebRequest.Get(url);
            web.timeout = timeout;
            if (url.Contains("https:"))
            {
                web.certificateHandler = new IgnoreHttps();
            }

            web.downloadHandler = new DownloadHandlerFile(dstName);
            var req = web.SendWebRequest();
            while (!req.isDone)
            {
                progress?.Invoke(web.downloadedBytes, web.downloadProgress);
                yield return null;
            }

            if (!string.IsNullOrEmpty(req.webRequest.error))
            {
                callback?.Invoke(req.webRequest.error);
                yield break;
            }

            callback?.Invoke(null);
        }

        public static IEnumerator DownloadFileAppend(string url, string dstName, Action<string> callback,
            Action<ulong, float> progress, int timeout = 5)
        {
            var dir = Path.GetDirectoryName(dstName);
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }

            FileStream fs = new FileStream(dstName, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.Read);
            long startLen = fs.Length;

            using var web = UnityWebRequest.Get(url);
            web.timeout = timeout;
            if (url.Contains("https:"))
            {
                web.certificateHandler = new IgnoreHttps();
            }

            web.SetRequestHeader("Range", $"bytes={startLen}-");

            web.downloadHandler = new DownloadHandlerBuffer();
            var req = web.SendWebRequest();
            //var cRange = web.GetResponseHeader("Content-Range");
            while (!req.isDone)
            {
                yield return null;

                var data = web.downloadHandler.data;
                progress?.Invoke(web.downloadedBytes, web.downloadProgress);

                fs.Seek(0, SeekOrigin.End);

                int offset = (int)(fs.Length - startLen);
                int count = data.Length - offset;
                if (count > 0)
                {
                    fs.Write(data, offset, count);
                }
            }

            fs.Dispose();
            fs = null;

            if (!string.IsNullOrEmpty(req.webRequest.error))
            {
                callback?.Invoke(req.webRequest.error);
                yield break;
            }

            callback?.Invoke(null);

        }

        public static void DownloadFileAsyncNative(string url, string destPath, Action<string> callback, Action<ulong, float> progress, int timeout = 5)
        {
            using (WebClient client = new WebClient())
            {
                client.DownloadFileCompleted += (s, e) =>
                {
                    string error = null;
                    if (e.Error != null)
                    {
                        error = e.Error.Message;
                    }

                    SimpleLog.Log("DownloadFileCompleted  " + error);
                    callback?.Invoke(error);
                };

                client.DownloadFileAsync(new Uri(url), destPath);
            }
        }
    }
}
