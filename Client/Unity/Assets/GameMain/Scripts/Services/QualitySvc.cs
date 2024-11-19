// 功能：质量管理器

using HuHu;
using Newtonsoft.Json;
using UnityEngine;

namespace DarkGod.Main
{
    public class QualitySvc : Singleton<QualitySvc>
    {
        private const string prefsKey_SettingsGameRoot = "prefsKey_SettingsGameRoot";

        protected override void Awake()
        {
            base.Awake();

            EventMgr.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        [System.Serializable]
        public class PlayerPrefsData
        {
            public int QualityLevel;
        }

        private void InitSvc()
        {
            LoadPrefsData();
        }

        public void LoadPrefsData()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsGameRoot))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsGameRoot);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);

                QualitySettings.SetQualityLevel(saveData.QualityLevel);
            }
        }

        public void SavePrefsData(PlayerPrefsData saveData)
        {
            PlayerPrefsSvc.MainInstance.SaveByPlayerPrefs(prefsKey_SettingsGameRoot, saveData);

            QualitySettings.SetQualityLevel(saveData.QualityLevel);
        }

        public void ApplyCurrentQualityLevel(PlayerPrefsData saveData)
        {
            QualitySettings.SetQualityLevel(saveData.QualityLevel);
        }

        private void OnDisable()
        {
            EventMgr.MainInstance.OnGameEnter -= delegate { InitSvc(); };
        }
    }
}
