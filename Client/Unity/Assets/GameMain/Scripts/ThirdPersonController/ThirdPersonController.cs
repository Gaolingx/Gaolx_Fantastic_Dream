using Cinemachine;
using DarkGod.Main;
using DarkGod.Tools;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM 
using UnityEngine.InputSystem;
#endif

/* Note: animations are called via the controller for both the character and capsule using animator null checks
 */

namespace StarterAssets
{
    [RequireComponent(typeof(CharacterController))]
#if ENABLE_INPUT_SYSTEM 
    //[RequireComponent(typeof(PlayerInput))]
#endif
    public class ThirdPersonController : MonoBehaviour
    {
        public PlayerInput PlayerInput;
        public StarterAssetsInputs StarterAssetsInputs;
        public CinemachineVirtualCamera playerFollowVirtualCamera;

        [Header("Player")]
        [Tooltip("Move Control Mode")]
        public ControlState MoveControlState = ControlState.Walk;

        [Tooltip("Apply Human Scale")]
        public bool ApplyHumanScale = false;

        [Tooltip("Move speed of the character in m/s")]
        public float MoveSpeed = 2.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 5.335f;

        [Tooltip("Air Move speed of the character in m/s")]
        public float AirMoveSpeed = 2.5f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Space(10)]
        [Tooltip("The height the player can jump")]
        public float JumpHeight = 1.2f;

        [Space(10)]
        [Tooltip("The height the player can flip jump")]
        public float FlipJumpHeight = 1.4f;

        [Tooltip("The character uses its own gravity value. The engine default is -9.81f")]
        public float Gravity = -15.0f;

        [Space(10)]
        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout = 0.50f;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout = 0.15f;

        [Header("Player Grounded")]
        [Tooltip("If the character is grounded or not. Not part of the CharacterController built in grounded check")]
        public bool Grounded = true;

        [Tooltip("Useful for rough ground")]
        public float GroundedOffset = -0.14f;

        [Tooltip("The radius of the grounded check. Should match the radius of the CharacterController")]
        public float GroundedRadius = 0.28f;

        [Tooltip("What layers the character uses as ground")]
        public LayerMask GroundLayers;

