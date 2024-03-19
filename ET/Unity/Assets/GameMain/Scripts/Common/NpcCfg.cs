using UnityEngine;

public class NpcCfg : MonoBehaviour
{
    public static NpcCfg Instance = null;
    public static ResSvc resSvc = null;

    #region Npc Data
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

    public void GetNpcCfgFromXml(int npcType)
    {
        npcData = ResSvc.Instance.GetNpcCfg(npcType);
    }


    //NPC配置
    private void GetNpcTrans(int NpcType)
    {
        switch (NpcType)
        {
            case Constants.NpcTypeID_0:
                Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y,npcData.NPC_Transform_Rotation_Z);
                Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                break;
            case Constants.NpcTypeID_1:
                Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
                Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                break;
            case Constants.NpcTypeID_2:
                Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
                Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                break;
            case Constants.NpcTypeID_3:
                Transform_NpcID_Position = new Vector3(npcData.NPC_Transform_Position_X, npcData.NPC_Transform_Position_Y, npcData.NPC_Transform_Position_Z);
                Transform_NpcID_Rotation = new Vector3(npcData.NPC_Transform_Rotation_X, npcData.NPC_Transform_Rotation_Y, npcData.NPC_Transform_Rotation_Z);
                Transform_NpcID_Scale = new Vector3(npcData.NPC_Transform_Scale_X, npcData.NPC_Transform_Scale_Y, npcData.NPC_Transform_Scale_Z);
                break;
            default:
                Debug.LogError("未成功加载指定类型的NPC，NPC类型：" + NpcType);
                break;
        }
    }

    public GameObject LoadMapNpc(int NpcType, string NpcPrefabPath)
    {
        GetNpcCfgFromXml(NpcType);
        GetNpcTrans(NpcType);
        GameObject NPC_GO = resSvc.LoadPrefab(NpcPrefabPath, true);
        GameRoot.Instance.GetGameObjectTrans(NPC_GO.gameObject, Transform_NpcID_Position, Transform_NpcID_Rotation, Transform_NpcID_Scale);

        Debug.Log("NPC预制件加载成功！" + " 类型：" + NpcType + " 路径：" + NpcPrefabPath);
        return NPC_GO;

    }

}
