using DarkGod.Main;
using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : MonoBehaviour
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        private bool cursorInputForLook = false;
        private void Update()
        {
            UIController uiController = GameRoot.Instance.GetUIController();
            if (uiController != null)
            {
                cursorInputForLook = uiController._isInputEnable;
            }
        }

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            if (cursorInputForLook)
            {
                starterAssetsInputs.MoveInput(virtualMoveDirection);
            }
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            starterAssetsInputs.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            starterAssetsInputs.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            starterAssetsInputs.SprintInput(virtualSprintState);
        }

        public void VirtualSkill01Input(bool virtualSkill01State)
        {
            starterAssetsInputs.Skill01Input(virtualSkill01State);
        }

        public void VirtualSkill02Input(bool virtualSkill02State)
        {
            starterAssetsInputs.Skill02Input(virtualSkill02State);
        }

        public void VirtualSkill03Input(bool virtualSkill03State)
        {
            starterAssetsInputs.Skill03Input(virtualSkill03State);
        }

        public void VirtualNormalAtkInput(bool virtualNormalAtkState)
        {
            starterAssetsInputs.NormalAtkInput(virtualNormalAtkState);
        }

        public void VirtualGamePauseInput(bool virtualGamePauseState)
        {
            starterAssetsInputs.GamePauseInput(virtualGamePauseState);
        }

    }

}
