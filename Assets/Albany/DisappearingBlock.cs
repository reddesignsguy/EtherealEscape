using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingBlock : MonoBehaviour
{

    BoxCollider collide;
    private Animator animator;

    private void Awake()
    {
        collide = GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
    }

    public void SetAppear(bool appear)
    {
        Debug.Log("Disappear");
        collide.enabled = appear;
        animator.SetBool("Appear", appear);
    }
}
