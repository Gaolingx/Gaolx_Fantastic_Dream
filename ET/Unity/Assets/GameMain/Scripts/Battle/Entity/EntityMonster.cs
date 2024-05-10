//功能：怪物逻辑实体


using UnityEngine;

public class EntityMonster : EntityBase
{
    public MonsterData md;

    private float checkCountTime = 0f;

    //考虑等级影响，覆盖父类SetBattleProps默认方法
    public override void SetBattleProps(BattleProps props)
    {
        int level = md.mLevel;

        //计算怪物属性，与等级相关
        BattleProps p = new BattleProps
        {
            hp = props.hp * level,
            ad = props.ad * level,
            ap = props.ap * level,
            addef = props.addef * level,
            apdef = props.apdef * level,
            dodge = props.dodge * level,
            pierce = props.pierce * level,
            critical = props.critical * level
        };

        Props = p;
        HP = p.hp;
    }

    bool runAI = true;
    public override void TickAILogic()
    {
        float delta = Time.deltaTime;
        checkCountTime += delta;
        if (checkCountTime < Constants.MonsterCheckTime)
        {
            return;
        }
        else
        {
            //找到玩家，并攻击
            //1.计算目标方向
            Vector2 dir = CalcTargetDir();

        }
    }

    public override Vector2 CalcTargetDir()
    {
        EntityPlayer entityPlayer = GameRoot.Instance.GetCurrentPlayer();
        if (entityPlayer == null || entityPlayer.currentAniState == AniState.Die)
        {
            runAI = false;
            return Vector2.zero;
        }
        else
        {
            Vector3 target = entityPlayer.GetPos();
            Vector3 self = this.GetPos();
            return new Vector2(target.x - self.x, target.z - self.z).normalized;
        }
    }
}

