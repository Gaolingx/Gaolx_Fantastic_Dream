using HuHu;
using System.Xml.Linq;
using UnityEngine;

namespace DarkGod.Main
{
    public class NpcSvc : Singleton<NpcSvc>
    {
        public static ResSvc resSvc = null;

        #region Npc Data
        private class NpcTransform
        {
            public Vector3 Transform_NpcID_Position;
            public Vector3 Transform_NpcID_Rotation;
            public Vector3 Transform_NpcID_Scale;
        }

        #endregion

        protected override void Awake()
        {
            base.Awake();
        }

        public void InitCfg()
        {
            resSvc = ResSvc.MainInstance;
            PECommon.Log("Init NpcCfg...");
        }

        private NpcTransform BuildNpcTransform(NpcData npcData)
        {
            NpcTransform npcTransform = new NpcTransform();

            npcTransform.Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
            npcTransform.Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
            npcTransform.Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
            return npcTransform;
        }

        public async void LoadMapNpc(int npcType)
        {
            NpcData data = ConfigSvc.MainInstance.GetNpcCfg(npcType);
            NpcTransform npcTrans = BuildNpcTransform(data);
            await resSvc.LoadGameObjectAsync(Constants.ResourcePackgeName, data.npcResPath, npcTrans.Transform_NpcID_Position, npcTrans.Transform_NpcID_Rotation, npcTrans.Transform_NpcID_Scale, false, true, true);
        }

    }
}
