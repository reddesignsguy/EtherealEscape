using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DisappearingBlockSequence : MonoBehaviour
{
    List<DisappearingBlock> blocks;

    private float timer = 0f;
    public float interval = 2f; // The interval in seconds
    private bool turnOnEvens = true;

    private void Awake()
    {
        blocks = new List<DisappearingBlock>();
        foreach (DisappearingBlock block in GetComponentsInChildren<DisappearingBlock>())
        {
            Debug.Log(block);
            blocks.Add(block);
        }        

    }

    private void Start()
    {
        SwitchBlockVisibility();
    }
    void Update()
    {
        // Increment the timer by the time passed since the last frame
        timer += Time.deltaTime;

        // Check if the timer has reached or exceeded the interval
        if (timer >= interval)
        {
            SwitchBlockVisibility();
            // Reset the timer
            timer = 0f;
        }
    }

    private void SwitchBlockVisibility()
    {
        for (int i = 0; i < blocks.Count; i ++)
        {
            if (i % 2 == 0)
            {
                blocks[i].SetAppear(turnOnEvens);
            }
            else
            {
                blocks[i].SetAppear(!turnOnEvens);
            }
        }

        turnOnEvens = !turnOnEvens;
    }
}
