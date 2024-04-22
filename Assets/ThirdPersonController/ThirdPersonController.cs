using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{

    public event EventHandler<OnJumpEventArgs> OnJump;
    public class OnJumpEventArgs : EventArgs
    {
        public bool jumpForward;
        public bool jumpUp;
    }

    [Header("References")]
    [SerializeField] private Camera playerCamera;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    [Header("Settings Camera")]
    [SerializeField] private float runningFovAmount = 5f;
    [SerializeField] private float timeToRunningFov = 18f;
    float walkingFov;
    float runningFov;

    [Header("Settings Movement")]
    [SerializeField] private float walkingSpeed = 1.5f;
    [SerializeField] private float runningSpeed = 3.5f;
    [SerializeField] private float jumpHeight = 1f;
    [SerializeField] private float groundCheckRadius = 0.4f;
    [SerializeField, Range(0f, 0.3f)] float rotationSmoothTime = 0.1f;
    private float rotationSmoothVelocity;
    private float jumpCooldown = 1f;
    public Vector3 MoveDirection { get; private set; }
    private float movementSpeed = 3f;
    Vector3 lastMoveDirection;
    float lastMovementSpeed;
    float jumpTimer;
    bool isWalking;
    public bool IsWalking { get { return isWalking && IsRunning == false; } }
    public bool IsRunning
    {
        get
        {
            return isWalking && movementSpeed > walkingSpeed;
        }
    }
    public bool IsFalling
    {
        get
        {
            return IsGrounded == false && velocity.y < -3f;
        }
    }
    public bool IsLanding { get; private set; }

    [Header("Settings Gravity")]
    [SerializeField] private float gravity = -9.81f;
    public float Gravity { get { return gravity; } }
    public bool IsGrounded { get; private set; }
    Vector3 velocity;

    private void Start()
    {
        // If camera not set, try get it from tag MainCamera
        if (playerCamera == null) playerCamera = Camera.main;

        // If characterController not set, try to get it from this GameObject
        if (characterController == null) characterController = GetComponent<CharacterController>();

        // If groundCheck not set, try find it in child
        if (groundCheck == null)
        {
            for (int i = 0; i < transform.childCount; i++)
            {
                if (transform.GetChild(i).name.ToLower() == "groundcheck") groundCheck = transform.GetChild(i);
            }
        }
        walkingFov = playerCamera.fieldOfView;
        runningFov = walkingFov + runningFovAmount;
    }

    private void Update()
    {
        AddGravity();

        Movement();
    }
    private void Movement()
    {
        // Inputs
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");
        
        // Move Direction
        Vector3 moveDirection = new Vector3(xInput, 0, zInput).normalized;

        MoveDirection = moveDirection;
        // Rotate Towards MoveDirection
        
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + playerCamera.transform.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSmoothVelocity, rotationSmoothTime);
        if (moveDirection.magnitude > 0.1f && IsGrounded)
        {
            transform.rotation = Quaternion.Euler(transform.rotation.x, angle, transform.rotation.z);
        }

        // Running
        if (Input.GetKey(KeyCode.LeftShift))
        {
            movementSpeed = runningSpeed;
        }
        else
        {
            movementSpeed = walkingSpeed;
        }

        // Move
        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;

        if (moveDirection.magnitude > 0.1f && IsGrounded)
            characterController.Move(moveDir.normalized * movementSpeed * Time.deltaTime);

        if (IsGrounded)
        {
            if (moveDirection.magnitude > 0.1f) isWalking = true;
            else isWalking = false;
            IsLanding = false;
            lastMovementSpeed = movementSpeed;
            lastMoveDirection = moveDir;
        }
        else
        {
            isWalking = false;
            characterController.Move(lastMoveDirection * lastMovementSpeed * Time.deltaTime);
        }


        jumpTimer += Time.deltaTime;

        // Jump
        if (Input.GetKeyDown(KeyCode.Space) && IsGrounded && jumpTimer >= jumpCooldown)
        {
            jumpTimer = 0;
            bool jumpWithMovement = lastMoveDirection != Vector3.zero;
            OnJump?.Invoke(this, new OnJumpEventArgs
            {
                jumpForward = jumpWithMovement,
                jumpUp = !jumpWithMovement
            });
            velocity.y = Mathf.Sqrt(jumpHeight * -2 * gravity);
        }

        UpdateCameraFov();
    }

    private void AddGravity()
    {
        IsGrounded = Physics.CheckSphere(groundCheck.position, groundCheckRadius, groundLayer);

        if (IsGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
        else
        {
            velocity.y += gravity * Time.deltaTime;
            CheckLanding();
        }
        characterController.Move(velocity * Time.deltaTime);
    }

    private void UpdateCameraFov()
    {
        runningFov = walkingFov + runningFovAmount;
        // if running or falling
        if (IsRunning || velocity.y < 0.1f && !IsGrounded)
        {
            playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, runningFov, timeToRunningFov * Time.deltaTime);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.MoveTowards(playerCamera.fieldOfView, walkingFov, timeToRunningFov * Time.deltaTime);
        }
    }

    private void CheckLanding()
    {
        if (Physics.SphereCast(transform.position, characterController.radius, -transform.up, out RaycastHit hit, characterController.height * 2))
        {
            IsLanding = true;
        }
    }
}
