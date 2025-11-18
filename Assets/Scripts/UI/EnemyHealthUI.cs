using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(HealthSystem))]
public class EnemyHealthUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Canvas worldSpaceCanvas; 
    [SerializeField] private Slider healthSlider;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0, 2f, 0);

    private HealthSystem health;
    private Camera mainCamera;

    private void Awake()
    {
        health = GetComponent<HealthSystem>();
        mainCamera = Camera.main;

        if (healthSlider != null)
        {
            healthSlider.maxValue = health.MaxHealth;
            healthSlider.value = health.CurrentHealth;
        }
    }

    private void LateUpdate()
    {
        if (worldSpaceCanvas == null || healthSlider == null) return;
        
        healthSlider.value = health.CurrentHealth;
        
        if (mainCamera != null)
            worldSpaceCanvas.transform.rotation = Quaternion.LookRotation(worldSpaceCanvas.transform.position - mainCamera.transform.position);
        
        worldSpaceCanvas.transform.position = transform.position + offset;
    }
}