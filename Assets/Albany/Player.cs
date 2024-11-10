using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class Player : MonoBehaviour
{
    // Singleton instance
    public static Player Instance { get; private set; }

    public XRBaseController left;
    public XRBaseController right;

    public int _maxHealth = 5;
    public int _health = 5;

    public float hapticStrength = 5f;
    public float hapticDuration = 5f;

    private void Awake()
    {
        // Implement singleton pattern
        if (Instance == null)
        {
            Instance = this;
            _health = _maxHealth;
        }
        else
        {
            Destroy(gameObject); // Destroy duplicate instance
        }
    }

    public void HurtPlayer(int dmg)
    {
        _health -= dmg;

        HealthBar.Instance.UpdateHealth(_health);

        left.SendHapticImpulse(hapticStrength, hapticDuration);
        right.SendHapticImpulse(hapticStrength, hapticDuration);

        if (_health <= 0)
        {
            HandleDeath();
        }
    }

    private void HandleDeath()
    {

    }
}
