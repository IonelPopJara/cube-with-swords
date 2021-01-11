using System.Collections;
using UnityEngine;

public class PlayerInputController : MonoBehaviour
{
    public PlayerInput Current;

    // Agregar cool downs here
    public float jumpPressedRememberTime = 0.25f;
    public float jumpPressedRemember;

    private void Start()
    {
        Current = new PlayerInput();
    }

    private void Update()
    {
        Vector3 moveInputRaw = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
        Vector3 moveInput = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        
        Vector2 mouseInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        jumpPressedRemember -= jumpPressedRemember > 0 ? Time.deltaTime : 0;

        // Jump Input
        if (Input.GetButtonDown("Jump"))
        {
            jumpPressedRemember = jumpPressedRememberTime;
        }

        bool interactInput = Input.GetKeyDown(KeyCode.F);
        bool lightAttackInput = Input.GetKeyDown(KeyCode.Mouse0);
        bool strongAttackInput = Input.GetKeyDown(KeyCode.Mouse1);
        bool dashInput = Input.GetKeyDown(KeyCode.LeftShift);

        Current = new PlayerInput()
        {
            MoveInputRaw = moveInputRaw,
            MoveInput = moveInput,
            MouseInput = mouseInput,
            JumpInput = jumpPressedRemember > 0f,
            LightAttackInput = lightAttackInput,
            StrongAttackInput = strongAttackInput,
            InteractInput = interactInput,
            DashInput = dashInput,
        };
    }
    public struct PlayerInput
    {
        public Vector3 MoveInputRaw;
        public Vector3 MoveInput;
        public Vector2 MouseInput;
        public bool JumpInput;
        public bool LightAttackInput;
        public bool StrongAttackInput;
        public bool InteractInput;
        public bool DashInput;
    }
}