/*
 * PuzzleSwitchController
 * ----------------------
 * Handles an individual lever puzzle where each lever must match a target ON/OFF pattern.
 * Updates torches, triggers puzzle completion, and communicates with group/master controllers.
 *
 * Dependencies:
 * - LeverSwitch (with OnToggle callbacks)
 * - TorchController (for each lever + final torch)
 * - UnityEvents for puzzle progression
 */

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
        // Validate correct array sizes
        ValidateSetup();

        // Randomize initial lever states
        ApplyRandomStart();

        // Update torches after lever initialization
        StartCoroutine(VerifyTorchesAfterSetup());

        // Subscribe to lever state changes
        for (int i = 0; i < levers.Length; i++)
        {
            int index = i;
            levers[i].OnToggle.AddListener((state) => OnLeverChanged(index, state));
        }
    }

    private void ApplyRandomStart()
    {
        // Random ON/OFF starting state for each lever
        for (int i = 0; i < levers.Length; i++)
        {
            bool randomState = Random.value > 0.5f;
            levers[i].SetState(randomState, false);
        }
    }

    private IEnumerator VerifyTorchesAfterSetup()
    {
        // Wait one frame for levers to finish initialization
        yield return null;

        // Update indicator torches based on pattern match
        for (int i = 0; i < levers.Length; i++)
        {
            bool match = levers[i].IsOn == targetPattern[i];
            torches[i].SetTorchState(match);
        }

        if (finalTorch != null)
            finalTorch.SetTorchState(false);

        // Notify group controller
        OnPuzzleStateChanged?.Invoke(this);
    }

    private void OnLeverChanged(int leverIndex, bool state)
    {
        // Ignore interaction if puzzle is already solved
        if (puzzleCompleted) return;

        // Update indicator torch for this lever
        bool correct = (state == targetPattern[leverIndex]);
        torches[leverIndex].SetTorchState(correct);

        // Check if entire puzzle is now solved
        CheckAllLevers();
    }

    private void CheckAllLevers()
    {
        bool allCorrect = true;

        // Validate every lever against the pattern
        for (int i = 0; i < levers.Length; i++)
        {
            bool match = levers[i].IsOn == targetPattern[i];
            torches[i].SetTorchState(match);

            if (!match)
                allCorrect = false;
        }

        // If something is wrong, reset final torch
        if (!allCorrect)
        {
            if (finalTorch != null)
                finalTorch.SetTorchState(false);

            OnPuzzleStateChanged?.Invoke(this);
            return;
        }

        // All levers correct → puzzle solved!
        CompletePuzzle();
    }

    private void CompletePuzzle()
    {
        if (puzzleCompleted) return;

        puzzleCompleted = true;

        // Light final completion torch
        if (finalTorch != null)
            finalTorch.SetTorchState(true);

        // Enable & disable assigned objects
        foreach (var obj in objectsToEnable)
            if (obj != null)
                obj.SetActive(true);

        foreach (var obj in objectsToDisable)
            if (obj != null)
                obj.SetActive(false);

        // Notify listeners and group master
        OnPuzzleCompleted?.Invoke();
        OnPuzzleStateChanged?.Invoke(this);
    }

    private void ValidateSetup()
    {
        // Basic consistency checks
        if (levers.Length != targetPattern.Length)
            Debug.LogError($"{name}: targetPattern length mismatch.");

        if (levers.Length != torches.Length)
            Debug.LogError($"{name}: torches length mismatch.");
    }
}
