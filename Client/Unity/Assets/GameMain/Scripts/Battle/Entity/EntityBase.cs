//���ܣ��߼�ʵ�����

using StarterAssets;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace DarkGod.Main
{
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

        private bool canControl = true;
        public bool CanControl { get { return canControl; } }

        private bool canRlsSkill = true;
        public bool CanRlsSkill { get { return canRlsSkill; } set { canRlsSkill = value; } }

        public EntityType entityType = EntityType.None;

        public EntityState entityState = EntityState.None;

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
                //PECommon.Log("hp change:" + hp + " to " + value);
                //֪ͨUI��
                SetHPVal(hp, value); //�����������ط�����
                hp = value;
            }
        }

        //�ö��д洢���ж�Ӧ�ļ���id�����ͷ���˴��չ��󣬼���Ƿ������һ�μ���id��
        public Queue<int> comboQue = new Queue<int>();
        public int nextSkillID = 0;

        public SkillCfg curtSkillCfg; //��ǰ����ʩ�ż��ܵ�id

        //����λ�ƵĻص�id
        public List<int> skMoveCBLst = new List<int>();
        //�����˺�����ص�id
        public List<int> skActionCBLst = new List<int>();

        //���ܽ����ص�
        public int skEndCB = -1;

        #region State Define
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
        #endregion

        #region AI Logic
        //����ai�߼�
        public virtual void TickAILogic()
        {

        }
        #endregion

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

        public void PlayerCanControl(bool cancontrol = true)
        {
            canControl = cancontrol;
        }

        public virtual void SetBattleProps(BattleProps props)
        {
            HP = props.hp;
            Props = props;
        }

        public virtual void SetAniBlend(int blend)
        {
            if (controller != null)
            {
                controller.SetBlend(blend);
            }
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
                playerController.SetDir(dir);
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

        public virtual void SetCFX(string fxName, float destroyTime, float volume)
        {
            if (playerController != null)
            {
                playerController.SetFX(fxName, destroyTime, volume);
            }
        }
        public virtual void SetSkillMoveState(bool move, float speed = 0f)
        {
            if (playerController != null)
            {
                playerController.SetSkillMove(move, speed);
            }
        }
        public virtual void SetAtkRotation(Vector2 dir, bool isOffset = false) //���������ƫ��
        {
            if (controller != null)
            {
                if (isOffset)
                {
                    controller.SetAtkRotationCam(dir);
                }
                else
                {
                    controller.SetAtkRotationLocal(dir);
                }
            }
            if (playerController != null)
            {
                playerController.SetAtkRotationLocal(dir);
            }
        }

        #region ս����Ϣ��ʾ
        public virtual void SetDodge()
        {
            if (controller != null || playerController != null)
            {
                GameRoot.Instance.dynamicWnd.SetDodge(Name);
            }
        }
        public virtual void SetCritical(int critical)
        {
            if (controller != null || playerController != null)
            {
                GameRoot.Instance.dynamicWnd.SetCritical(Name, critical);
            }
        }
        public virtual void SetHurt(int hurt)
        {
            if (controller != null || playerController != null)
            {
                GameRoot.Instance.dynamicWnd.SetHurt(Name, hurt);
            }
        }
        public virtual void SetHPVal(int oldval, int newval)
        {
            if (controller != null || playerController != null)
            {
                GameRoot.Instance.dynamicWnd.SetHPVal(Name, oldval, newval);
            }
        }
        #endregion

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
            if (playerController != null)
            {
                return playerController.GetAnimator().runtimeAnimatorController.animationClips;
            }
            return null;
        }

        public AudioSource GetAudioSource()
        {
            if (playerController != null)
            {
                return playerController.GetComponent<AudioSource>();
            }
            if (controller != null)
            {
                return controller.GetComponent<AudioSource>();
            }
            return null;
        }

        public CharacterController GetCharacterController()
        {
            if (playerController != null)
            {
                return playerController.GetComponent<CharacterController>();
            }
            if (controller != null)
            {
                return controller.GetComponent<CharacterController>();
            }
            return null;
        }

        //��ȡ��ǰ�����Ƿ�ɱ��ж�״̬
        public virtual bool GetBreakState()
        {
            return true;
        }

        public virtual void PlayHitAudio()
        {

        }

        public virtual Vector2 CalcTargetDir()
        {
            return Vector2.zero;
        }

        //����˼·�������չ�ʱ��д����С�����ͨ������ɺ��˳�Attack״̬ʱ��⣬�жϴ洢�������ݵĶ������Ƿ������ݣ�
        //����ȡ��һ��skillID��ֵ��nextSkillID������Idle״̬ʱ�����nextSkillID��Ϊ�㣬���빥��״̬���ͷ���һ������

        //�˳����ܵ�ͳһ����
        public void ExitCurtSkill()
        {
            PlayerCanControl(true);

            if (curtSkillCfg != null)
            {
                //�˳�����״̬
                if (!curtSkillCfg.isBreak)
                {
                    entityState = EntityState.None;
                }
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
                //������������
                curtSkillCfg = null;
            }
            SetAction(Constants.ActionDefault);
        }

        public void RmvMoveCB(int tid)
        {
            int index = -1;
            for (int i = 0; i < skMoveCBLst.Count; i++)
            {
                if (skMoveCBLst[i] == tid)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                skMoveCBLst.RemoveAt(index);
            }
        }

        public void RmvActionCB(int tid)
        {
            int index = -1;
            for (int i = 0; i < skActionCBLst.Count; i++)
            {
                if (skActionCBLst[i] == tid)
                {
                    index = i;
                    break;
                }
            }
            if (index != -1)
            {
                skActionCBLst.RemoveAt(index);
            }
        }

        public void ClearActionCBLst()
        {
            skMoveCBLst.Clear();
            skActionCBLst.Clear();
        }

        #region RemoveSkillCB
        private void CancelSkillMove(EntityBase entity)
        {
            entity.SetDir(Vector2.zero);
            entity.SetSkillMoveState(false);
        }

        private void DelTimeTaskByTid(EntityBase entity)
        {
            for (int i = 0; i < entity.skMoveCBLst.Count; i++)
            {
                int tid = entity.skMoveCBLst[i];
                TimerSvc.MainInstance.DelTask(tid);
            }

            for (int i = 0; i < entity.skActionCBLst.Count; i++)
            {
                int tid = entity.skActionCBLst[i];
                TimerSvc.MainInstance.DelTask(tid);
            }
        }

        private void ClearSkillEndCB(EntityBase entity)
        {
            if (entity.skEndCB != -1)
            {
                TimerSvc.MainInstance.DelTask(entity.skEndCB);
                entity.skEndCB = -1;
            }
            entity.ClearActionCBLst();
        }

        private void ClearComboData(EntityBase entity)
        {
            if (entity.nextSkillID != 0 || entity.comboQue.Count > 0)
            {
                entity.nextSkillID = 0;
                entity.comboQue.Clear();

                entity.battleMgr.lastAtkTime = 0;
                entity.battleMgr.comboIndex = 0;
            }
        }

        public void RmvSkillCB()
        {
            CancelSkillMove(this);

            //����tidɾ����ʱ�ص�����Ӧ���˺����ƶ�������Ч
            DelTimeTaskByTid(this);

            //�������жϣ�ɾ����ʱ�ص�
            ClearSkillEndCB(this);

            //�����������
            ClearComboData(this);
        }
        #endregion
    }
}