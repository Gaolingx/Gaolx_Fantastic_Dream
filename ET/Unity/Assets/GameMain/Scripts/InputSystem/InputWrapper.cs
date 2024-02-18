using StarterAssets;
using UnityEngine;

public class InputWrapper
{
    private Vector2 move;
    private Vector2 look;
    public float zoom;
    private bool jump;
    private bool sprint;
    private bool punchRight;
    private bool punchLeft;
    private bool crouch;
    private bool flipJump;
    private bool roll;
    private bool lockOn;
    private bool isModified;



    [Header("Movement Settings")]
    public bool analogMovement;

    [Header("Mouse Cursor Settings")]
    public bool cursorLocked = true;
    public bool cursorInputForLook = true;

    private StarterAssetsInputs _starterAssetInputs;

    public Vector2 Move { get => move; set { move = value; if (_starterAssetInputs) { /*_starterAssetInputs.move = move*/; } } }
    public Vector2 Look { get => look; set { look = value; if (_starterAssetInputs) { _starterAssetInputs.look = look; } } }
    public float Zoom { get => zoom; set { zoom = value; if (_starterAssetInputs) { _starterAssetInputs.zoom = zoom; } } }
    public bool Jump { get => jump; set { jump = value; if (_starterAssetInputs) { _starterAssetInputs.jump = jump; } } }
    public bool Sprint { get => sprint; set { sprint = value; if (_starterAssetInputs) { _starterAssetInputs.sprint = sprint; } } }
    public bool PunchRight { get => punchRight; set { punchRight = value; if (_starterAssetInputs) { _starterAssetInputs.punchRight = punchRight; } } }
    public bool PunchLeft { get => punchLeft; set { punchLeft = value; if (_starterAssetInputs) { _starterAssetInputs.punchLeft = punchLeft; } } }
    public bool Crouch { get => crouch; set { crouch = value; if (_starterAssetInputs) { _starterAssetInputs.crouch = crouch; } } }
    public bool FlipJump { get => flipJump; set { flipJump = value; if (_starterAssetInputs) { _starterAssetInputs.flipJump = flipJump; } } }
    public bool Roll { get => roll; set { roll = value; if (_starterAssetInputs) { _starterAssetInputs.roll = roll; } } }
    public bool LockOn { get => lockOn; set { lockOn = value; if (_starterAssetInputs) { _starterAssetInputs.lockOn = lockOn; } } }
    public bool IsModified { get => isModified; set { isModified = value; if (_starterAssetInputs) { _starterAssetInputs.isModified = isModified; } } }

    public InputWrapper()
    {
    }

    public InputWrapper(StarterAssetsInputs inputs)
    {
        _starterAssetInputs = inputs;

        Move = inputs.move;
        Look = inputs.look;
        Zoom = inputs.zoom;
        Jump = inputs.jump;
        Sprint = inputs.sprint;
        PunchRight = inputs.punchRight;
        PunchLeft = inputs.punchLeft;
        Crouch = inputs.crouch;
        FlipJump = inputs.flipJump;
        Roll = inputs.roll;
        LockOn = inputs.lockOn;
        IsModified = inputs.isModified;
        analogMovement = inputs.analogMovement;
        cursorLocked = inputs.cursorLocked;
        cursorInputForLook = inputs.cursorInputForLook;
    }

    public void SetInputs(StarterAssetsInputs inputs)
    {
        Move = inputs.move;
        Look = inputs.look;
        Zoom = inputs.zoom;
        Jump = inputs.jump;
        Sprint = inputs.sprint;
        PunchRight = inputs.punchRight;
        PunchLeft = inputs.punchLeft;
        Crouch = inputs.crouch;
        FlipJump = inputs.flipJump;
        Roll = inputs.roll;
        LockOn = inputs.lockOn;
        IsModified = inputs.isModified;
        analogMovement = inputs.analogMovement;
        cursorLocked = inputs.cursorLocked;
        cursorInputForLook = inputs.cursorInputForLook;
    }
}
