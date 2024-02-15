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
    public Vector3 Transform_NpcID_0_Position = new Vector3((float)-20.1362114, (float)-0.615053654, (float)66.0065002);
    public Vector3 Transform_NpcID_0_Rotation = new Vector3(0, -180, 0);
    public Vector3 Transform_NpcID_0_Scale = new Vector3(1, 1, 1);
    public Vector3 Transform_NpcID_1_Position = new Vector3((float)-126.038147, (float)24.9887524, (float)237.177338);
    public Vector3 Transform_NpcID_1_Rotation = new Vector3(0, 140, 0);
    public Vector3 Transform_NpcID_1_Scale = new Vector3(1, 1, 1);
    public Vector3 Transform_NpcID_2_Position = new Vector3((float)-60.0505524, (float)2.59646082, (float)97.9177856);
    public Vector3 Transform_NpcID_2_Rotation = new Vector3(0, 75, 0);
    public Vector3 Transform_NpcID_2_Scale = new Vector3(1, 1, 1);
    public Vector3 Transform_NpcID_3_Position = new Vector3((float)-70.5333099, (float)-0.279643297, (float)21.70224);
    public Vector3 Transform_NpcID_3_Rotation = new Vector3(0, 60, 0);
    public Vector3 Transform_NpcID_3_Scale = new Vector3(1, 1, 1);
}
