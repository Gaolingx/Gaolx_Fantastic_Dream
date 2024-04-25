//���ܣ��߼�ʵ�����

using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

public abstract class EntityBase
{
    //��ҵ�ǰ״̬
    public AniState currentAniState = AniState.None;

    public BattleMgr battleMgr = null;
    public StateMgr stateMgr = null;
    public SkillMgr skillMgr = null;
    public ThirdPersonController playerController = null;
    public StarterAssetsInputs playerInput = null;
    protected Controller controller = null;

    private string name;
    public string Name { get { return name; } set { name = value; } }
    private InputActionAsset _inputActionAsset;
    private InputActionMap _player;

    public bool canControl = true;

    private BattleProps props;
    public BattleProps Props { get { return props; } protected set { props = value; } } //ֻ���ڼ̳������������޸�

    private int hp; //ս���е�hp
    public int HP
    {
        get
        {
            return hp;
        }

        set
        {
            PECommon.Log("hp change:" + hp + " to " + value);
            //֪ͨUI��
            SetHPVal(hp, value); //�����������ط�����
            hp = value;
        }
    }

    //�ö��д洢���ж�Ӧ�ļ���id�����ͷ���˴��չ��󣬼���Ƿ������һ�μ���id��
    public Queue<int> comboQue = new Queue<int>();
    public int nextSkillID = 0;

    public SkillCfg curtSkillCfg; //��ǰ����ʩ�ż��ܵ�id

    //״̬�л�
    public void StateBorn()
    {
        stateMgr.ChangeStatus(this, AniState.Born, null);
    }
    public void StateMove()
    {
        stateMgr.ChangeStatus(this, AniState.Move, null);
    }
    public void StateIdle()
    {
        stateMgr.ChangeStatus(this, AniState.Idle, null);
    }
    public void StateAttack(int skillID)
    {
        stateMgr.ChangeStatus(this, AniState.Attack, skillID);
    }
    public void StateHit()
    {
        stateMgr.ChangeStatus(this, AniState.Hit, null);
    }
    public void StateDie()
    {
        stateMgr.ChangeStatus(this, AniState.Die, null);
    }

    public void SetCtrl(Controller ctrl)
    {
        controller = ctrl;
    }
    public void SetActive(bool active = true)
    {
        if (controller != null)
        {
            controller.gameObject.SetActive(active);
        }
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
            }
            else
            {
                _player.Enable();
            }
        }
    }

    public virtual void SetBattleProps(BattleProps props)
    {
        HP = props.hp;
        Props = props;
    }

    public virtual void SetAniBlend(int blend)
    {
        if (playerController != null)
        {
            playerController.SetAniBlend(blend);
        }
    }
    public virtual void SetDir(Vector2 dir)
    {
        if (controller != null)
        {
            controller.Dir = dir;
        }
        if (playerController != null)
        {
            PlayerCanControl();
        }
    }
    public virtual void SetAction(int action, bool inputValues = true)
    {
        if (playerController != null)
        {
            playerController.SetAction(action, inputValues);
        }
        if (controller != null)
        {
            controller.SetAction(action);
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
    public virtual void SetAtkRotation(Vector2 dir)
    {
        if (controller != null)
        {
            controller.SetAtkRotationLocal(dir);
        }
        if (playerController != null)
        {
            playerController.SetAtkRotationLocal(dir);
        }
    }

    public virtual void SetDodge()
    {
        if (controller != null)
        {
            GameRoot.Instance.dynamicWnd.SetDodge(Name);
        }
    }
    public virtual void SetCritical(int critical)
    {
        if (controller != null)
        {
            GameRoot.Instance.dynamicWnd.SetCritical(Name, critical);
        }
    }
    public virtual void SetHurt(int hurt)
    {
        if (controller != null)
        {
            GameRoot.Instance.dynamicWnd.SetHurt(Name, hurt);
        }
    }
    public virtual void SetHPVal(int oldval, int newval)
    {
        if (controller != null)
        {
            GameRoot.Instance.dynamicWnd.SetHPVal(Name, oldval, newval);
        }
    }

    public virtual void SkillAttack(int skillID)
    {
        skillMgr.SkillAttack(this, skillID);
    }

    public virtual Vector2 GetDirInput()
    {
        return Vector2.zero;
    }

    public virtual Vector3 GetPos()
    {
        if (controller != null)
        {
            return controller.transform.position;
        }
        if (playerController != null)
        {
            return playerController.transform.position;
        }

        return Vector3.zero;
    }

    public virtual Transform GetTrans()
    {
        if (controller != null)
        {
            return controller.transform;
        }
        if (playerController != null)
        {
            return playerController.transform;
        }

        return null;
    }

    public AnimationClip[] GetAniClips()
    {
        if (controller != null)
        {
            return controller.ani.runtimeAnimatorController.animationClips;
        }
        return null;
    }

    public virtual Vector2 CalcTargetDir()
    {
        return Vector2.zero;
    }

    //����˼·�������չ�ʱ��д����С�����ͨ������ɺ��˳�Attack״̬ʱ��⣬�жϴ洢�������ݵĶ������Ƿ������ݣ�
    //����ȡ��һ��skillID��ֵ��nextSkillID������Idle״̬ʱ�����nextSkillID��Ϊ�㣬���빥��״̬���ͷ���һ������
    public void ExitCurtSkill()
    {
        canControl = true;
        PlayerCanControl();

        //�������ݸ���
        if (curtSkillCfg.isCombo)
        {
            if (comboQue.Count > 0)
            {
                nextSkillID = comboQue.Dequeue();
            }
            else
            {
                nextSkillID = 0;
            }
        }
        SetAction(Constants.ActionDefault);
    }
}