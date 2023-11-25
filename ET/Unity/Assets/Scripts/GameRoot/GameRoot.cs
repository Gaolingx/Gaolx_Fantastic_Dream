using UnityEngine;

namespace GameMain.Scripts
{
    public partial class GameRoot : MonoBehaviour
    {
        public static GameRoot Instance = null;
        private async void Awake()
        {
            Instance = this;

            InitUI();

            var oldName = gameObject.name;
            gameObject.name = "ApplicationManager";

            if (await CheckResource())
            {
                return;
            }

            gameObject.name = oldName;
            await LoadGameDll();
            await RunDll();
        }
    }
}
