using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current.digit2Key.wasPressedThisFrame)
        {
            animator.SetTrigger("Shoot");
        }
    }
}
