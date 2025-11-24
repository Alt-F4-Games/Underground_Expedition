/*
 * PressurePlate
 * This script represents a damage-dealing pressure plate that activates when a player
 * or any object implementing IDamageable steps on it. It can optionally require the
 * object to have the "Player" tag and includes visual color feedback when pressed.
 *
 * Dependencies:
 * - Requires a Collider set as Trigger.
 * - Optional: Renderer for color feedback.
 * - Optional: Any object stepping on the plate must implement IDamageable to receive damage.
 */

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

    // Tracks whether a damageable object is currently touching the plate.
    private bool _playerOnPlate = false;

    private void Reset()
    {
        // Automatically assign the Renderer if the component exists on the same GameObject.
        plateRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        // Set the initial color to idle when the game starts.
        if (useColorFeedback && plateRenderer != null)
        {
            plateRenderer.material.color = idleColor;
        }
    }

    // Helper method for debug logging.
    
    private void Log(string msg)
    {
        if (verboseLogs) Debug.Log($"[PressurePlate:{name}] {msg}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Log($"OnTriggerEnter with '{other.name}' (tag={other.tag}). _playerOnPlate={_playerOnPlate}");

        // If tag-checking is enabled, ignore anything not tagged as Player.
        if (requirePlayerTag && !other.CompareTag("Player"))
        {
            Log("Ignored because tag != Player");
            return;
        }

        // Prevent damage being applied multiple times while staying on the plate.
        if (_playerOnPlate)
        {
            Log("Already activated — no additional damage.");
            return;
        }

        // Try to locate an IDamageable component in the object or its parents.
        if (TryGetDamageableFromCollider(other, out var damageable))
        {
            Log($"Found IDamageable '{damageable}' — dealing {damageAmount} damage.");
            damageable.TakeDamage(damageAmount);

            // Mark as pressed so it doesn't retrigger.
            _playerOnPlate = true;

            // Apply pressed visual color if enabled.
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

        // Ignore exit events from objects that don't match the tag requirement.
        if (requirePlayerTag && !other.CompareTag("Player"))
        {
            Log("Ignored exit because tag != Player");
            return;
        }

        // Reset the state so stepping again deals damage again.
        _playerOnPlate = false;
        Log("Reset _playerOnPlate -> false");

        // Restore idle visual color.
        if (useColorFeedback && plateRenderer != null)
            plateRenderer.material.color = idleColor;
    }
    
    // Searches the collider and its entire parent hierarchy to find an IDamageable component.
    
    private bool TryGetDamageableFromCollider(Collider c, out IDamageable damageable)
    {
        damageable = null;

        // Direct attempt on the collider GameObject.
        if (c.TryGetComponent<IDamageable>(out damageable))
            return true;

        // Traverse parents searching for IDamageable.
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
