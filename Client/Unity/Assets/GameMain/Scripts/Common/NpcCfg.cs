using UnityEngine;

namespace DarkGod.Main
{
    public class NpcCfg : MonoBehaviour
    {
        public static NpcCfg Instance = null;
        public static ResSvc resSvc = null;

        #region Npc Data
        private string ResPath;
        private Vector3 Transform_NpcID_Position;
        private Vector3 Transform_NpcID_Rotation;
        private Vector3 Transform_NpcID_Scale;

        #endregion
        private NpcData npcData;

        public void InitCfg()
        {
            Instance = this;
            resSvc = ResSvc.Instance;
            PECommon.Log("Init NpcCfg...");
        }

        private void InitAllNpcTransformData()
        {
            Transform_NpcID_Position = Vector3.zero;
            Transform_NpcID_Rotation = Vector3.zero;
            Transform_NpcID_Scale = Vector3.one;
        }

        private void GetNpcCfgFromXml(int npcType)
        {
            npcData = ResSvc.Instance.GetNpcCfg(npcType);
        }


        //NPC配置
        private void GetNpcTrans(int NpcType)
        {
            InitAllNpcTransformData();

            switch (NpcType)
            {
                case Constants.NpcTypeID_0:
                    ResPath = npcData.npcResPath;
                    Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                    Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
                    Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                    break;
                case Constants.NpcTypeID_1:
                    ResPath = npcData.npcResPath;
                    Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                    Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
                    Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                    break;
                case Constants.NpcTypeID_2:
                    ResPath = npcData.npcResPath;
                    Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                    Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
                    Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                    break;
                case Constants.NpcTypeID_3:
                    ResPath = npcData.npcResPath;
                    Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                    Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
                    Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                    break;
                default:
                    PECommon.Log("NPC Type dose not exist. NpcType:" + NpcType, PELogType.Error);
                    break;
            }
        }

        public async void LoadMapNpc(int NpcType)
        {
            GetNpcCfgFromXml(NpcType);
            GetNpcTrans(NpcType);
            await resSvc.LoadGameObjectAsync(ResPath, Transform_NpcID_Position, Transform_NpcID_Rotation, Transform_NpcID_Scale);
        }

    }
}
