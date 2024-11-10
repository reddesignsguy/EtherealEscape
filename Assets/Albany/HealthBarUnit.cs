using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBarUnit : MonoBehaviour
{
    public bool on = true;
    private Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        animator.SetBool("On", true);
    }

    public void SetState(bool state)
    {
        animator.SetBool("On", state);
    }
}
