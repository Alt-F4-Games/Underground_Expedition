/*
 * PuzzleGroupMaster
 * -----------------
 * Controls a group of puzzle controllers and monitors when all puzzles are completed.
 * When all are solved, triggers events, activates/deactivates objects, and lights a final torch.
 *
 * Dependencies:
 * - PuzzleSwitchController
 * - TorchController (optional)
 * - UnityEvents for external reactions
 */

using UnityEngine;
using UnityEngine.Events;

public class PuzzleGroupMaster : MonoBehaviour
{
    [Header("Puzzle Groups")]
    [Tooltip("Assign any number of PuzzleSwitchController instances.")]
    [SerializeField] private PuzzleSwitchController[] puzzles;

    [Header("Final Torch (all puzzles completed)")]
    [SerializeField] private TorchController finalTorch;

    [Header("Objects to Enable/Disable when all puzzles are solved")]
    [SerializeField] private GameObject[] objectsToEnable;
    [SerializeField] private GameObject[] objectsToDisable;

    [Header("Events")]
    public UnityEvent OnAllPuzzlesCompleted;

    private bool[] puzzleStates;
    private bool allCompleted = false;

    private void Start()
    {
        // Initialize array tracking each puzzle's completion state
        puzzleStates = new bool[puzzles.Length];

        // Subscribe to puzzle events
        for (int i = 0; i < puzzles.Length; i++)
        {
            int index = i;
            puzzles[i].OnPuzzleStateChanged.AddListener((controller) => OnPuzzleUpdated(index));
            puzzleStates[i] = puzzles[i].IsPuzzleCompleted;
        }

        // Evaluate initial puzzle state
        UpdateGlobalState();
    }

    private void OnPuzzleUpdated(int index)
    {
        // Update state when a puzzle changes
        puzzleStates[index] = puzzles[index].IsPuzzleCompleted;
        UpdateGlobalState();
    }

    private void UpdateGlobalState()
    {
        // If any puzzle is incomplete, the group is not complete
        for (int i = 0; i < puzzleStates.Length; i++)
        {
            if (!puzzleStates[i])
            {
                if (finalTorch != null)
                    finalTorch.SetTorchState(false);

                allCompleted = false;
                return;
            }
        }

        // Prevent firing multiple times
        if (allCompleted) return;
        allCompleted = true;

        // Light final torch
        if (finalTorch != null)
            finalTorch.SetTorchState(true);

        // Enable assigned objects
        foreach (var obj in objectsToEnable)
            if (obj != null)
                obj.SetActive(true);

        // Disable assigned objects
        foreach (var obj in objectsToDisable)
            if (obj != null)
                obj.SetActive(false);

        // Invoke completion event
        OnAllPuzzlesCompleted?.Invoke();
    }
}