        [Header("Cinemachine")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 70.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = -30.0f;

        [Tooltip("How fast in speed can you move the camera in X axis")]
        public float speedLook = 1.0f;

        public float maxDistance = 5.0f;
        public float minDistance = 0.5f;
        public float distanceChangeRate = 10.0f;

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        // move state
        [HideInInspector] public enum ControlState { None, Walk, Idle, Manual };

        // cinemachine
        private float _cinemachineTargetYaw;
        private float _cinemachineTargetPitch;
        private float _cameraDistance;
        private float _targetCameraDistance;

        // player
        private float _speed;
        private float _animationBlend;
        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _verticalVelocity;
        private float _terminalVelocity = 53.0f;

        // timeout deltatime
        private float _jumpTimeoutDelta;
        private float _fallTimeoutDelta;

        // animation IDs
        private int _animIDSpeed;
        private int _animIDGrounded;
        private int _animIDJump;
        private int _animIDFreeFall;
        private int _animIDMotionSpeed;
        private int _animIDCrouch;
        private int _animIDFlip;
        private int _animIDSkillAction;
        private int _animIDIdleAnim;

        // skill Move
        private bool _isSkillMove;
        private float _skillMoveSpeed;
        private Vector2 _dir;

        // idle task
        private int _idletid = 0;

#if ENABLE_INPUT_SYSTEM 
        private PlayerInput _playerInput;
#endif
        private Animator _animator;
        private CharacterController _controller;
        private StarterAssetsInputs _input;
        private GameObject _mainCamera;
        private Cinemachine3rdPersonFollow _personFollow;

        private BindableProperty<bool> idleAction = new BindableProperty<bool>();
        private System.Action<int> skillAction;
        private System.Action<bool, float> skillMoveAction;
        private System.Action<Vector2> atkRotationAction;
        private static AudioSvc _audioSvc;
        private static TimerSvc _timerSvc;

        private const float _threshold = 0.01f;

        private bool _hasAnimator;

        private bool IsCurrentDeviceMouse
        {
            get
            {
#if ENABLE_INPUT_SYSTEM
                return _playerInput.currentControlScheme == "KeyboardMouse";
#else
                return false;
#endif
            }
        }

        public Animator GetAnimator()
        {
            return _animator;
        }

        /// <summary>
        /// 移动模式切换
        /// </summary>
        /// <param name="moveMode">默认使用StarterAssetsInputs</param>
        public void SetMoveMode(ControlState state)
        {
            MoveControlState = state;
        }

        public void SetDir(Vector2 move)
        {
            _dir = move;
        }

        public void SetAniBlend(int blend)
        {

        }

        public void SetAction(int actionID)
        {
            skillAction?.Invoke(actionID);
        }

        public void SetSkillMove(bool isSkillMove, float skillMoveSpeed = 0f)
        {
            skillMoveAction?.Invoke(isSkillMove, skillMoveSpeed);
        }

        public void SetAtkRotationLocal(Vector2 atkDir)
        {
            atkRotationAction?.Invoke(atkDir);
        }


        #region MonoBehaviour
        private void ClassAwake()
        {
            idleAction.OnValueChanged += delegate (bool val) { OnIdle(val); };
            skillAction += delegate (int val) { OnAtkSkill(val); };
            skillMoveAction += delegate (bool val1, float val2) { OnSkillMove(val1, val2); };
            atkRotationAction += delegate (Vector2 val) { OnAtkRotation(val); };

            // get a reference to our main camera
            if (_mainCamera == null)
            {
                _mainCamera = GameObject.FindGameObjectWithTag("MainCamera");
            }

            // get instance reference
            _audioSvc = AudioSvc.MainInstance;
            _timerSvc = TimerSvc.MainInstance;
        }
        private void ClassStart()
        {
            _cinemachineTargetYaw = CinemachineCameraTarget.transform.rotation.eulerAngles.y;

            _hasAnimator = TryGetComponent(out _animator);
            _controller = GetComponent<CharacterController>();
            _input = StarterAssetsInputs.GetComponent<StarterAssetsInputs>();
            _personFollow = playerFollowVirtualCamera.GetComponent<CinemachineVirtualCamera>()
                .GetCinemachineComponent<Cinemachine3rdPersonFollow>();

#if ENABLE_INPUT_SYSTEM 
            _playerInput = PlayerInput.GetComponent<PlayerInput>();
#else
            Debug.LogError( "Starter Assets package is missing dependencies. Please use Tools/Starter Assets/Reinstall Dependencies to fix it");
#endif

            _cameraDistance = _personFollow.CameraDistance;
            _targetCameraDistance = _cameraDistance;

            AssignAnimationIDs();

            // reset our timeouts on start
            _jumpTimeoutDelta = JumpTimeout;
            _fallTimeoutDelta = FallTimeout;

            // apply human scale
            if (_hasAnimator && ApplyHumanScale) { _animator.speed /= _animator.humanScale; }

        }
        private void ClassUpdate()
        {
            _hasAnimator = TryGetComponent(out _animator);

            SkillMove();
            GroundedCheck();
            JumpAndGravity();
            Crouch();

            // idle state
            idleAction.Value = !CheckHasInput();

        }
        private void ClassLateUpdate()
        {
            CameraRotation();
        }
        #endregion

        private void Awake()
        {
            ClassAwake();
        }

        private void Start()
        {
            ClassStart();
        }

        private void Update()
        {
            ClassUpdate();
        }

        private void LateUpdate()
        {
            ClassLateUpdate();
        }

        private void AssignAnimationIDs()
        {
            _animIDSpeed = Animator.StringToHash("Speed");
            _animIDGrounded = Animator.StringToHash("Grounded");
            _animIDJump = Animator.StringToHash("Jump");
            _animIDFreeFall = Animator.StringToHash("FreeFall");
            _animIDMotionSpeed = Animator.StringToHash("MotionSpeed");
            _animIDCrouch = Animator.StringToHash("Crouch");
            _animIDFlip = Animator.StringToHash("Flip");
            _animIDSkillAction = Animator.StringToHash("SkillAction");
            _animIDIdleAnim = Animator.StringToHash("IdleAnim");
        }

        private void GroundedCheck()
        {
            // set sphere position, with offset
            Vector3 spherePosition = new Vector3(transform.position.x, transform.position.y - GroundedOffset,
                transform.position.z);
            Grounded = Physics.CheckSphere(spherePosition, GroundedRadius, GroundLayers,
                QueryTriggerInteraction.Ignore);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetBool(_animIDGrounded, Grounded);
            }
        }

