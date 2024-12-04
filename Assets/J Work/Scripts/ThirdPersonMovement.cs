using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SocialPlatforms.Impl;

public class ThirdPersonMovement : MonoBehaviour
{
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    public Transform cam; // Camera reference for directional movement
    public Transform map;
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
    int interactTriggerHash;

    Vector2 currentMovementInput;
    Vector3 currentMovement;
    Vector3 moveDirection;
    bool isMovementPressed;
    bool isRunPressed;
    bool isJumpPressed;
    bool isInteractPressed;
    bool isGrounded;
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
        interactTriggerHash = Animator.StringToHash("isInteracting"); // Using a trigger for interaction

        playerInput.CharacterControls.Move.started += onMovementInput;
        playerInput.CharacterControls.Move.canceled += onMovementInput;
        playerInput.CharacterControls.Move.performed += onMovementInput;
        playerInput.CharacterControls.Run.started += onRun;
        playerInput.CharacterControls.Run.canceled += onRun;
        playerInput.CharacterControls.Jump.started += onJump; // Add jump input listener
        playerInput.CharacterControls.Jump.canceled += onJump;
        playerInput.CharacterControls.Interact.started += onClick;
        playerInput.CharacterControls.Interact.canceled += onClick;
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

    void onClick(InputAction.CallbackContext context)
    {
        isInteractPressed = context.ReadValueAsButton();
    }

    void handleRotation()
    {
        if (isMovementPressed && !animator.GetBool("isInInteractState")) // Check if the character is interacting
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

        // Walking animation
        if (isMovementPressed && !isWalking && !animator.GetBool("isInInteractState")) // Prevent walking when interacting
        {
            animator.SetBool(isWalkingHash, true);
        }
        else if (!isMovementPressed && isWalking)
        {
            animator.SetBool(isWalkingHash, false);
        }

        // Running animation
        if (isMovementPressed && isRunPressed && !isRunning && !animator.GetBool("isInInteractState")) // Prevent running when interacting
        {
            animator.SetBool(isRunningHash, true);
        }
        else if ((!isMovementPressed || !isRunPressed) && isRunning)
        {
            animator.SetBool(isRunningHash, false);
        }

        // Jumping animation
        if (isJumpPressed && !animator.GetBool("isInInteractState")) // Prevent jumping during interaction
        {
            animator.SetBool(isJumpingHash, true);
        }
        if (isGrounded)
        {
            animator.SetBool(isJumpingHash, false);
        }

        // Interaction animation (triggered once and then reset)
        if (isInteractPressed && !animator.GetBool("isInInteractState"))
        {
            animator.SetTrigger(interactTriggerHash);
            animator.SetBool("isInInteractState", true); // Set flag to indicate interaction is happening
            isInteractPressed = false;  // Reset the flag to prevent continuous animation triggering
        }
        else
        {
            animator.ResetTrigger(interactTriggerHash);
        }
    }


    void handleGravityAndJump()
    {
        if (isGrounded)
        {
            verticalVelocity = groundedGravity; // Set slight negative value to keep grounded

            // Allow jumping only when grounded
            if (isJumpPressed && !animator.GetBool("isInInteractState")) // Prevent jumping during interaction
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

    void FixedUpdate()
    {
        isGrounded = characterController.isGrounded || Physics.Raycast(transform.position, Vector3.down, 0.1f);
    }

    void Update()
    {
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        Vector3 finalMovement;
        Vector3 horizontalMovement;

        handleGravityAndJump();
        handleRotation();
        handleAnimation();

        // Calculate the movement, but only allow movement when not interacting
        if (animator.GetBool("isInInteractState") && !isRunPressed) // Prevent movement while interacting
        {
            finalMovement = Vector3.zero + Vector3.up * verticalVelocity;
        }
        else
        {
            float speed = isRunPressed ? runSpeed : walkSpeed;
            horizontalMovement = isMovementPressed ? moveDirection * speed : Vector3.zero;
            finalMovement = horizontalMovement + Vector3.up * verticalVelocity;
        }

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
