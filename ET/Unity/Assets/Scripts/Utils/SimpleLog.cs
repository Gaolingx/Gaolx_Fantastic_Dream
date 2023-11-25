using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace GameMain.Utils
{
    public class SimpleLog
    {
#if UNITY_EDITOR
        private static string Prefix => "";
#else
        private static string Prefix => $"[{DateTime.Now:HH.mm.ss.fff}]";
#endif

        public static void Log(string strMsg)
        {
            Debug.Log($"{Prefix}{strMsg}");
        }

        public static void LogWarning(string strMsg)
        {
            Debug.LogWarning($"{Prefix}{strMsg}");
        }

        public static void LogError(string strMsg)
        {
            Debug.LogError($"{Prefix}{strMsg}");
        }

        public static void LogException(Exception ex)
        {
            LogWarning("Exception Occurred:");
            Debug.LogException(ex);
        }

        public static void LogException(Exception ex, Object context)
        {
            LogWarning("Exception Occurred:");
            Debug.LogException(ex, context);
        }
    }
}
