using UnityEngine;

public class PressurePlate : MonoBehaviour
{
    [Header("Damage Settings")]
    [Tooltip("Amount of damage dealt when the plate is stepped on.")]
    [SerializeField] private int damageAmount = 20;

    [Tooltip("If true, only objects tagged 'Player' can trigger damage. If false, it will search for any IDamageable in the hierarchy.")]
    [SerializeField] private bool requirePlayerTag = true;

    [Header("Color Feedback")]
    [Tooltip("If true, the plate changes color when pressed.")]
    [SerializeField] private bool useColorFeedback = true;

    [Tooltip("Renderer used to change the plate color for visual feedback.")]
    [SerializeField] private Renderer plateRenderer;

    [Tooltip("Color of the plate when not pressed.")]
    [SerializeField] private Color idleColor = Color.green;

    [Tooltip("Color of the plate while pressed.")]
    [SerializeField] private Color pressedColor = Color.red;

    [Header("Debug")]
    [Tooltip("Enable verbose console logs for debugging.")]
    [SerializeField] private bool verboseLogs = true;

    // Tracks whether the player or damageable object is currently on the plate.
    private bool _playerOnPlate = false;

    private void Reset()
    {
        // Auto-assign renderer if available on the same GameObject.
        plateRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        // Apply initial idle color if feedback is enabled.
        if (useColorFeedback && plateRenderer != null)
        {
            plateRenderer.material.color = idleColor;
        }
    }

    private void Log(string msg)
    {
        if (verboseLogs) Debug.Log($"[PressurePlate:{name}] {msg}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Log($"OnTriggerEnter with '{other.name}' (tag={other.tag}). _playerOnPlate={_playerOnPlate}");

        // If tag is required and doesn't match, ignore.
        if (requirePlayerTag && !other.CompareTag("Player"))
        {
            Log("Ignored because tag != Player");
            return;
        }

        // Avoid applying damage multiple times while standing on the plate.
        if (_playerOnPlate)
        {
            Log("Already activated — no additional damage.");
            return;
        }

        // Try to locate an IDamageable on the object or its parents.
        if (TryGetDamageableFromCollider(other, out var damageable))
        {
            Log($"Found IDamageable '{damageable}' — dealing {damageAmount} damage.");
            damageable.TakeDamage(damageAmount);

            _playerOnPlate = true;

            // Apply pressed color for visual feedback.
            if (useColorFeedback && plateRenderer != null)
                plateRenderer.material.color = pressedColor;
        }
        else
        {
            Log("No IDamageable found on object or its parents.");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Log($"OnTriggerExit with '{other.name}' (tag={other.tag}).");

        // If using tag restriction, ignore exits from non-player objects.
        if (requirePlayerTag && !other.CompareTag("Player"))
        {
            Log("Ignored exit because tag != Player");
            return;
        }

        // Reset the state so stepping on again deals damage again.
        _playerOnPlate = false;
        Log("Reset _playerOnPlate -> false");

        // Restore idle color.
        if (useColorFeedback && plateRenderer != null)
            plateRenderer.material.color = idleColor;
    }
    
    /// Searches for an IDamageable component on the collider or any of its parents.
    private bool TryGetDamageableFromCollider(Collider c, out IDamageable damageable)
    {
        damageable = null;

        // Direct component check.
        if (c.TryGetComponent<IDamageable>(out damageable))
            return true;

        // Recursively search parent transform hierarchy.
        var parent = c.transform;
        while (parent != null)
        {
            if (parent.TryGetComponent<IDamageable>(out damageable))
                return true;

            parent = parent.parent;
        }

        return false;
    }
}
