using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
    [RequireComponent(typeof(PlayerInput))]
    public class StarterAssetsInputs : MonoBehaviour
    {
        [Header("Character Input Values")]
        public Vector2 move;
        public Vector2 look;
        public float zoom;
        public bool jump;
        public bool sprint;
        public bool crouch;
        public bool flipJump;
        public bool skill01;
        public bool skill02;
        public bool skill03;
        public bool normalAtk;
        public bool isPause;
        public bool canMove = true;
        public bool canLook = true;

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            if (!canMove) { MoveInput(Vector2.zero); }
            else { MoveInput(value.Get<Vector2>()); }
        }

        public void OnLook(InputValue value)
        {
            if (!canLook) { LookInput(Vector2.zero); }
            else { LookInput(value.Get<Vector2>()); }
        }

        public void OnJump(InputValue value)
        {
            if (!canMove) { JumpInput(false); }
            else { JumpInput(value.isPressed); }
        }

        public void OnFlipJump(InputValue value)
        {
            if (!canMove) { FlipJumpInput(false); }
            else { FlipJumpInput(value.isPressed); }
        }

        public void OnSprint(InputValue value)
        {
            if (!canMove) { SprintInput(false); }
            else { SprintInput(value.isPressed); }
        }

        public void OnCrouch(InputValue value)
        {
            if (!canMove) { CrouchInput(false); }
            else { CrouchInput(value.isPressed); }
        }

        public void OnZoom(InputValue value)
        {
            if (!canLook) { ZoomInput(0f); }
            else { ZoomInput(value.Get<float>()); }
        }

        public void OnAtkSkill01(InputValue value)
        {
            if (!canMove) { Skill01Input(false); }
            else { Skill01Input(value.isPressed); }
        }

        public void OnAtkSkill02(InputValue value)
        {
            if (!canMove) { Skill02Input(false); }
            else { Skill02Input(value.isPressed); }
        }

        public void OnAtkSkill03(InputValue value)
        {
            if (!canMove) { Skill03Input(false); }
            else { Skill03Input(value.isPressed); }
        }

        public void OnNormalAtk(InputValue value)
        {
            if (!canMove) { NormalAtkInput(false); }
            else { NormalAtkInput(value.isPressed); }
        }

        public void OnCursorLock(InputValue value)
        {
            CursorLockInput(value.isPressed);
        }

        public void OnGamePause(InputValue value)
        {
            GamePauseInput(value.isPressed);
        }
#endif

        public void MoveInput(Vector2 newMoveDirection)
        {
            move = newMoveDirection;
        }

        public void LookInput(Vector2 newLookDirection)
        {
            look = newLookDirection;
        }

        public void JumpInput(bool newJumpState)
        {
            jump = newJumpState;
        }

        public void FlipJumpInput(bool newFlipJumpState)
        {
            flipJump = newFlipJumpState;
        }

        public void SprintInput(bool newSprintState)
        {
            sprint = newSprintState;
        }

        public void ZoomInput(float newZoomValue)
        {
            zoom = newZoomValue;
        }

        public void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }

        public void Skill01Input(bool newSkill01State)
        {
            skill01 = newSkill01State;
        }

        public void Skill02Input(bool newSkill02State)
        {
            skill02 = newSkill02State;
        }

        public void Skill03Input(bool newSkill03State)
        {
            skill03 = newSkill03State;
        }

        public void NormalAtkInput(bool newNormalAtkState)
        {
            normalAtk = newNormalAtkState;
        }

        public void CursorLockInput(bool newCursorLockState)
        {
            cursorLocked = newCursorLockState;
        }

        public void GamePauseInput(bool newGamePauseState)
        {
            isPause = newGamePauseState;
        }
    }

}