//功能：逻辑实体基类

using StarterAssets;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public abstract class EntityBase
{
    //玩家当前状态
    public AniState currentAniState = AniState.None;

    public StateMgr stateMgr = null;
    public SkillMgr skillMgr = null;
    public ThirdPersonController playerController = null;
    public StarterAssetsInputs playerInput = null;
    public Controller controller = null;

    private InputActionAsset _inputActionAsset;
    private InputActionMap _player;

    public bool canControl = true;

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
    public void PlayerCanControl()
    {
        _inputActionAsset = GameRoot.Instance.GetEventSystemObject(Constants.EventSystemGOName).GetComponent<InputSystemUIInputModule>().actionsAsset;
        _player = _inputActionAsset.FindActionMap("Player");
        if (_player != null)
        {
            if (canControl != true)
            {
                _player.Disable();
                playerInput.move = Vector2.zero;
            }
            else
            {
                _player.Enable();
            }
        }
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
                case Constants.SkillID_Mar7th00_skill01:
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
    public virtual void SetSkillMoveState(bool move, float speed = 0f)
    {
        if (playerController != null)
        {
            playerController.SetSkillMove(move, speed);
        }
    }

    public virtual void AttackEffect(int skillID)
    {
        skillMgr.AttackEffect(this, skillID);
    }

}