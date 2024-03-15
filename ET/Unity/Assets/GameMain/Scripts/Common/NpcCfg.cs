using UnityEngine;

public class NpcCfg : MonoBehaviour
{
    public static NpcCfg Instance = null;

    public void InitCfg()
    {
        Instance = this;
        PECommon.Log("Init NpcCfg...");
    }

    //NPC配置
    public Vector3 Transform_NpcID_0_Position = new Vector3(135.0f, 9.9981689453125f, 140.0f);
    public Vector3 Transform_NpcID_0_Rotation = new Vector3(0.0f, 135.0f, 0.0f);
    public Vector3 Transform_NpcID_0_Scale = new Vector3(1.0f, 1.0f, 1.0f);

    public Vector3 Transform_NpcID_1_Position = new Vector3(136.5f, 9.998172760009766f, 175.0f);
    public Vector3 Transform_NpcID_1_Rotation = new Vector3(0.0f, 160.237f, 0.0f);
    public Vector3 Transform_NpcID_1_Scale = new Vector3(1.0f, 1.0f, 1.0f);

    public Vector3 Transform_NpcID_2_Position = new Vector3(154.52000427246095f, 9.998165130615235f, 160.85000610351563f);
    public Vector3 Transform_NpcID_2_Rotation = new Vector3(0.0f, -140.911f, 0.0f);
    public Vector3 Transform_NpcID_2_Scale = new Vector3(1.0f, 1.0f, 1.0f);

    public Vector3 Transform_NpcID_3_Position = new Vector3(148.24000549316407f, 9.9981689453125f, 177.60000610351563f);
    public Vector3 Transform_NpcID_3_Rotation = new Vector3(0.0f, -150.0f, 0.0f);
    public Vector3 Transform_NpcID_3_Scale = new Vector3(1.0f, 1.0f, 1.0f);
}
