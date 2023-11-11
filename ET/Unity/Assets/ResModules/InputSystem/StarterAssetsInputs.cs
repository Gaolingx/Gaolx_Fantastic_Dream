using System;
using Unity.VisualScripting;
using UnityEngine;

using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using static UnityEngine.Rendering.DebugUI;


namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool punchRight;
		public bool punchLeft;
		public bool crouch;
		public bool flipJump;
		public bool roll;
		public bool lockOn;
		public bool isModified;



		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;


		[SerializeField]


        public void Awake()
        {
			// TODO: Maybe don't search for this
			InputAction jumpAction = null;
            foreach (var item in GetComponent<PlayerInput>().actions)
            {
				if (item.name == "Jump")
                {
					jumpAction = item;
                }

			}


			if (jumpAction != null)
			{
				jumpAction.performed +=
					context =>
					{
						Debug.Log($"Interaction: {context.interaction}");

						FlipJumpInput(context.interaction is HoldInteraction);
						JumpInput(context.interaction is PressInteraction || context.interaction is TapInteraction);
					};
			}
			
		}

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

		public void OnKickModifier(InputValue value)
		{
            KickModifierInput(value.isPressed);
        }

		public void OnRoll(InputValue value)
		{
			RollInput(value.isPressed);
		}

		public void OnFlipJump(InputValue value)
		{
			FlipJumpInput(value.isPressed);
		}


        public void OnCrouch(InputValue value)
        {
			CrouchInput(value.isPressed);
        }


        public void OnPunchRight(InputValue value)
		{
			PunchRightInput(value.isPressed);
		}

		public void OnPunchLeft(InputValue inputValue) 
		{
			PunchLeftInput(inputValue.isPressed);
        }

       


        public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnLockOn(InputValue value)
		{
            LockOnInput(value.isPressed);
        }

        private void LockOnInput(bool isPressed)
        {
            lockOn = isPressed;
        }

		private void KickModifierInput(bool isPressed)
		{
			isModified = isPressed;
		}



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
        private void CrouchInput(bool newCrouchState)
        {
            crouch = newCrouchState;
        }

		public void RollInput(bool newRollState)
		{
			roll = newRollState;
		}

		public void PunchRightInput(bool newPunchRightState)
		{
			punchRight = newPunchRightState;
		}
		
		public void PunchLeftInput(bool newPunchLeftState)
		{
			punchLeft = newPunchLeftState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
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