namespace DarkGod.Main
{
//���ܣ�����������
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
    public int npcID; //��������Ŀ��NPC������
    public string dilogArr; //�Ի�����
    public int actID; //Ŀ������ID
    public int coin; //�����Ľ��
    public int exp; //����

}

public class MonsterCfg : BaseData<MonsterCfg>
{
    public string mName; //��������
    public string resPath; //��Դ·��
    public int skillID;
    public float atkDis;
    public BattleProps bps;
}

public class MonsterData : BaseData<MonsterData>
{
    public int mWave;//����
    public int mIndex;//���
    public MonsterCfg mCfg;
    public Vector3 mBornPos;
    public Vector3 mBornRote;
    public int mLevel;
    public float mMoveSpeed;
}

public class MapCfg : BaseData<MapCfg>
{
    public string mapName; //��ͼ����
    public string sceneName; //��������
    public int power; //����ؿ����ĵ�����
    public Vector3 mainCamPos; //���λ��
    public Vector3 mainCamRote; //�����ת
    public Vector3 playerBornPos; //��ҳ���λ��
    public Vector3 playerBornRote;
    public List<MonsterData> monsterLst;
}

//����������
public class TaskRewardCfg : BaseData<TaskRewardCfg>
{
    public string taskName;
    public int count;
    public int exp;
    public int coin;
}

//������״̬������/�Ƿ���ȡ��
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
    public float radius; //�˺����㷶Χ
    public int angle; //�˺���Ч�Ƕ�
}

public class SkillCfg : BaseData<SkillCfg>
{
    public string skillName;
    public int cdTime;
    public int skillTime; //���ܳ���ʱ��
    public int aniAction; //�������Ʋ���
    public string fx; //��Ч����
    public bool isCombo; //�Ƿ�����
    public bool isCollide; //������ײ
    public bool isBreak; //�Ƿ�ɱ��ж�
    public DamageType dmgType; //�˺�����
    public List<int> skillMoveLst;
    public List<int> skillActionLst;
    public List<int> skillDamageLst;
}


public class BaseData<T>
{
    public int ID;
}

//ս����ֵ����
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


}