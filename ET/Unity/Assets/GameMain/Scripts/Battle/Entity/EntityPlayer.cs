//功能：玩家逻辑实体


using UnityEngine;

public class EntityPlayer : EntityBase
{
    public override Vector2 GetDirInput()
    {
        return battleMgr.GetDirInput();
    }
}

