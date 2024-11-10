using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ThirdPersonMovement : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    public Transform cam; // Camera reference for directional movement
    public float walkSpeed = 6f;
    public float runSpeed = 18f;
    public float smooth = 0.1f;
    public float rotationFactorPerFrame = 15.0f;
    public float gravity = -9.81f;
    public float groundedGravity = -0.5f;
    public float jumpHeight = 3.0f; // Height of the jump

    int isWalkingHash;
    int isRunningHash;
    int isJumpingHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 moveDirection;
    bool isMovementPressed;
    bool isRunPressed;
    bool isJumpPressed;
    float turnsmoothvelocity;
    float verticalVelocity = 0f; // Vertical speed for gravity

    void Awake()
    {
        playerInput = new PlayerInput();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        isWalkingHash = Animator.StringToHash("isWalking");
        isRunningHash = Animator.StringToHash("isRunning");
        isJumpingHash = Animator.StringToHash("isJumping");

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump; // Add jump input listener
        playerInput.CharacterControls.Jump.canceled += onJump;
    }

    void onMovementInput(InputAction.CallbackContext context)
    {
        currentMovementInput = context.ReadValue<Vector2>();
        isMovementPressed = currentMovementInput.magnitude > 0;
    }

    void onRun(InputAction.CallbackContext context)
    {
        isRunPressed = context.ReadValueAsButton();
    }

    void onJump(InputAction.CallbackContext context)
    {
        isJumpPressed = context.ReadValueAsButton();
    }

    void handleRotation()
    {
        if (isMovementPressed)
        {
            float targetAngle = Mathf.Atan2(currentMovementInput.x, currentMovementInput.y) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnsmoothvelocity, smooth);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);
            moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        }
    }

    void handleAnimation()
    {
        bool isWalking = animator.GetBool(isWalkingHash);
        bool isRunning = animator.GetBool(isRunningHash);

        if (isMovementPressed && !isWalking)
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        if (isMovementPressed && isRunPressed && !isRunning)
        {
            animator.SetBool(isRunningHash, true);
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }

        if (isJumpPressed)
        {
            animator.SetBool(isJumpingHash, true);
        }
        if (characterController.isGrounded)
        {
            animator.SetBool(isJumpingHash, false);
        }
    }

    void handleGravityAndJump()
    {
        if (characterController.isGrounded)
        {
            verticalVelocity = groundedGravity; // Set slight negative value to keep grounded

            // Allow jumping only when grounded
            if (isJumpPressed)
            {
                verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
            }
        }
        else
        {
            // Apply gravity over time when in the air
            verticalVelocity += gravity * Time.deltaTime;
        }

        // Apply the vertical velocity to the current movement regardless of horizontal movement
        currentMovement.y = verticalVelocity;
    }

    void Update()
    {
        Debug.Log("Is Grounded: " + characterController.isGrounded);
        handleGravityAndJump();
        handleRotation();
        handleAnimation();

        // Calculate the movement
        float speed = isRunPressed ? runSpeed : walkSpeed;
        Vector3 horizontalMovement = isMovementPressed ? moveDirection * speed : Vector3.zero;
        Vector3 finalMovement = horizontalMovement + Vector3.up * verticalVelocity;

        // Move the character regardless of horizontal input to ensure gravity is applied
        characterController.Move(finalMovement * Time.deltaTime);
    }

    private void OnEnable()
    {
        playerInput.CharacterControls.Enable();
    }

    private void OnDisable()
    {
        playerInput.CharacterControls.Disable();
    }
}
