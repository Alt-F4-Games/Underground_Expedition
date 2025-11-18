using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class PuzzleSwitchController : MonoBehaviour
{
    [Header("Puzzle Levers")]
    [SerializeField] private LeverSwitch[] levers;

    [Header("Target Pattern (true = ON, false = OFF)")]
    [SerializeField] private bool[] targetPattern;

    [Header("Individual Indicator Torches")]
    [SerializeField] private TorchController[] torches;

    [Header("Final Torch (puzzle complete)")]
    [SerializeField] private TorchController finalTorch;

    [Header("Objects to Enable/Disable on Completion")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Events")]
    public UnityEvent OnPuzzleCompleted;
    public UnityEvent<PuzzleSwitchController> OnPuzzleStateChanged;

    private bool puzzleCompleted = false;
    public bool IsPuzzleCompleted => puzzleCompleted;

    private void Start()
    {
        ValidateSetup();
        ApplyRandomStart();
        StartCoroutine(VerifyTorchesAfterSetup());

        for (int i = 0; i < levers.Length; i++)
        {
            int index = i;
            levers[i].OnToggle.AddListener((state) => OnLeverChanged(index, state));
        }
    }

    private void ApplyRandomStart()
    {
        for (int i = 0; i < levers.Length; i++)
        {
            bool randomState = Random.value > 0.5f;
            levers[i].SetState(randomState, false);
        }
    }

    private IEnumerator VerifyTorchesAfterSetup()
    {
        yield return null;

        for (int i = 0; i < levers.Length; i++)
        {
            bool match = levers[i].IsOn == targetPattern[i];
            torches[i].SetTorchState(match);
        }

        if (finalTorch != null)
            finalTorch.SetTorchState(false);

        OnPuzzleStateChanged?.Invoke(this);
    }

    private void OnLeverChanged(int leverIndex, bool state)
    {
        if (puzzleCompleted) return;

        bool correct = (state == targetPattern[leverIndex]);
        torches[leverIndex].SetTorchState(correct);

        CheckAllLevers();
    }

    private void CheckAllLevers()
    {
        bool allCorrect = true;

        for (int i = 0; i < levers.Length; i++)
        {
            bool match = levers[i].IsOn == targetPattern[i];
            torches[i].SetTorchState(match);

            if (!match)
                allCorrect = false;
        }

        if (!allCorrect)
        {
            if (finalTorch != null)
                finalTorch.SetTorchState(false);
            OnPuzzleStateChanged?.Invoke(this);
            return;
        }

        CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        if (puzzleCompleted) return;

        puzzleCompleted = true;

        if (finalTorch != null)
            finalTorch.SetTorchState(true);

        foreach (var obj in objectsToEnable)
            if (obj != null)
                obj.SetActive(true);

        foreach (var obj in objectsToDisable)
            if (obj != null)
                obj.SetActive(false);

        OnPuzzleCompleted?.Invoke();
        OnPuzzleStateChanged?.Invoke(this);
    }

    private void ValidateSetup()
    {
        if (levers.Length != targetPattern.Length)
            Debug.LogError($"{name}: targetPattern length mismatch.");

        if (levers.Length != torches.Length)
            Debug.LogError($"{name}: torches length mismatch.");
    }
}
