using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonAnimationController : MonoBehaviour
{
    [SerializeField] ThirdPersonController thirdPersonController;
    [SerializeField] Animator animator;

    private const string isWalking = "IsWalking";
    private const string isRunning = "IsRunning";
    private const string jumpUp = "JumpUp";
    private const string jumpForward = "JumpForward";
    private const string isFalling = "Falling";
    private const string isLanding = "Landing";

    private void Start()
    {
        if (animator == null) { }
            animator = GetComponent<Animator>();

        if (thirdPersonController == null)
            thirdPersonController = GetComponentInParent<ThirdPersonController>();

        thirdPersonController.OnJump += thirdPersonController_OnJump;
    }

    private void thirdPersonController_OnJump(object sender, ThirdPersonController.OnJumpEventArgs e)
    {
        if (e.jumpUp) animator.SetTrigger(jumpUp);
        else if (e.jumpForward) animator.SetTrigger(jumpForward);
    }

    private void Update()
    {
        animator.SetBool(isWalking, thirdPersonController.IsWalking);
        animator.SetBool(isRunning, thirdPersonController.IsRunning);
        animator.SetBool(isFalling, thirdPersonController.IsFalling);
        animator.SetBool(isLanding, thirdPersonController.IsLanding);
    }
}