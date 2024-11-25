// 功能：质量管理器

using HuHu;
using Newtonsoft.Json;
using UnityEngine;

namespace DarkGod.Main
{
    public class SoundVolume
    {
        public BindableProperty<float> BGAudioVolumeValue { get; set; } = new BindableProperty<float>();
        public BindableProperty<float> UIAudioVolumeValue { get; set; } = new BindableProperty<float>();
        public BindableProperty<float> CharacterAudioVolumeValue { get; set; } = new BindableProperty<float>();
        public BindableProperty<float> CharacterFxAudioVolumeValue { get; set; } = new BindableProperty<float>();
    }

    public class ScreenPrefsData
    {
        public GraphicsType graphicsType;
        public int targetFrameRate;
        public (int, int) resolution;
        public FullScreenMode fullScreenMode;
    }

    public class QualitySvc : Singleton<QualitySvc>
    {
        public SoundVolume volume { get; private set; }
        public ScreenPrefsData screen { get; private set; }

        private const string prefsKey_SettingsGameRoot = "prefsKey_SettingsGameRoot";
        private const string prefsKey_SettingsAudioSvc = "prefsKey_SettingsAudioSvc";

        private void InitScreenSetting()
        {
            screen.graphicsType = GetGraphicsType();
            QualitySettings.SetQualityLevel((int)screen.graphicsType);

            screen.targetFrameRate = GetFrames();
            Application.targetFrameRate = screen.targetFrameRate;

            screen.resolution = GetResolution();
            screen.fullScreenMode = GetWindowType();
            Screen.SetResolution(screen.resolution.Item1, screen.resolution.Item2, screen.fullScreenMode);

            GameRoot.MainInstance.GetUIController().CursorLock = false;
        }

        private void InitVolumeData()
        {
            if (PlayerPrefsSvc.MainInstance.CheckPlayerPrefsHasKey(prefsKey_SettingsAudioSvc))
            {
                var json = PlayerPrefsSvc.MainInstance.LoadFromPlayerPrefs(prefsKey_SettingsAudioSvc);
                var saveData = JsonConvert.DeserializeObject<PlayerPrefsData2>(json);

                volume.BGAudioVolumeValue.Value = saveData.BGAudioVolume;
                volume.UIAudioVolumeValue.Value = saveData.UIAudioVolume;
                volume.CharacterAudioVolumeValue.Value = saveData.CharacterAudioVolume;
                volume.CharacterFxAudioVolumeValue.Value = saveData.CharacterFxAudioVolume;
            }
            else
            {
                volume.BGAudioVolumeValue.Value = 0.25f;
                volume.UIAudioVolumeValue.Value = 0.5f;
                volume.CharacterAudioVolumeValue.Value = 0.5f;
                volume.CharacterFxAudioVolumeValue.Value = 0.5f;
            }
        }

        public void QualitySetting()
        {
            InitScreenSetting();
            saveAction += delegate (PlayerPrefsData data) { OnUpdatePrefsData(data); };
        }

        public void VolumeSetting()
        {
            InitVolumeData();
            saveAction2 += delegate (PlayerPrefsData2 data) { OnUpdatePrefsData2(data); };
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

        protected override void Awake()
        {
            base.Awake();

            EventMgr.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

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

        private System.Action<PlayerPrefsData> saveAction;
        private System.Action<PlayerPrefsData2> saveAction2;

        private void InitSvc()
        {
            volume = new();
            screen = new();
            QualitySetting();
            VolumeSetting();
        }

        private void OnUpdatePrefsData(PlayerPrefsData saveData)
        {
            PlayerPrefsSvc.MainInstance.SaveByPlayerPrefs(prefsKey_SettingsGameRoot, saveData);

            QualitySettings.SetQualityLevel((int)saveData.graphicsType);
            Application.targetFrameRate = saveData.targetFrameRate;
            (int, int) resolution = saveData.resolution;
            Screen.SetResolution(resolution.Item1, resolution.Item2, saveData.fullScreenMode);
        }

        private static void OnUpdatePrefsData2(PlayerPrefsData2 saveData)
        {
            PlayerPrefsSvc.MainInstance.SaveByPlayerPrefs(prefsKey_SettingsAudioSvc, saveData);
        }

        public void SavePlayerData()
        {
            PlayerPrefsData data = new PlayerPrefsData();
            data.graphicsType = screen.graphicsType;
            data.targetFrameRate = screen.targetFrameRate;
            data.resolution = screen.resolution;
            data.fullScreenMode = screen.fullScreenMode;

            saveAction?.Invoke(data);
        }

        public void SavePlayerData2()
        {
            PlayerPrefsData2 data = new PlayerPrefsData2();
            data.BGAudioVolume = volume.BGAudioVolumeValue.Value;
            data.UIAudioVolume = volume.UIAudioVolumeValue.Value;
            data.CharacterAudioVolume = volume.CharacterAudioVolumeValue.Value;
            data.CharacterFxAudioVolume = volume.CharacterFxAudioVolumeValue.Value;

            saveAction2?.Invoke(data);
        }

        private void OnDisable()
        {
            EventMgr.MainInstance.OnGameEnter -= delegate { InitSvc(); };
            saveAction -= delegate (PlayerPrefsData data) { OnUpdatePrefsData(data); };
            saveAction2 -= delegate (PlayerPrefsData2 data) { OnUpdatePrefsData2(data); };
        }
    }
}
