using UnityEngine;
using HuHu;

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
        private string PrefsKeyLoginAccount = "Login_Account";
        private string PrefsKeyLoginPassword = "Login_Password";
        private string PrefsKeyLoginRemember = "Login_RememberPass";

        public class LoginItem
        {
            public string account;
            public string password;
            public bool isRemember;
        }

        public LoginItem GetLoginItem()
        {
            LoginItem loginItem = new LoginItem();

            if (PlayerPrefs.HasKey(PrefsKeyLoginRemember))
            {
                loginItem.isRemember = UIItemUtils.IntToBool(PlayerPrefs.GetInt(PrefsKeyLoginRemember));
            }
            else
            {
                loginItem.isRemember = false;
            }

            if (PlayerPrefs.HasKey(PrefsKeyLoginAccount) && PlayerPrefs.HasKey(PrefsKeyLoginPassword) && loginItem.isRemember == true)
            {
                loginItem.account = PlayerPrefs.GetString(PrefsKeyLoginAccount);
                loginItem.password = PlayerPrefs.GetString(PrefsKeyLoginPassword);
            }
            else
            {
                loginItem.account = "";
                loginItem.password = "";
            }

            return loginItem;
        }

        public void SetGetLoginItem(LoginItem loginItem)
        {
            PlayerPrefs.SetString(PrefsKeyLoginAccount, loginItem.account);
            PlayerPrefs.SetString(PrefsKeyLoginPassword, loginItem.password);
            PlayerPrefs.SetInt(PrefsKeyLoginRemember, UIItemUtils.BoolToInt(loginItem.isRemember));
        }

        #endregion
    }
}