        private void CameraRotation()
        {
            float zoom = _input.zoom;

            // if there is an input and camera position is not fixed
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                //Don't multiply mouse input by Time.deltaTime;
                float deltaTimeMultiplier = IsCurrentDeviceMouse ? 1.0f : Time.deltaTime;

                _cinemachineTargetYaw += _input.look.x * speedLook * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * speedLook * deltaTimeMultiplier;
            }

            // clamp our rotations so our values are limited 360 degrees
            _cinemachineTargetYaw = ClampAngle(_cinemachineTargetYaw, float.MinValue, float.MaxValue);
            _cinemachineTargetPitch = ClampAngle(_cinemachineTargetPitch, BottomClamp, TopClamp);

            // Zoom
            _targetCameraDistance -= zoom / 360.0f;
            _targetCameraDistance = Mathf.Clamp(_targetCameraDistance, minDistance, maxDistance);
            _cameraDistance = Mathf.Lerp(_cameraDistance, _targetCameraDistance,
                Time.deltaTime * distanceChangeRate);
            //_cameraDistance -= zoom * Time.deltaTime * distanceChangeRate / 18.0f;
            //_cameraDistance = Mathf.Clamp(_cameraDistance, minDistance, maxDistance);
            if (_personFollow != null)
            {
                _personFollow.CameraDistance = _cameraDistance;
            }

