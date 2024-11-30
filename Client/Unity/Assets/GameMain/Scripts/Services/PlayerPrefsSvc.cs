using DarkGod.Tools;
using HuHu;
using Newtonsoft.Json;
using UnityEngine;

namespace DarkGod.Main
{
    public class PlayerPrefsSvc : Singleton<PlayerPrefsSvc>
    {
        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitSvc(); };
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

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitSvc(); };
        }
    }
}