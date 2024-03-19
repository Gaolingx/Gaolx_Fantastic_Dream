//功能：配置数据类
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuyCfg : BaseData<BuyCfg>
{
    public int buyCostDiamondOnce;
    public int amountEachPurchase;

}

public class StrongCfg : BaseData<StrongCfg>
{
    public int pos;
    public int startlv;
    public int addhp;
    public int addhurt;
    public int adddef;
    public int minlv;
    public int coin;
    public int crystal;
}

public class AutoGuideCfg : BaseData<AutoGuideCfg>
{
    public int npcID; //触发任务目标NPC索引号
    public string dilogArr; //对话数据
    public int actID; //目标任务ID
    public int coin; //奖励的金币
    public int exp; //经验

}

public class MapCfg : BaseData<MapCfg>
{
    public string mapName; //地图名称
    public string sceneName; //场景名称
    public Vector3 mainCamPos; //相机位置
    public Vector3 mainCamRote; //相机旋转
    public Vector3 playerBornPos; //玩家出生位置
    public Vector3 playerBornRote;
}

//任务奖励配置
public class TaskRewardCfg : BaseData<TaskRewardCfg>
{
    public string taskName;
    public int count;
    public int exp;
    public int coin;
}

//子任务状态（进度/是否被领取）
public class TaskRewardData : BaseData<TaskRewardData>
{
    public int prgs;
    public bool taked;
}

public class NpcData : BaseData<NpcData>
{
    public float NPC_Transform_Position_X;
    public float NPC_Transform_Position_Y;
    public float NPC_Transform_Position_Z;
    public float NPC_Transform_Rotation_X;
    public float NPC_Transform_Rotation_Y;
    public float NPC_Transform_Rotation_Z;
    public float NPC_Transform_Scale_X;
    public float NPC_Transform_Scale_Y;
    public float NPC_Transform_Scale_Z;
}

public class BaseData<T>
{
    public int ID;
}

