using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class ThirdPersonController : MonoBehaviour
{

    [SerializeField] private float movementSpeed = 2.5f;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private Transform playerCamera;
    [SerializeField, Range(0f, 0.3f)] float rotationSmoothTime = 0.1f;
    private float rotationSmoothVelocity;

    private void Start()
    {
        if (characterController == null) characterController = GetComponent<CharacterController>();
        if (playerCamera == null) playerCamera = Camera.main.transform;
    }

    private void Update()
    {
        Movement();

    }
    private void Movement()
    {
        // Inputs
        float xInput = Input.GetAxisRaw("Horizontal");
        float zInput = Input.GetAxisRaw("Vertical");
        
        // Move Direction
        Vector3 moveDirection = new Vector3(xInput, 0, zInput).normalized;
        if (moveDirection.magnitude < 0.1f) return; 
        
        // Rotate Towards MoveDirection
        float targetAngle = Mathf.Atan2(moveDirection.x, moveDirection.z) * Mathf.Rad2Deg + playerCamera.eulerAngles.y;
        float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref rotationSmoothVelocity, rotationSmoothTime);
        transform.rotation = Quaternion.Euler(transform.rotation.x, angle, transform.rotation.z);

        // Move
        Vector3 moveDir = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
        characterController.Move(moveDir.normalized * movementSpeed * Time.deltaTime);
    }
}
