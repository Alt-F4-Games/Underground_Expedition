using UnityEngine;

public class HealthTest : MonoBehaviour
{
    HealthSystem _healthSystem;
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            _healthSystem.TakeDamage(10);
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            _healthSystem.Heal(10);
        }
    }
}
