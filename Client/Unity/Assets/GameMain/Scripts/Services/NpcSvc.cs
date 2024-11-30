using HuHu;
using UnityEngine;

namespace DarkGod.Main
{
    public class NpcSvc : Singleton<NpcSvc>
    {
        public static ResSvc resSvc = null;

        protected override void Awake()
        {
            base.Awake();

            GameStateEvent.MainInstance.OnGameEnter += delegate { InitSvc(); };
        }

        public void InitSvc()
        {
            resSvc = ResSvc.MainInstance;
            PECommon.Log("Init NpcCfg...");
        }

        public async void LoadMapNpc(int npcType)
        {
            NpcData data = ConfigSvc.MainInstance.GetNpcCfg(npcType);
            await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, data.npcResPath, data.NPC_Transform_Position, Quaternion.Euler(data.NPC_Transform_Rotation), data.NPC_Transform_Scale, true, false, false);
        }

        private void OnDisable()
        {
            GameStateEvent.MainInstance.OnGameEnter -= delegate { InitSvc(); };
        }
    }
}
