//功能：逻辑实体基类

using DarkGod.Tools;
using StarterAssets;
using System.Collections.Generic;
using UnityEngine;

namespace DarkGod.Main
{
    public abstract class EntityBase
    {
        //玩家当前状态
        public AniState currentAniState { get; set; } = AniState.None;

        public BattleMgr battleMgr { get; set; }
        public StateMgr stateMgr { get; set; }
        public SkillMgr skillMgr { get; set; }
        public EventMgr eventMgr { get; set; }
        public VFXManager VFXMgr { get; set; }

        protected ThirdPersonController playerController { get; set; }
        protected Controller controller { get; set; }

        public string EntityName { get; set; }
        public int EntityID { get => this.GetHashCode(); }

        public BindableProperty<bool> CanControl { get; set; } = new BindableProperty<bool>();
        public BindableProperty<bool> CanRlsSkill { get; set; } = new BindableProperty<bool>();

        public EntityType entityType { get; set; } = EntityType.None;
        public EntityState entityState { get; set; } = EntityState.None;

        public BattleProps Props { get; protected set; } //只能在继承他的子类中修改

        public BindableProperty<int> CurrentHP { get; set; } = new BindableProperty<int>();
        //战斗中的hp
        protected int oldHp;

        //用队列存储连招对应的技能id，当释放完此次普攻后，检测是否存在下一次技能id。
        public Queue<int> comboQue { get; set; } = new Queue<int>();
        public int nextSkillID { get; set; } = 0;

        //当前正在施放技能的id
        public SkillCfg curtSkillCfg { get; set; }

        //技能位移的回调id
        public List<int> skMoveCBLst { get; set; } = new List<int>();
        //技能伤害计算回调id
        public List<int> skActionCBLst { get; set; } = new List<int>();
        //技能结束回调
        public int skEndCB { get; set; } = -1;

        #region State Define
        public void OnInitFSM()
        {
            if (stateMgr != null)
            {
                stateMgr.InitFSM(this);
            }
        }

        //状态切换
        public void StateBorn()
        {
            if (stateMgr != null)
            {
                stateMgr.ChangeStatus(this, AniState.Born, null);
            }
        }
        public void StateMove()
        {
            if (stateMgr != null)
            {
                stateMgr.ChangeStatus(this, AniState.Move, null);
            }
        }
        public void StateIdle()
        {
            if (stateMgr != null)
            {
                stateMgr.ChangeStatus(this, AniState.Idle, null);
            }
        }
        public void StateAttack(int skillID)
        {
            if (stateMgr != null)
            {
                stateMgr.ChangeStatus(this, AniState.Attack, skillID);
            }
        }
        public void StateHit()
        {
            if (stateMgr != null)
            {
                stateMgr.ChangeStatus(this, AniState.Hit, null);
            }
        }
        public void StateDie()
        {
            if (stateMgr != null)
            {
                stateMgr.ChangeStatus(this, AniState.Die, null);
            }
        }
        #endregion

        #region AI Logic
        //怪物ai逻辑
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
            CanControl.Value = state;
        }

        public virtual void SetBattleProps(BattleProps props)
        {
            CurrentHP.Value = props.hp;
            Props = props;
        }

        public virtual void AddEntityEventListener()
        {
            CurrentHP.OnValueChanged += delegate (int val) { OnUpdateHP(val); };
            CanControl.OnValueChanged += delegate (bool val) { OnUpdateCanControl(val); };
            CanRlsSkill.OnValueChanged += delegate (bool val) { OnUpdateCanRlsSkill(val); };
        }

        public virtual void RmvEntityEventListener()
        {
            CurrentHP.OnValueChanged -= delegate (int val) { OnUpdateHP(val); };
            CanControl.OnValueChanged -= delegate (bool val) { OnUpdateCanControl(val); };
            CanRlsSkill.OnValueChanged -= delegate (bool val) { OnUpdateCanRlsSkill(val); };
        }

        private void OnUpdateHP(int value)
        {
            PECommon.Log($"HP change:{oldHp} to {value}.EntityType:{entityType}.EntityName:{EntityName}.EntityID:{EntityID}");
            SetHPVal(oldHp, value);
            oldHp = value;
        }

        private void OnUpdateCanControl(bool value)
        {
            GameRoot.MainInstance.EnableInputAction(value);
        }

        private void OnUpdateCanRlsSkill(bool value)
        {
            BattleSys.MainInstance.playerCtrlWnd.CanRlsSkill = value;
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
        public virtual void SetAction(int action)
        {
            if (entityType == EntityType.Monster)
            {
                controller.SetAction(action);
            }
            else if (entityType == EntityType.Player)
            {
                playerController.SetAction(action);
            }
        }

        public virtual async void SetCFX(string fxName, float destroyTime)
        {
            if (entityType == EntityType.Player && VFXMgr != null)
            {
                await VFXMgr.Play(fxName, playerController.transform.localPosition, playerController.transform.localRotation, destroyTime, playerController.transform);
            }
        }
        public virtual void SetSkillMoveState(bool move, float speed = 0f)
        {
            if (entityType == EntityType.Player)
            {
                playerController.SetSkillMove(move, speed);
            }
        }
        public virtual void SetAtkRotation(Vector2 dir, bool isOffset = false) //考虑摄像机偏移
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

        #region 战斗信息显示
        public virtual void SetDodge()
        {
            if (controller != null || playerController != null)
            {
                MessageBox.MainInstance.SetDodge(EntityName);
            }
        }
        public virtual void SetCritical(int critical)
        {
            if (controller != null || playerController != null)
            {
                MessageBox.MainInstance.SetCritical(EntityName, critical);
            }
        }
        public virtual void SetHurt(int hurt)
        {
            if (controller != null || playerController != null)
            {
                MessageBox.MainInstance.SetHurt(EntityName, hurt);
            }
        }
        public virtual void SetHPVal(int oldval, int newval)
        {
            if (controller != null || playerController != null)
            {
                MessageBox.MainInstance.SetHPVal(EntityName, oldval, newval);
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

        //获取当前怪物是否可被中断状态
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

        //连招思路：按下普攻时，写入队列。当普通攻击完成后，退出Attack状态时检测，判断存储连招数据的队列中是否有数据，
        //有则取出一条skillID赋值给nextSkillID。进入Idle状态时候，如果nextSkillID不为零，进入攻击状态，释放下一个技能

        //退出技能的统一处理
        public void ExitCurtSkill()
        {
            PlayerCanControl(true);

            if (curtSkillCfg != null)
            {
                //退出霸体状态
                if (!curtSkillCfg.isBreak)
                {
                    entityState = EntityState.None;
                }
                //连招数据更新
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
                //技能配置清理
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

            //根据tid删除定时回调，相应的伤害和移动将不生效
            DelTimeTaskByTid(this);

            //攻击被中断，删除定时回调
            ClearSkillEndCB(this);

            //清空连招数据
            ClearComboData(this);
        }
        #endregion
    }
}