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
        if (leverBone == null)
            Debug.LogWarning($"{name}: leverBone is not assigned.");

        ApplyImmediateVisual(IsOn);
    }

    private void Update()
    {
        if (!isAnimating || leverBone == null) return;

        t += Time.deltaTime / Mathf.Max(rotateDuration, 0.0001f);
        float eval = smooth ? easeCurve.Evaluate(Mathf.Clamp01(t)) : Mathf.Clamp01(t);

        leverBone.localRotation = Quaternion.Slerp(fromRot, toRot, eval);

        if (t >= 1f)
            isAnimating = false;
    }

    public void Interact(PlayerInteraction player)
    {
        Debug.Log($"Interacted with lever: {name}");
        Toggle();
    }

    public void Release() { }

    public void Toggle()
    {
        SetState(!IsOn, true);
    }

    public void SetState(bool newState, bool animate = true)
    {
        IsOn = newState;

        Vector3 targetRotEuler = IsOn ? onRotation : offRotation;

        if (leverBone != null)
        {
            fromRot = leverBone.localRotation;
            toRot = Quaternion.Euler(targetRotEuler);

            if (animate)
            {
                t = 0f;
                isAnimating = true;
            }
            else
            {
                leverBone.localRotation = toRot;
                isAnimating = false;
            }
        }

        OnToggle?.Invoke(IsOn);
    }

    private void ApplyImmediateVisual(bool state)
    {
        if (leverBone == null) return;

        Vector3 targetRotEuler = state ? onRotation : offRotation;
        leverBone.localRotation = Quaternion.Euler(targetRotEuler);
        isAnimating = false;
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (!Application.isPlaying && leverBone != null)
        {
            Vector3 targetRotEuler = IsOn ? onRotation : offRotation;
            leverBone.localRotation = Quaternion.Euler(targetRotEuler);
        }
    }
#endif
}
