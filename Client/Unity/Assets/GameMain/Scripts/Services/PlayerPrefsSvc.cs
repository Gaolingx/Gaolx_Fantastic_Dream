using UnityEngine;
using HuHu;
using XiHUtil;
using Newtonsoft.Json;

namespace DarkGod.Main
{
    public class PlayerPrefsSvc : Singleton<PlayerPrefsSvc>
    {
        protected override void Awake()
        {
            base.Awake();

            GameRoot.MainInstance.OnGameEnter += InitSvc;
        }

        private void InitSvc()
        {
            PECommon.Log("PrefsSvc Init Done.");
        }

        public void SaveByPlayerPrefs(string key, object value)
        {
            var jsonStr = JsonConvert.SerializeObject(value);

            PlayerPrefsUtil.Set(key, jsonStr);
            PlayerPrefs.Save();
        }

        public string LoadFromPlayerPrefs(string key)
        {
            return PlayerPrefsUtil.Get(key, "");
        }

        public bool CheckPlayerPrefsHasKey(string key)
        {
            if (PlayerPrefsUtil.Get(key, "") == "" || PlayerPrefsUtil.Get(key, "") == null)
            {
                return false;
            }
            return true;
        }

        private void OnDestroy()
        {
            GameRoot.MainInstance.OnGameEnter -= InitSvc;
        }
    }
}