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
        puzzleStates = new bool[puzzles.Length];

        for (int i = 0; i < puzzles.Length; i++)
        {
            int index = i;
            puzzles[i].OnPuzzleStateChanged.AddListener((controller) => OnPuzzleUpdated(index));
            puzzleStates[i] = puzzles[i].IsPuzzleCompleted;
        }

        UpdateGlobalState();
    }

    private void OnPuzzleUpdated(int index)
    {
        puzzleStates[index] = puzzles[index].IsPuzzleCompleted;
        UpdateGlobalState();
    }

    private void UpdateGlobalState()
    {
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

        if (allCompleted) return;
        allCompleted = true;

        if (finalTorch != null)
            finalTorch.SetTorchState(true);

        foreach (var obj in objectsToEnable)
            if (obj != null)
                obj.SetActive(true);

        foreach (var obj in objectsToDisable)
            if (obj != null)
                obj.SetActive(false);

        OnAllPuzzlesCompleted?.Invoke();
    }
}