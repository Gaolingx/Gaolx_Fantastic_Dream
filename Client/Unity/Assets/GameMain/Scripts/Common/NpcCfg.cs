using System.Xml.Linq;
using UnityEngine;

namespace DarkGod.Main
{
    public class NpcCfg : MonoBehaviour
    {
        public static NpcCfg Instance = null;
        public static ResSvc resSvc = null;

        #region Npc Data
        private class NpcTransform
        {
            public Vector3 Transform_NpcID_Position;
            public Vector3 Transform_NpcID_Rotation;
            public Vector3 Transform_NpcID_Scale;
        }

        #endregion

        public void InitCfg()
        {
            Instance = this;
            resSvc = ResSvc.MainInstance;
            PECommon.Log("Init NpcCfg...");
        }

        private NpcData GetNpcCfgFromXml(int npcType)
        {
            NpcData npcData = ResSvc.MainInstance.GetNpcCfg(npcType);
            return npcData;
        }

        private NpcTransform BuildNpcTransform(NpcData npcData)
        {
            NpcTransform npcTransform = new NpcTransform();

            npcTransform.Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
            npcTransform.Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
            npcTransform.Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
            return npcTransform;
        }

        //NPC配置
        private NpcTransform GetNpcTrans(NpcData npcData, int NpcType) => NpcType switch
        {

            Constants.NpcTypeID_0 => BuildNpcTransform(npcData),
            Constants.NpcTypeID_1 => BuildNpcTransform(npcData),
            Constants.NpcTypeID_2 => BuildNpcTransform(npcData),
            Constants.NpcTypeID_3 => BuildNpcTransform(npcData),
            _ => null,

        };

        public async void LoadMapNpc(int NpcType)
        {
            NpcData data = GetNpcCfgFromXml(NpcType);
            NpcTransform npcTrans = GetNpcTrans(data, NpcType);
            await resSvc.LoadGameObjectAsync(data.npcResPath, npcTrans.Transform_NpcID_Position, npcTrans.Transform_NpcID_Rotation, npcTrans.Transform_NpcID_Scale);
        }

    }
}
