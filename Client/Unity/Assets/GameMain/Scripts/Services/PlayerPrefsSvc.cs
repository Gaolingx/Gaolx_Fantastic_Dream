using UnityEngine;
using HuHu;
using XiHUtil;

namespace DarkGod.Main
{
    public class PlayerPrefsSvc : Singleton<PlayerPrefsSvc>
    {
        protected override void Awake()
        {
            base.Awake();
            Debug.Log("PlayerPrefsSvc Init Done.");
        }

        #region Login
        private const string PrefsKey_LoginAccount = "Login_Account";
        private const string PrefsKey_LoginPassword = "Login_Password";
        private const string PrefsKey_LoginRemember = "Login_RememberPass";

        public object GetLoginItem(string key)
        {
            bool isRemember;

            if (PlayerPrefsUtil.HasKey(PrefsKey_LoginRemember))
            {
                isRemember = PlayerPrefsUtil.Get(PrefsKey_LoginRemember, false);
            }
            else
            {
                isRemember = false;
            }

            switch (key)
            {
                case PrefsKey_LoginRemember:
                    return isRemember;
                case PrefsKey_LoginAccount:
                    if (PlayerPrefsUtil.HasKey(PrefsKey_LoginAccount) && isRemember)
                    {
                        return PlayerPrefsUtil.Get(PrefsKey_LoginAccount, "");
                    }
                    return "";
                case PrefsKey_LoginPassword:
                    if (PlayerPrefsUtil.HasKey(PrefsKey_LoginPassword) && isRemember)
                    {
                        return PlayerPrefsUtil.Get(PrefsKey_LoginPassword, "");
                    }
                    return "";
                default:
                    return null;
            }
        }

        #endregion

        #region Settings
        private const string PrefsKey_QualitySelect = "Settings_QualitySelect";
        private const string PrefsKey_BGAudioVolume = "Settings_BGAudioSlider";
        private const string PrefsKey_UIAudioVolume = "Settings_UIAudioSlider";
        private const string PrefsKey_CharacterAudioVolume = "Settings_CharacterAudioSlider";
        private const string PrefsKey_CharacterFxAudioVolume = "Settings_CharacterFxAudioSlider";

        public object GetSettingsItem(string key)
        {
            switch (key)
            {
                case PrefsKey_QualitySelect:
                    if (PlayerPrefsUtil.HasKey(PrefsKey_QualitySelect))
                    {
                        return PlayerPrefsUtil.Get(PrefsKey_QualitySelect, 0);
                    }
                    return QualitySettings.GetQualityLevel();
                case PrefsKey_BGAudioVolume:
                    if (PlayerPrefsUtil.HasKey(PrefsKey_BGAudioVolume))
                    {
                        return PlayerPrefsUtil.Get(PrefsKey_BGAudioVolume, 0f);
                    }
                    return AudioSvc.MainInstance.BGAudioVolumeValue;
                case PrefsKey_UIAudioVolume:
                    if (PlayerPrefsUtil.HasKey(PrefsKey_UIAudioVolume))
                    {
                        return PlayerPrefsUtil.Get(PrefsKey_UIAudioVolume, 0f);
                    }
                    return AudioSvc.MainInstance.UIAudioVolumeValue;
                case PrefsKey_CharacterAudioVolume:
                    if (PlayerPrefsUtil.HasKey(PrefsKey_CharacterAudioVolume))
                    {
                        return PlayerPrefsUtil.Get(PrefsKey_CharacterAudioVolume, 0f);
                    }
                    return AudioSvc.MainInstance.CharacterAudioVolumeValue;
                case PrefsKey_CharacterFxAudioVolume:
                    if (PlayerPrefsUtil.HasKey(PrefsKey_CharacterFxAudioVolume))
                    {
                        return PlayerPrefsUtil.Get(PrefsKey_CharacterFxAudioVolume, 0f);
                    }
                    return AudioSvc.MainInstance.CharacterFxAudioVolumeValue;
                default:
                    return null;
            }
        }

        #endregion
    }
}