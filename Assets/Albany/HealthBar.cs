using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    // Static instance of HealthBar to ensure only one exists
    public static HealthBar Instance { get; private set; }

    private List<HealthBarUnit> units;

    private void Awake()
    {
        // Ensure only one instance of HealthBar exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);  // Destroy the new instance if one already exists
        }
        else
        {
            Instance = this;  // Set the instance to this object
            DontDestroyOnLoad(gameObject);  // Optional: keep this object alive across scenes
        }

        units = new List<HealthBarUnit>();

        foreach (HealthBarUnit unit in GetComponentsInChildren<HealthBarUnit>())
        {
            print("Unit!");
            units.Add(unit);
        }
    }



    public void UpdateHealth(int newHealth)
    {
        // Ensure the health is within bounds
        if (newHealth >= 0 && newHealth < units.Count)
        {
            units[newHealth].SetState(false);
        }
    }
}