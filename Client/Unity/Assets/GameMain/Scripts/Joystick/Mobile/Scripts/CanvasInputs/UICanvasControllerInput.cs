using HuHu;
using UnityEngine;

namespace StarterAssets
{
    public class UICanvasControllerInput : Singleton<UICanvasControllerInput>
    {

        [Header("Output")]
        public StarterAssetsInputs starterAssetsInputs;

        private bool EnableUICanvasInput = false;
        private void Update()
        {
            EnableUICanvasInput = DarkGod.Main.GameRoot.MainInstance.GetUIController()._isInputEnable;
        }

        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.MoveInput(virtualMoveDirection);
            }
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.LookInput(virtualLookDirection);
            }
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.JumpInput(virtualJumpState);
            }
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.SprintInput(virtualSprintState);
            }
        }

        public void VirtualSkill01Input(bool virtualSkill01State)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.Skill01Input(virtualSkill01State);
            }
        }

        public void VirtualSkill02Input(bool virtualSkill02State)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.Skill02Input(virtualSkill02State);
            }
        }

        public void VirtualSkill03Input(bool virtualSkill03State)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.Skill03Input(virtualSkill03State);
            }
        }

        public void VirtualNormalAtkInput(bool virtualNormalAtkState)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.NormalAtkInput(virtualNormalAtkState);
            }
        }

        public void VirtualGamePauseInput(bool virtualGamePauseState)
        {
            if (EnableUICanvasInput)
            {
                starterAssetsInputs.GamePauseInput(virtualGamePauseState);
            }
        }

    }

}
