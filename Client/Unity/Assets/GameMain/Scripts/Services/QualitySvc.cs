// 功能：质量管理器

using HuHu;
using Newtonsoft.Json;
using UnityEngine;

namespace DarkGod.Main
{
    public class QualitySvc : Singleton<QualitySvc>
    {
        private const string prefsKey_SettingsAudioSvc = "prefsKey_SettingsAudioSvc";
        private const string prefsKey_SettingsGameRoot = "prefsKey_SettingsGameRoot";
        private const string prefsKey_LoginWnd = "prefsKey_LoginWnd";


        [System.Serializable]
        public class PlayerPrefsData
        {
            public GraphicsType graphicsType;
            public int targetFrameRate;
            public (int, int) resolution;
            public FullScreenMode fullScreenMode;
        }

        [System.Serializable]
        public class PlayerPrefsData2
        {
            public float BGAudioVolume;
            public float UIAudioVolume;
            public float CharacterAudioVolume;
            public float CharacterFxAudioVolume;
        }

        [System.Serializable]
        public class PlayerPrefsData3
        {
            public bool isRemember;
            public string Login_Account;
            public string Login_Password;
        }

        private System.Action<PlayerPrefsData> saveAction;
        private System.Action<PlayerPrefsData2> saveAction2;
        private System.Action<PlayerPrefsData3> saveAction3;


        public PlayerPrefsData GetScreenSetting()
        {
            PlayerPrefsData screen = new PlayerPrefsData();

            screen.graphicsType = GetGraphicsType();
            screen.targetFrameRate = GetFrames();
            screen.resolution = GetResolution();
            screen.fullScreenMode = GetWindowType();

            return screen;
        }

        private static void InitScreenSetting(PlayerPrefsData data)
        {
            QualitySettings.SetQualityLevel((int)data.graphicsType);
            Application.targetFrameRate = data.targetFrameRate;
            Screen.SetResolution(data.resolution.Item1, data.resolution.Item2, data.fullScreenMode);
        }

        public void InitQualitySetting()
        {
            InitScreenSetting(GetScreenSetting());

            saveAction += delegate (PlayerPrefsData data) { OnUpdatePrefsData(data); };
            saveAction2 += delegate (PlayerPrefsData2 data) { OnUpdatePrefsData2(data); };
            saveAction3 += delegate (PlayerPrefsData3 data) { OnUpdatePrefsData3(data); };
        }

        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        private void InitSvc()
        {
            InitQualitySetting();
            PECommon.Log("Init QualitySvc...");
        }

        private static GraphicsType GetGraphicsType()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsGameRoot))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsGameRoot);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);
                return saveData.graphicsType;
            }

            if (SystemInfo.processorCount <= 2)
            {
                return GraphicsType.Low;
            }
            if (SystemInfo.systemMemorySize <= 4096)
            {
                return GraphicsType.Low;
            }
            switch (SystemInfo.graphicsDeviceVendorID)
            {
                case 4098:
                case 4318:
                    if (SystemInfo.graphicsMemorySize > 4096)
                    {
                        return GraphicsType.Ultra;
                    }
                    return GraphicsType.High;
                case 32902:
                    return GraphicsType.Middle;
                default:
                    return GraphicsType.Middle;
            }
        }

        private static (int, int) GetResolution()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsGameRoot))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsGameRoot);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);
                return saveData.resolution;
            }
            return GameRoot.MainInstance.GetUIController().ScreenResolution;
        }

        public static FullScreenMode GetWindowType()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsGameRoot))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsGameRoot);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);
                return saveData.fullScreenMode;
            }
            return GameRoot.MainInstance.GetUIController().FullScreen;
        }

        private static int GetFrames()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsGameRoot))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsGameRoot);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData>(json);
                return saveData.targetFrameRate;
            }
            return GameRoot.MainInstance.GetUIController().FrameRate;
        }


        private void OnUpdatePrefsData(PlayerPrefsData saveData)
        {
            PlayerPrefsSvc.MainInstance.SaveByPlayerPrefs(prefsKey_SettingsGameRoot, saveData);

            QualitySettings.SetQualityLevel((int)saveData.graphicsType);
            Application.targetFrameRate = saveData.targetFrameRate;
            (int, int) resolution = saveData.resolution;
            Screen.SetResolution(resolution.Item1, resolution.Item2, saveData.fullScreenMode);
        }

        private void OnUpdatePrefsData2(PlayerPrefsData2 saveData)
        {
            PlayerPrefsSvc.MainInstance.SaveByPlayerPrefs(prefsKey_SettingsAudioSvc, saveData);
        }

        private void OnUpdatePrefsData3(PlayerPrefsData3 saveData)
        {
            PlayerPrefsSvc.MainInstance.SaveByPlayerPrefs(prefsKey_LoginWnd, saveData);
        }


        public void SavePlayerData(PlayerPrefsData data)
        {
            saveAction?.Invoke(data);
        }

        public void SavePlayerData2(PlayerPrefsData2 data)
        {
            saveAction2?.Invoke(data);
        }

        public void SavePlayerData3(PlayerPrefsData3 data)
        {
            saveAction3?.Invoke(data);
        }

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitSvc(); };
            saveAction -= delegate (PlayerPrefsData data) { OnUpdatePrefsData(data); };
            saveAction2 -= delegate (PlayerPrefsData2 data) { OnUpdatePrefsData2(data); };
            saveAction3 -= delegate (PlayerPrefsData3 data) { OnUpdatePrefsData3(data); };
        }
    }
}
