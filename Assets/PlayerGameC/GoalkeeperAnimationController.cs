using UnityEngine;
using UnityEngine.InputSystem;

public class GoalkeeperAnimationController : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Keyboard.current.gKey.wasPressedThisFrame)
        {
            Debug.Log("G Pressed");

            animator.SetTrigger("Save");
        }
    }
}