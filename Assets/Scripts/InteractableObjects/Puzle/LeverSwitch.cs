/*
 * LeverSwitch
 * -----------
 * Interactable lever that toggles between ON/OFF states.
 * Plays a rotation animation on the armature transform and triggers events when toggled.
 *
 * Dependencies:
 * - IInteractable interface
 * - (Optional) Lever bone Transform for visual rotation
 * - UnityEvent<bool> for callbacks on toggle
 */

using UnityEngine;
using UnityEngine.Events;
using Player;

public class LeverSwitch : MonoBehaviour, IInteractable
{
    [Header("State")]
    public bool IsOn = false;

    [Header("Lever Bone (Armature Transform)")]
    [SerializeField] private Transform leverBone;

    [Header("Final Local Rotation")]
    [Tooltip("Local rotation when the lever is ON.")]
    [SerializeField] private Vector3 onRotation = new Vector3(0, 0, 0);

    [Tooltip("Local rotation when the lever is OFF.")]
    [SerializeField] private Vector3 offRotation = new Vector3(180, 0, 0);

    [Header("Animation Settings")]
    [SerializeField] private float rotateDuration = 0.18f;
    [SerializeField] private bool smooth = true;
    [SerializeField] private AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEvent<bool> OnToggle;

    private Quaternion fromRot;
    private Quaternion toRot;
    private float t = 0f;
    private bool isAnimating = false;

    private void Start()
    {
        // Warn if no leverBone is assigned
        if (leverBone == null)
            Debug.LogWarning($"{name}: leverBone is not assigned.");

        // Apply the correct initial rotation visually
        ApplyImmediateVisual(IsOn);
    }

    private void Update()
    {
        // Handle smooth animation between states
        if (!isAnimating || leverBone == null) return;

        t += Time.deltaTime / Mathf.Max(rotateDuration, 0.0001f);
        float eval = smooth ? easeCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);

        leverBone.localRotation = Quaternion.Slerp(fromRot, toRot, eval);

        // Stop animation when finished
        if (t >= 1f)
            isAnimating = false;
    }

    public void Interact(PlayerInteraction player)
    {
        // Triggered by the player — simply toggle the lever state
        Debug.Log($"Interacted with lever: {name}");
        Toggle();
    }

    public void Release() { }

    public void Toggle()
    {
        // Invert the current state and apply it
        SetState(!IsOn, true);
    }

    public void SetState(bool newState, bool animate = true)
    {
        // Update the logical state
        IsOn = newState;

        // Determine target rotation
        Vector3 targetRotEuler = IsOn ? onRotation : offRotation;

        if (leverBone != null)
        {
            // Prepare animation parameters
            fromRot = leverBone.localRotation;
            toRot = Quaternion.Euler(targetRotEuler);

            if (animate)
            {
                t = 0f;
                isAnimating = true;
            }
            else
            {
                // Instantly set the rotation with no animation
                leverBone.localRotation = toRot;
                isAnimating = false;
            }
        }

        // Notify listeners that the lever changed state
        OnToggle?.Invoke(IsOn);
    }

    private void ApplyImmediateVisual(bool state)
    {
        // Apply correct rotation from the start (used on initialization)
        if (leverBone == null) return;

        Vector3 targetRotEuler = state ? onRotation : offRotation;
        leverBone.localRotation = Quaternion.Euler(targetRotEuler);
        isAnimating = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        // Update rotation in editor when changing variables
        if (!Application.isPlaying && leverBone != null)
        {
            Vector3 targetRotEuler = IsOn ? onRotation : offRotation;
            leverBone.localRotation = Quaternion.Euler(targetRotEuler);
        }
    }
#endif
}
