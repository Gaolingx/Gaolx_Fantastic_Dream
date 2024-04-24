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

public class MonsterCfg : BaseData<MonsterCfg>
{
    public string mName; //怪物名字
    public string resPath; //资源路径
    public BattleProps bps;
}

public class MonsterData : BaseData<MonsterData>
{
    public int mWave;//批次
    public int mIndex;//序号
    public MonsterCfg mCfg;
    public Vector3 mBornPos;
    public Vector3 mBornRote;
    public int mLevel;
}

public class MapCfg : BaseData<MapCfg>
{
    public string mapName; //地图名称
    public string sceneName; //场景名称
    public int power; //进入关卡消耗的体力
    public Vector3 mainCamPos; //相机位置
    public Vector3 mainCamRote; //相机旋转
    public Vector3 playerBornPos; //玩家出生位置
    public Vector3 playerBornRote;
    public List<MonsterData> monsterLst;
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
    public string npcResPath;
    public string npcName;
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

public class SkillMoveCfg : BaseData<SkillMoveCfg>
{
    public int delayTime;
    public int moveTime;
    public float moveDis;
}

public class SkillActionCfg : BaseData<SkillActionCfg>
{
    public int delayTime;
    public float radius; //伤害计算范围
    public int angle; //伤害有效角度
}

public class SkillCfg : BaseData<SkillCfg>
{
    public string skillName;
    public int cdTime;
    public int skillTime; //技能持续时间
    public int aniAction; //动画控制参数
    public string fx; //特效名称
    public bool isCombo; //是否连招
    public DamageType dmgType; //伤害类型
    public List<int> skillMoveLst;
    public List<int> skillActionLst;
    public List<int> skillDamageLst;
}


public class BaseData<T>
{
    public int ID;
}

//战斗数值属性
public class BattleProps
{
    public int hp;
    public int ad;
    public int ap;
    public int addef;
    public int apdef;
    public int dodge;
    public int pierce;
    public int critical;
}

