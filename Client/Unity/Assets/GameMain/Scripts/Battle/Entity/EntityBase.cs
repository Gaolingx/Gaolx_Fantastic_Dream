//���ܣ��߼�ʵ�����

using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public abstract class EntityBase
    {
        //��ҵ�ǰ״̬
        public AniState currentAniState { get; set; } = AniState.None;

        public BattleMgr battleMgr { get; set; } = null;
        public StateMgr stateMgr { get; set; } = null;
        public SkillMgr skillMgr { get; set; } = null;

        public ThirdPersonController playerController { get; set; } = null;
        protected Controller controller { get; set; } = null;

        public string EntityName { get; set; } = null;

        public bool CanControl { get; set; } = true;
        public bool CanRlsSkill { get; set; } = true;

        public EntityType entityType { get; set; } = EntityType.None;
        public EntityState entityState { get; set; } = EntityState.None;

        public BattleProps Props { get; protected set; } //ֻ���ڼ̳������������޸�

        public BindableProperty<int> currentHP { get; set; } = new BindableProperty<int>();
        protected int oldHp; //ս���е�hp

        //�ö��д洢���ж�Ӧ�ļ���id�����ͷ���˴��չ��󣬼���Ƿ������һ�μ���id��
        public Queue<int> comboQue { get; set; } = new Queue<int>();
        public int nextSkillID { get; set; } = 0;

        public SkillCfg curtSkillCfg { get; set; } //��ǰ����ʩ�ż��ܵ�id

        //����λ�ƵĻص�id
        public List<int> skMoveCBLst { get; set; } = new List<int>();
        //�����˺�����ص�id
        public List<int> skActionCBLst { get; set; } = new List<int>();

        //���ܽ����ص�
        public int skEndCB { get; set; } = -1;

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

        public void SetCtrl<T>(T ctrl) where T : Component
        {
            if (ctrl is Controller)
            {
                controller = ctrl as Controller;
            }
            else if (ctrl is ThirdPersonController)
            {
                playerController = ctrl as ThirdPersonController;
            }
        }

        public void SetActive(bool active = true)
        {
            if (entityType == EntityType.Monster)
            {
                controller.gameObject.SetActive(active);
            }
            else if (entityType == EntityType.Player)
            {
                playerController.gameObject.SetActive(active);
            }
        }

        public void PlayerCanControl(bool state = true)
        {
            CanControl = state;
        }

        public virtual void SetBattleProps(BattleProps props)
        {
            currentHP.Value = props.hp;
            Props = props;
        }

        public virtual void AddHealthData()
        {
            currentHP.OnValueChanged += OnUpdateHP;
        }

        public virtual void RmvHealthData()
        {
            currentHP.OnValueChanged -= OnUpdateHP;
        }

        private void OnUpdateHP(int value)
        {
            PECommon.Log("HP change:" + oldHp + " to " + value + ".EntityType:" + entityType + ".EntityName:" + EntityName);
            SetHPVal(oldHp, value);
            oldHp = value;
        }

        public virtual void SetAniBlend(int blend)
        {
            if (entityType == EntityType.Monster)
            {
                controller.SetBlend(blend);
            }
            else if (entityType == EntityType.Player)
            {
                playerController.SetAniBlend(blend);
            }
        }
        public virtual void SetDir(Vector2 dir)
        {
            if (entityType == EntityType.Monster)
            {
                controller.Dir = dir;
            }
            else if (entityType == EntityType.Player)
            {
                playerController.SetDir(dir);
            }
        }
        public virtual void SetAction(int action, bool rootMotion = true)
        {
            if (entityType == EntityType.Monster)
            {
                controller.SetAction(action);
            }
            else if (entityType == EntityType.Player)
            {
                playerController.ApplyRootMotion = rootMotion;
                playerController.SetAction(action);
            }
        }

        public virtual void SetCFX(string fxName, float destroyTime)
        {
            if (entityType == EntityType.Player)
            {
                VFXManager.MainInstance.SetFX(playerController.transform, fxName, destroyTime);
            }
        }
        public virtual void SetSkillMoveState(bool move, float speed = 0f)
        {
            if (entityType == EntityType.Player)
            {
                playerController.SetSkillMove(move, speed);
            }
        }
        public virtual void SetAtkRotation(Vector2 dir, bool isOffset = false) //���������ƫ��
        {
            if (entityType == EntityType.Monster)
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
            else if (entityType == EntityType.Player)
            {
                playerController.SetAtkRotationLocal(dir);
            }
        }

        #region ս����Ϣ��ʾ
        public virtual void SetDodge()
        {
            if (controller != null || playerController != null)
            {
                GameRoot.MainInstance.dynamicWnd.SetDodge(EntityName);
            }
        }
        public virtual void SetCritical(int critical)
        {
            if (controller != null || playerController != null)
            {
                GameRoot.MainInstance.dynamicWnd.SetCritical(EntityName, critical);
            }
        }
        public virtual void SetHurt(int hurt)
        {
            if (controller != null || playerController != null)
            {
                GameRoot.MainInstance.dynamicWnd.SetHurt(EntityName, hurt);
            }
        }
        public virtual void SetHPVal(int oldval, int newval)
        {
            if (controller != null || playerController != null)
            {
                GameRoot.MainInstance.dynamicWnd.SetHPVal(EntityName, oldval, newval);
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
            if (entityType == EntityType.Monster)
            {
                return controller.transform.position;
            }
            else if (entityType == EntityType.Player)
            {
                return playerController.transform.position;
            }

            return Vector3.zero;
        }

        public virtual Transform GetTrans()
        {
            if (entityType == EntityType.Monster)
            {
                return controller.transform;
            }
            else if (entityType == EntityType.Player)
            {
                return playerController.transform;
            }

            return null;
        }

        public AnimationClip[] GetAniClips()
        {
            if (entityType == EntityType.Monster)
            {
                return controller.ani.runtimeAnimatorController.animationClips;
            }
            else if (entityType == EntityType.Player)
            {
                return playerController.GetAnimator().runtimeAnimatorController.animationClips;
            }
            return null;
        }

        public AudioSource GetAudioSource()
        {
            if (entityType == EntityType.Monster)
            {
                return controller.GetComponent<AudioSource>();
            }
            else if (entityType == EntityType.Player)
            {
                return playerController.GetComponent<AudioSource>();
            }
            return null;
        }

        public CharacterController GetCharacterController()
        {
            if (entityType == EntityType.Monster)
            {
                return controller.GetComponent<CharacterController>();
            }
            else if (entityType == EntityType.Player)
            {
                return playerController.GetComponent<CharacterController>();
            }
            return null;
        }

        //��ȡ��ǰ�����Ƿ�ɱ��ж�״̬
        public virtual bool GetBreakState()
        {
            return true;
        }

        public virtual void PlayHitAudio(Transform transform)
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