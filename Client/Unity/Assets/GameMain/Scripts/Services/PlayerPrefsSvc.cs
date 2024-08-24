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

            if (PlayerPrefsUtil.HasKey(PrefsKeyLoginRemember))
            {
                loginItem.isRemember = PlayerPrefsUtil.Get(PrefsKeyLoginRemember, false);
            }
            else
            {
                loginItem.isRemember = false;
            }

            if (PlayerPrefsUtil.HasKey(PrefsKeyLoginAccount) && PlayerPrefsUtil.HasKey(PrefsKeyLoginPassword) && loginItem.isRemember == true)
            {
                loginItem.account = PlayerPrefsUtil.Get(PrefsKeyLoginAccount, "");
                loginItem.password = PlayerPrefsUtil.Get(PrefsKeyLoginPassword, "");
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
            PlayerPrefsUtil.Set(PrefsKeyLoginAccount, loginItem.account);
            PlayerPrefsUtil.Set(PrefsKeyLoginPassword, loginItem.password);
            PlayerPrefsUtil.Set(PrefsKeyLoginRemember, loginItem.isRemember);
        }

        #endregion
    }
}