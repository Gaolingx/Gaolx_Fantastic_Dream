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

    public void InitCfg()
    {
        Instance = this;
        resSvc = ResSvc.Instance;
        PECommon.Log("Init NpcCfg...");
    }

    //NPC配置
    private void GetNpcCfg(int NpcType)
    {
        switch (NpcType)
        {
            case 0:
                Transform_NpcID_Position = new Vector3(135.0f, 9.9981689453125f, 140.0f);
                Transform_NpcID_Rotation = new Vector3(0.0f, 135.0f, 0.0f);
                Transform_NpcID_Scale = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case 1:
                Transform_NpcID_Position = new Vector3(136.5f, 9.998172760009766f, 175.0f);
                Transform_NpcID_Rotation = new Vector3(0.0f, 160.237f, 0.0f);
                Transform_NpcID_Scale = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case 2:
                Transform_NpcID_Position = new Vector3(154.52000427246095f, 9.998165130615235f, 160.85000610351563f);
                Transform_NpcID_Rotation = new Vector3(0.0f, -140.911f, 0.0f);
                Transform_NpcID_Scale = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case 3:
                Transform_NpcID_Position = new Vector3(148.24000549316407f, 9.9981689453125f, 177.60000610351563f);
                Transform_NpcID_Rotation = new Vector3(0.0f, -150.0f, 0.0f);
                Transform_NpcID_Scale = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            default:
                Debug.LogError("未成功加载指定类型的NPC，NPC类型：" + NpcType);
                break;
        }
    }

    public GameObject LoadMapNpc(int NpcType, string NpcPrefabPath)
    {
        GetNpcCfg(NpcType);
        GameObject NPC_GO = resSvc.LoadPrefab(NpcPrefabPath, true);
        GameRoot.Instance.GetGameObjectTrans(NPC_GO.gameObject, Transform_NpcID_Position, Transform_NpcID_Rotation, Transform_NpcID_Scale);

        Debug.Log("NPC预制件加载成功！" + " 类型：" + NpcType + " 路径：" + NpcPrefabPath);
        return NPC_GO;

    }

}
