//功能：逻辑实体基类

using StarterAssets;
using UnityEngine.InputSystem;

public abstract class EntityBase
{
    //玩家当前状态
    public AniState currentAniState = AniState.None;

    public StateMgr stateMgr = null;
    public SkillMgr skillMgr = null;
    public ThirdPersonController playerController = null;
    public StarterAssetsInputs playerInput = null;
    public Controller controller = null;

    //玩家状态切换
    public void PlayerStateMove()
    {
        stateMgr.ChangeStatus(this, AniState.Move, null);
    }
    public void PlayerStateIdle()
    {
        stateMgr.ChangeStatus(this, AniState.Idle, null);
    }
    public void PlayerStateAttack(int skillID)
    {
        stateMgr.ChangeStatus(this, AniState.Attack, skillID);
    }

    public virtual void SetAniBlend(int blend)
    {
        if (playerController != null)
        {
            playerController.SetAniBlend(blend);
        }
    }

    public virtual void SetAction(int action, bool inputValues = true)
    {
        if (playerController != null)
        {
            playerController.SetAction(action, inputValues);
        }
    }
    public virtual void SetInputBool(int inputSkillID, bool inputValue = false)
    {
        if (playerInput != null)
        {
            switch (inputSkillID)
            {
                case 101:
                    playerInput.skill01 = inputValue;
                    break;
                default:
                    break;
            }
        }
    }

    public virtual void SetCFX(string fxName, float destroyTime)
    {
        if (playerController != null)
        {
            playerController.SetFX(fxName, destroyTime);
        }
    }

    public void AttackEffect(int skillID)
    {
        skillMgr.AttackEffect(this, skillID);
    }

}