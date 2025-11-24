/*
 * GameManager
 * Central controller for:
 *  - Player spawning and respawning
 *  - Enemy spawning, death tracking, and timed respawning
 *  - Global game state (pause, restart, scene loading)
 *  - Simple persistence (score, progress)
 *
 * Behaviour summary:
 *  - Initializes systems and spawns player on start
 *  - Respawns player when health reaches zero
 *  - Spawns enemies based on ID and spawn point definitions
 *  - Automatically respawns enemies after a given delay
 *  - Maintains a singleton instance across scenes
 */

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

[System.Serializable]
public class EnemySpawnData
{
    public string enemyID;
    public string spawnPointID;
    public float respawnTime;

    public EnemySpawnData(string id, string sp, float time)
    {
        enemyID = id;
        spawnPointID = sp;
        respawnTime = time;
    }
}

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
    [SerializeField] private List<float> respawnTimes = new();

    private List<EnemySpawnData> enemySpawnData = new();
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
        // Singleton initialization
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
        // Delayed boot to ensure all systems (UI, XP, etc.) are loaded
        StartCoroutine(InitGameRoutine());
    }

    private IEnumerator InitGameRoutine()
    {
        // Small delay before initialization
        yield return new WaitForSeconds(0.3f);

        // Connect UI to experience systems if available
        if (UIManager.instance != null)
        {
            UIManager.instance.SetExperienceSystems(
                Player.ExperienceSystem.instance,
                Player.LevelSystem.instance
            );
        }

        // Spawn player and start match logic
        SpawnPlayer();
        StartGame();
    }
    
    // PLAYER MANAGEMENT

    public void SpawnPlayer()
    {
        if (playerPrefab == null) return;

        // Destroy previous player if respawning
        if (currentPlayer != null)
            Destroy(currentPlayer);

        // Get last activated respawn point
        Transform respawnTransform = RespawnSystem.Instance.GetCurrentRespawnTransform();

        // Default spawn position (fallback)
        Vector3 spawnPos = playerSpawnPoint != null ? playerSpawnPoint.position : Vector3.zero;
        Quaternion spawnRot = playerSpawnPoint != null ? playerSpawnPoint.rotation : Quaternion.identity;

        // Override with checkpoint position if available
        if (respawnTransform != null)
        {
            spawnPos = respawnTransform.position;
            spawnRot = respawnTransform.rotation;
        }

        // Instantiate player
        currentPlayer = Instantiate(playerPrefab, spawnPos, spawnRot);

        var health = currentPlayer.GetComponent<HealthSystem>();

        // Register player in UI
        if (UIManager.instance != null)
        {
            UIManager.instance.SetPlayer(currentPlayer);
            UIManager.instance.SetExperienceSystems(
                Player.ExperienceSystem.instance,
                Player.LevelSystem.instance
            );
        }

        // Wait for player death
        if (health != null)
            StartCoroutine(WatchPlayerHealth(health));
    }

    private IEnumerator WatchPlayerHealth(HealthSystem health)
    {
        // Wait until health reaches zero
        while (health != null && health.IsAlive)
            yield return null;

        HandlePlayerDeath();
    }

    private void HandlePlayerDeath()
    {
        // Respawn with delay
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
        // Placeholder for clearing buffs, states, etc.
    }

    // ENEMY MANAGEMENT

    private void PrepareEnemySpawnData()
    {
        enemySpawnData.Clear();

        // Align lists to avoid out-of-range issues
        int count = Mathf.Min(enemyIDsToSpawn.Count, spawnPointsToUse.Count, respawnTimes.Count);

        for (int i = 0; i < count; i++)
        {
            enemySpawnData.Add(new EnemySpawnData(
                enemyIDsToSpawn[i],
                spawnPointsToUse[i],
                respawnTimes[i]
            ));
        }
    }

    public void SpawnEnemies()
    {
        if (spawner == null) return;

        // Clear previous enemies on restart
        ClearEnemies();
        PrepareEnemySpawnData();

        // Spawn all defined enemies
        foreach (var data in enemySpawnData)
            SpawnSingleEnemy(data);
    }

    private void SpawnSingleEnemy(EnemySpawnData data)
    {
        var enemy = spawner.Spawn(data.enemyID, data.spawnPointID);

        if (enemy != null)
        {
            activeEnemies.Add(enemy);

            var health = enemy.GetComponent<HealthSystem>();

            // Watch for enemy death
            if (health != null)
                StartCoroutine(WatchEnemyHealth(enemy, health, data));
        }
    }

    private IEnumerator WatchEnemyHealth(GameObject enemy, HealthSystem health, EnemySpawnData data)
    {
        // Wait until enemy dies
        while (health != null && health.IsAlive)
            yield return null;

        if (enemy != null)
            activeEnemies.Remove(enemy);

        // Wait before respawn
        yield return new WaitForSeconds(data.respawnTime);

        SpawnSingleEnemy(data);
    }

    public void ClearEnemies()
    {
        // Destroy all current enemies
        foreach (var e in activeEnemies)
        {
            if (e != null)
                Destroy(e);
        }

        activeEnemies.Clear();
    }

    // GAME STATE

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

        // Wait until scene has fully loaded
        while (!async.isDone)
            yield return null;
    }

    // SAVE SYSTEM

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

    // RESPAWN POINT LOCATOR

    public RespawnPoint FindSpawnPointByID(string id)
    {
        // Search all RespawnPoints in the active scene
        RespawnPoint[] points = FindObjectsOfType<RespawnPoint>();

        foreach (var p in points)
        {
            if (p.respawnID == id)
                return p;
        }

        return null;
    }
}
