#if UNITY_WX && !UNITY_EDITOR
#define UNITY_WX_WITHOUT_EDITOR
#endif
#if UNITY_WX_WITHOUT_EDITOR
using WeChatWASM;
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace XiHUtil
{
    public class PlayerPrefsUtil
    {
        public static void Set(string key, string val) {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetStringSync(key, val);
#elif UNITY_DY
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.SetString(key, val);
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.Save();
#else
            
            UnityEngine.PlayerPrefs.SetString(key, val);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static void Set(string key, bool val)
        {
            Set(key,val?1:0);
        }
        public static void Set(string key, int val)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetIntSync(key, val);
#elif UNITY_DY
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.SetInt(key, val);
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.Save();
#else
            UnityEngine.PlayerPrefs.SetInt(key, val);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static void Set(string key, float val)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageSetFloatSync(key, val);
#elif UNITY_DY
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.SetFloat(key, val);
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.Save();
#else
            UnityEngine.PlayerPrefs.SetFloat(key, val);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static string Get(string key, string val="")
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetStringSync(key, val);
#elif UNITY_DY
            return StarkSDKSpace.StarkSDK.API.PlayerPrefs.GetString(key, val);
#else
            return UnityEngine.PlayerPrefs.GetString(key, val);
#endif
        }
        public static bool Get(string key, bool val=false)
        {
            return Get(key, val?1:0) == 1;
        }
        public static int Get(string key, int val=0)
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetIntSync(key, val);
#elif UNITY_DY
            return StarkSDKSpace.StarkSDK.API.PlayerPrefs.GetInt(key, val);
#else
            return UnityEngine.PlayerPrefs.GetInt(key, val);
#endif
        }
        public static float Get(string key, float val=0)
        {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageGetFloatSync(key, val);
#elif UNITY_DY
            return StarkSDKSpace.StarkSDK.API.PlayerPrefs.GetFloat(key, val);
#else
            return UnityEngine.PlayerPrefs.GetFloat(key, val);
#endif
        }
        public static void DeleteKey(string key)
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageDeleteKeySync(key);
#elif UNITY_DY
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.DeleteKey(key);
#else
            UnityEngine.PlayerPrefs.DeleteKey(key);
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static void DeleteAllKey()
        {
#if UNITY_WX_WITHOUT_EDITOR
            WX.StorageDeleteAllSync();
#elif UNITY_DY
            StarkSDKSpace.StarkSDK.API.PlayerPrefs.DeleteAll();
#else
            UnityEngine.PlayerPrefs.DeleteAll();
            UnityEngine.PlayerPrefs.Save();
#endif
        }
        public static bool HasKey(string key) {
#if UNITY_WX_WITHOUT_EDITOR
            return WX.StorageHasKeySync(key);
#elif UNITY_DY
            return StarkSDKSpace.StarkSDK.API.PlayerPrefs.HasKey(key);
#else
            return UnityEngine.PlayerPrefs.HasKey(key);
#endif
        }
    }
}
