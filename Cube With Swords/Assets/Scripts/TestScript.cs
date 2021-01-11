using UnityEngine;

/*
 * To do list:
 *  Dash Reset
 *  Falling State
 *  Damage while dashing
 */

public enum SimpleStateMachine
{
    Idle,
    Moving,
    Jumping,
    Falling,
    Dashing,
    Attacking
}

public enum ComboState
{
    None,
    Attack1,
    Attack2,
    Attack3,
    Attack4
}

public class TestScript : MonoBehaviour
{
    [Header("State Machine")]
    public SimpleStateMachine currentState;
    public ComboState currentComboState;
    public Animator animator;

    [Header("Player Settings")]
    public Transform playerMainCamera;
    public Transform inputDirection;
    public LayerMask whatIsGround;

    private CharacterController characterController;
    private PlayerInputController input;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
        input = GetComponent<PlayerInputController>();
        animator = GetComponent<Animator>();
    }

    private void Update()
    {
        //HandleInputs();
        //moveDirection = GetMoveDirection(movementInput);
        //SetGravity();
        //characterController.Move(moveDirection * Time.deltaTime);
        switch (currentState)
        {
            case SimpleStateMachine.Idle:
                HandleInputs();
                moveDirection = GetMoveDirection(movementInput);
                SetGravity();
                if (moveDirection.x != 0f && moveDirection.z != 0f)
                {
                    currentState = SimpleStateMachine.Moving;
                    //if (characterController.velocity.y < -1f)
                    //    currentState = SimpleStateMachine.Falling;
                    //else
                    //    currentState = SimpleStateMachine.Moving;
                }
                if (jumpInput && isGrounded)
                {
                    Jump();
                    currentState = SimpleStateMachine.Jumping;
                }
                if (dashInput) //Add a flag for canDash
                {
                    currentState = SimpleStateMachine.Dashing;
                }
                if (lightAttackInput)
                {
                    StartAttack();
                    currentState = SimpleStateMachine.Attacking;
                }

                characterController.Move(moveDirection * Time.deltaTime);
                break;
            case SimpleStateMachine.Moving:
                HandleInputs();
                moveDirection = GetMoveDirection(movementInput);
                if (moveDirection.x == 0f && moveDirection.z == 0f)
                    currentState = SimpleStateMachine.Idle;
                SetGravity();
                if (jumpInput && isGrounded)
                {
                    Jump();
                    currentState = SimpleStateMachine.Jumping;
                }
                if (dashInput)
                {
                    currentState = SimpleStateMachine.Dashing;
                }
                if (lightAttackInput)
                {
                    StartAttack();
                    currentState = SimpleStateMachine.Attacking;
                }
                characterController.Move(moveDirection * Time.deltaTime);
                break;
            case SimpleStateMachine.Jumping:
                HandleInputs();
                moveDirection = GetMoveDirection(movementInput);
                SetGravity();
                characterController.Move(moveDirection * Time.deltaTime);
                if (isGrounded)
                {
                    currentState = SimpleStateMachine.Moving;
                }
                if (dashInput)
                {
                    currentState = SimpleStateMachine.Dashing;
                }
                break;
            case SimpleStateMachine.Dashing:
                Dash();
                if (dashingTime >= dashlength)
                {
                    dashingTime = 0f;
                    currentState = SimpleStateMachine.Moving;
                }
                break;
            case SimpleStateMachine.Attacking:
                HandleInputs();
                ComboAttacks();
                ResetComboState();
                //if (animator.GetBool("Attack") == false)
                //    currentState = SimpleStateMachine.Moving;
                break;
            case SimpleStateMachine.Falling:
                HandleInputs();
                break;
        }
    }

    #region Inputs

    [Header("Debug Inputs")]
    public Vector3 movementInput;
    public bool isGrounded;
    public bool jumpInput;
    public bool dashInput;
    public bool lightAttackInput;

    public void HandleInputs()
    {
        movementInput = input.Current.MoveInputRaw;
        isGrounded = GetGroundedStatus();
        jumpInput = input.Current.JumpInput;
        dashInput = input.Current.DashInput;

        // Quizas podr[ia hacer que cambie de rotacion, no solo en el eje y
        inputDirection.rotation = Quaternion.Euler(0f, Mathf.Atan2(movementInput.x, movementInput.z) * Mathf.Rad2Deg + playerMainCamera.eulerAngles.y, 0f);

        lightAttackInput = input.Current.LightAttackInput;
    }

    #endregion

    #region Movement and Rotation

    [Header("Player Movement Rotation")]
    public float movementSpeed = 10f;
    [Range(0, 1f)] public float turnSmoothTime = 0.1f;
    private float turnSmoothVelocity;

    [Header("Movement Debug")]
    public Vector3 moveDirection;
    public Vector3 camForwad;
    public Vector3 camRight;

    private void GetCamDirection()
    {
        camForwad = playerMainCamera.forward;
        camRight = playerMainCamera.right;

        camForwad.y = 0;
        camRight.y = 0;
    }

    private Vector3 GetMoveDirection(Vector3 input)
    {
        GetCamDirection();

        if (input.sqrMagnitude > 0f)
        {
            float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + playerMainCamera.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            return (input.x * camRight + input.z * camForwad) * movementSpeed;
        }
        else
        {
            return Vector3.zero;
        }
        
        //if(input.sqrMagnitude > 0f)
        //{
        //    float targetAngle = Mathf.Atan2(input.x, input.z) * Mathf.Rad2Deg + playerMainCamera.eulerAngles.y;
        //    float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);

        //    transform.rotation = Quaternion.Euler(0f, angle, 0f);

        //    return (Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward).normalized * movementSpeed;
        //}
        //else
        //{
        //    return Vector3.zero;
        //}    
    }
    #endregion

    #region Abilities
    [Header("Abilities")]
    public float jumpForce = 15f;
    public float dashlength = 0.15f;
    public float dashSpeed = 50f;

    public float dashingTime = 0f;

    public void Jump()
    {
        fallVelocity = jumpForce;
        moveDirection.y = fallVelocity;
    }

    public void Dash()
    {
        // Hacer un dash reset
        characterController.Move(inputDirection.forward * dashSpeed * Time.deltaTime);

        if(dashingTime < dashlength)
        {
            dashingTime += Time.deltaTime;
        }
    }
    #endregion

    #region Gravity and Ground Detection

    [Header("Gravity Settings")]
    public float gravity = 40f;
    public float slideVelocity = 7f;
    public float slopeForceDown = 10f;

    [Header("Ground Time Remember")]
    public float groundedRememberTime = 0.1f;

    [Header("Debug Gravity and Ground Detection")]
    public Vector3 hitNormal;
    public float fallVelocity;
    public bool isOnSlope;
    public float groundedRemember;


    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        hitNormal = hit.normal;
    }

    private void SetGravity()
    {
        if(characterController.isGrounded)
        {
            fallVelocity = -gravity * Time.deltaTime;
            moveDirection.y = fallVelocity;
        }
        else
        {
            fallVelocity -= gravity * Time.deltaTime;
            moveDirection.y = fallVelocity;
        }

        SlideDown();
    }

    private void SlideDown()
    {
        isOnSlope = Vector3.Angle(Vector3.up, hitNormal) > characterController.slopeLimit;

        if(isOnSlope)
        {
            moveDirection.x += ((1f - hitNormal.y) * hitNormal.x) * slideVelocity;
            moveDirection.z += ((1f - hitNormal.y) * hitNormal.z) * slideVelocity;
            moveDirection.y -= slopeForceDown;
        }
    }

    private bool GetGroundedStatus()
    {
        groundedRemember -= groundedRemember > 0 ? Time.deltaTime : 0;

        if(characterController.isGrounded)
        {
            groundedRemember = groundedRememberTime;
        }

        return groundedRemember > 0f;
    }
    #endregion

    #region Attacks
    [Header("Combo Attacks Debug")]
    public bool activateTimerToReset;
    public float defaultComboTimer = 0.4f;
    public float currentComboTimer;

    public void StartAttack()
    {
        animator.applyRootMotion = true;
        animator.SetBool("Attack", true);
    }
    public void AttackFinished()
    {
        animator.applyRootMotion = false;
        Debug.Log("Attack finished");
        animator.SetBool("Attack", false);
    }

    public void ComboAttacks()
    {
        if(currentComboState <= ComboState.Attack3)
        {
            currentComboState++;
            activateTimerToReset = true;
            currentComboTimer = defaultComboTimer;

            if(currentComboState == ComboState.Attack1)
            {
                //Set Animator Attack 1
            }
            else if (currentComboState == ComboState.Attack2)
            {
                //Set Animator Attack 2
            }
            else if (currentComboState == ComboState.Attack3)
            {
                //Set Animator Attack 3
                //return
            }

        }
    }

    public void ResetComboState()
    {
        if(activateTimerToReset)
        {
            currentComboTimer -= Time.deltaTime;

            if(currentComboTimer <= 0f)
            {
                currentComboState = ComboState.None;

                activateTimerToReset = false;
                currentComboTimer = defaultComboTimer;
            }
        }
    }
    #endregion
}
