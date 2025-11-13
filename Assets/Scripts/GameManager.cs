using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [Header("Player Settings")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private Transform playerSpawnPoint;
    private GameObject currentPlayer;

    [Header("Enemy Settings")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private List<string> enemyIDsToSpawn = new(); 
    [SerializeField] private List<string> spawnPointsToUse = new(); 
    private List<GameObject> activeEnemies = new();

    [Header("Game State")]
    public bool isGameRunning { get; private set; } = false;
    public bool isPaused { get; private set; } = false;

    [Header("Persistence Data")]
    public int playerScore = 0;
    public int levelProgress = 0;
    public float playTime = 0f;
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartCoroutine(InitGameRoutine());
    }

    private IEnumerator InitGameRoutine()
    {
        yield return new WaitForSeconds(0.3f);

        if (UIManager.instance != null)
        {
            UIManager.instance.SetExperienceSystems(
                Player.ExperienceSystem.instance,
                Player.LevelSystem.instance
            );
        }

        SpawnPlayer();
        StartGame();
    }
    
    public void SpawnPlayer()
    {
        if (playerPrefab == null || playerSpawnPoint == null)
            return;

        if (currentPlayer != null)
        {
            Destroy(currentPlayer);
        }

        currentPlayer = Instantiate(playerPrefab, playerSpawnPoint.position, playerSpawnPoint.rotation);

        var health = currentPlayer.GetComponent<HealthSystem>();

        if (UIManager.instance != null)
        {
            UIManager.instance.SetPlayer(currentPlayer);
        }

        if (UIManager.instance != null)
        {
            UIManager.instance.SetExperienceSystems(
                Player.ExperienceSystem.instance,
                Player.LevelSystem.instance
            );
        }

        if (health != null)
        {
            StartCoroutine(WatchPlayerHealth(health));
        }
    }

    private IEnumerator WatchPlayerHealth(HealthSystem health)
    {
        while (health != null && health.IsAlive)
            yield return null;

        HandlePlayerDeath();
    }

    private void HandlePlayerDeath()
    {
        StartCoroutine(RespawnPlayerRoutine());
    }

    private IEnumerator RespawnPlayerRoutine()
    {
        yield return new WaitForSeconds(2f);
        ResetPlayerStats();
        SpawnPlayer();
    }

    private void ResetPlayerStats()
    {
    }
    
    public void SpawnEnemies()
    {
        if (spawner == null)
            return;

        ClearEnemies();

        int totalToSpawn = Mathf.Min(enemyIDsToSpawn.Count, spawnPointsToUse.Count);
        for (int i = 0; i < totalToSpawn; i++)
        {
            var enemy = spawner.Spawn(enemyIDsToSpawn[i], spawnPointsToUse[i]);
            if (enemy != null)
            {
                activeEnemies.Add(enemy);

                var h = enemy.GetComponent<HealthSystem>();
                if (h != null)
                {
                    StartCoroutine(WatchEnemyHealth(enemy, h));
                }
            }
        }
    }

    private IEnumerator WatchEnemyHealth(GameObject enemy, HealthSystem health)
    {
        while (health != null && health.IsAlive)
            yield return null;

        if (enemy != null)
        {
            activeEnemies.Remove(enemy);
        }
    }

    public void ClearEnemies()
    {
        foreach (var e in activeEnemies)
        {
            if (e != null)
                Destroy(e);
        }
        activeEnemies.Clear();
    }
    
    public void StartGame()
    {
        isGameRunning = true;
        SpawnEnemies();
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f;
    }

    public void RestartLevel()
    {
        StartCoroutine(RestartRoutine());
    }

    private IEnumerator RestartRoutine()
    {
        yield return new WaitForSeconds(1f);
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneRoutine(sceneName));
    }

    private IEnumerator LoadSceneRoutine(string sceneName)
    {
        AsyncOperation async = SceneManager.LoadSceneAsync(sceneName);
        while (!async.isDone)
            yield return null;
    }
    
    public void SaveData()
    {
        PlayerPrefs.SetInt("PlayerScore", playerScore);
        PlayerPrefs.SetInt("LevelProgress", levelProgress);
        PlayerPrefs.Save();
    }

    public void LoadData()
    {
        playerScore = PlayerPrefs.GetInt("PlayerScore", 0);
        levelProgress = PlayerPrefs.GetInt("LevelProgress", 0);
    }

    private void OnApplicationQuit()
    {
        SaveData();
    }
}

