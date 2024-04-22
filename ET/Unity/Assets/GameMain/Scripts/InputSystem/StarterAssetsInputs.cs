using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
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

        [Header("Movement Settings")]
        public bool analogMovement;

        [Header("Mouse Cursor Settings")]
        public bool cursorLocked = true;
        public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM
        public void OnMove(InputValue value)
        {
            MoveInput(value.Get<Vector2>());
        }

        public void OnLook(InputValue value)
        {
            if(cursorInputForLook)
            {
                LookInput(value.Get<Vector2>());
            }
        }

        public void OnJump(InputValue value)
        {
            JumpInput(value.isPressed);
        }

        public void OnFlipJump(InputValue value)
        {
            FlipJumpInput(value.isPressed);
        }
        
        public void OnSprint(InputValue value)
        {
            SprintInput(value.isPressed);
        }

        public void OnCrouch(InputValue value)
        {
            CrouchInput(value.isPressed);
        }

        public void OnZoom(InputValue value)
        {
            ZoomInput(value.Get<float>());
        }

        public void OnAtkSkill01(InputValue value)
        {
            Skill01Input(value.isPressed);
        }

        public void OnAtkSkill02(InputValue value)
        {
            Skill02Input(value.isPressed);
        }

        public void OnAtkSkill03(InputValue value)
        {
            Skill03Input(value.isPressed);
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
        private void FlipJumpInput(bool newFlipJumpState)
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

        private void CrouchInput(bool newCrouchState)
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

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        private void SetCursorState(bool newState)
        {
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
    }
    
}