            // Cinemachine will follow this target
            CinemachineCameraTarget.transform.rotation = Quaternion.Euler(_cinemachineTargetPitch + CameraAngleOverride,
                _cinemachineTargetYaw, 0.0f);
        }

        private Vector2 GetMoveInputState()
        {
            if (MoveControlState == ControlState.Manual)
            {
                return _dir;
            }
            else if (MoveControlState == ControlState.None)
            {
                return Vector2.zero;
            }
            else
            {
                return _input.move;
            }
        }

        private bool CheckHasInput()
        {
            if (MoveControlState != ControlState.None)
            {
                if (_input.hasInput || !Grounded)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return true;
        }

        private void OnIdle(bool val)
        {
            // 等待x秒后如果仍处于Idle状态，则播放待机动画（定时任务）
            if (val == true)
            {
                _idletid = _timerSvc.AddTimeTask((int tid) =>
                {
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDIdleAnim, true);
                    }
                }, Constants.IdleAniWaitDelay);
            }
            else
            {
                _timerSvc.DelTask(_idletid);

                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDIdleAnim, false);
                }
            }
        }

        private void OnAtkSkill(int val)
        {
            if (_hasAnimator)
            {
                _animator.SetInteger(_animIDSkillAction, val);
            }
        }

        private void OnSkillMove(bool isMove, float skillMoveSpeed)
        {
            _isSkillMove = isMove;
            _skillMoveSpeed = skillMoveSpeed;
        }

        private void OnAtkRotation(Vector2 atkDir)
        {
            float angle = Vector2.SignedAngle(atkDir, new Vector2(0, 1));
            Vector3 eulerAngles = new Vector3(0, angle, 0);
            transform.localEulerAngles = eulerAngles;
        }

        private void Move()
        {
            Vector2 _moveVal = GetMoveInputState();

            // set target speed based on move speed, sprint speed and if sprint is pressed
            float targetSpeed = _input.sprint ? SprintSpeed : MoveSpeed;

            // a simplistic acceleration and deceleration designed to be easy to remove, replace, or iterate upon

            // note: Vector2's == operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is no input, set the target speed to 0
            if (_moveVal == Vector2.zero) targetSpeed = 0.0f;

            float inputMagnitude = _input.analogMovement ? _moveVal.magnitude : 1f;

            _animationBlend = Mathf.Lerp(_animationBlend, targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_moveVal.x, 0.0f, _moveVal.y).normalized;

            // note: Vector2's != operator uses approximation so is not floating point error prone, and is cheaper than magnitude
            // if there is a move input rotate player when the player is moving
            if (_moveVal != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;
                float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                    RotationSmoothTime);

                // rotate to face input direction relative to camera position
                transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
            }

            Vector3 targetDirMovement = Vector3.zero;
            if (!Grounded)
            {
                targetDirMovement = _moveVal.magnitude * AirMoveSpeed * Time.deltaTime * transform.forward;
            }
            else
            {
                targetDirMovement = _animator.deltaPosition;
            }

            Vector3 playerDeltaMovement = targetDirMovement + (new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);

            // move the player(by animator)
            _controller.Move(playerDeltaMovement);

            // update animator if using character
            if (_hasAnimator)
            {
                _animator.SetFloat(_animIDSpeed, _animationBlend);
                _animator.SetFloat(_animIDMotionSpeed, inputMagnitude);
            }
        }

        private void SkillMove()
        {
            if (_isSkillMove)
            {
                // move the player
                _controller.Move(_skillMoveSpeed * Time.deltaTime * transform.forward);
            }
        }

        private void JumpAndGravity()
        {
            if (Grounded && !_input.crouch)
            {
                // reset the fall timeout timer
                _fallTimeoutDelta = FallTimeout;

                // update animator if using character
                if (_hasAnimator)
                {
                    _animator.SetBool(_animIDJump, false);
                    _animator.SetBool(_animIDFreeFall, false);
                }

                // stop our velocity dropping infinitely when grounded
                if (_verticalVelocity < 0.0f)
                {
                    _verticalVelocity = -2f;
                }

                if (_input.flipJump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(FlipJumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFlip, true);
                        _animator.SetBool(_animIDCrouch, false);
                    }

                }
                // Jump
                else if (_input.jump && _jumpTimeoutDelta <= 0.0f)
                {
                    // the square root of H * -2 * G = how much velocity needed to reach desired height
                    _verticalVelocity = Mathf.Sqrt(JumpHeight * -2f * Gravity);

                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDJump, true);
                        _animator.SetBool(_animIDCrouch, false);
                    }

                }

                // jump timeout
                if (_jumpTimeoutDelta >= 0.0f)
                {
                    _jumpTimeoutDelta -= Time.deltaTime;
                }
            }
            else
            {
                // reset the jump timeout timer
                _jumpTimeoutDelta = JumpTimeout;

                // fall timeout
                if (_fallTimeoutDelta >= 0.0f)
                {
                    _fallTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    // update animator if using character
                    if (_hasAnimator)
                    {
                        _animator.SetBool(_animIDFreeFall, true);
                        _animator.SetBool(_animIDFlip, false);
                    }
                }

                // if we are not grounded, do not jump
                //_input.jump = false;
                //_input.flipJump = false;
            }

            // apply gravity over time if under terminal (multiply by delta time twice to linearly speed up over time)
            if (_verticalVelocity < _terminalVelocity)
            {
                _verticalVelocity += Gravity * Time.deltaTime;
            }
        }

        private void Crouch()
        {
            bool val = _input.crouch;

            if (_hasAnimator)
            {
                _animator.SetBool(_animIDCrouch, val);
            }
        }

        private static float ClampAngle(float lfAngle, float lfMin, float lfMax)
        {
            if (lfAngle < -360f) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return Mathf.Clamp(lfAngle, lfMin, lfMax);
        }

        private void OnDrawGizmosSelected()
        {
            Color transparentGreen = new Color(0.0f, 1.0f, 0.0f, 0.35f);
            Color transparentRed = new Color(1.0f, 0.0f, 0.0f, 0.35f);

            if (Grounded) Gizmos.color = transparentGreen;
            else Gizmos.color = transparentRed;

            // when selected, draw a gizmo in the position of, and matching radius of, the grounded collider
            Gizmos.DrawSphere(
                new Vector3(transform.position.x, transform.position.y - GroundedOffset, transform.position.z),
                GroundedRadius);
        }

        private void OnFootstep(AnimationEvent animationEvent)
        {
            if (_audioSvc != null)
            {
                if (animationEvent.animatorClipInfo.weight > 0.5f)
                {
                    _audioSvc.PlayFootStep(transform);
                }
            }
        }

        private void OnJump(AnimationEvent animationEvent)
        {
            if (_audioSvc != null)
            {
                if (animationEvent.animatorClipInfo.weight > 0.5f)
                {
                    _audioSvc.PlayJumpEffort(transform);
                }
            }
        }

        private void OnLand(AnimationEvent animationEvent)
        {
            if (_audioSvc != null)
            {
                if (animationEvent.animatorClipInfo.weight > 0.5f)
                {
                    _audioSvc.PlayLanding(transform);
                }
            }
        }

        private void OnAnimatorMove()
        {
            Move();
        }

        private void OnDisable()
        {
            idleAction.OnValueChanged -= delegate (bool val) { OnIdle(val); };
            skillAction -= delegate (int val) { OnAtkSkill(val); };
            skillMoveAction -= delegate (bool val1, float val2) { OnSkillMove(val1, val2); };
            atkRotationAction -= delegate (Vector2 val) { OnAtkRotation(val); };
        }

        private void OnDestroy()
        {
            _timerSvc.DelTask(_idletid);
        }
    }
}