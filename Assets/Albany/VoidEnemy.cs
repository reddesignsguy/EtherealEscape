using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VoidEnemy : MonoBehaviour
{
    public float speed = 2f;
    public bool active = false;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    public void StartMovement()
    {
        active = true;
    }

    private void FixedUpdate()
    {
        if (active)
        {
            Vector3 movement = new Vector3(0,speed * Time.deltaTime,0);
            rb.MovePosition(rb.position + movement);

        }
    }

    // TODO refactor (should be in enemy)

    public int dmg = 1;


    private Coroutine hurtCoroutine;


    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player" && hurtCoroutine == null)
        {
            // Initiate hurt FX
            // Haptic feedback
            hurtCoroutine = StartCoroutine(damageCoroutine());
        }
    }

    private IEnumerator damageCoroutine()
    {
        // Trigger vignette player state

        while (true)
        {
            Player.Instance.HurtPlayer(dmg);
            yield return new WaitForSeconds(1f); // Wait for 5 seconds before repeating
        }
    }

    private void OnTriggerExit(Collider other)
    {

        // Exit vignette
        if (other.gameObject.tag == "Player")
        {

            // Initiate hurt FX
            // Haptic feedback
            if (hurtCoroutine != null)
                StopCoroutine(hurtCoroutine);

            print("Exiting damaging");
            hurtCoroutine = null;
        }
    }

}