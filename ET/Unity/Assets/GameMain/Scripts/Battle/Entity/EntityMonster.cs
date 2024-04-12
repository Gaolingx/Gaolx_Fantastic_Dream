//功能：怪物逻辑实体


public class EntityMonster : EntityBase
{
    public MonsterData md;

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
}

