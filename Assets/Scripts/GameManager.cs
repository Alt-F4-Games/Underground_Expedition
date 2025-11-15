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

    [Header("Respawn Points")]
    private RespawnPoint currentRespawnPoint;

    [Header("Enemy Settings")]
    [SerializeField] private Spawner spawner;
    [SerializeField] private List<string> enemyIDsToSpawn = new();
    [SerializeField] private List<string> spawnPointsToUse = new();

    [Header("Respawn Settings")]
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

    // PLAYER

    public void SpawnPlayer()
    {
        if (playerPrefab == null)
            return;

        if (currentPlayer != null)
            Destroy(currentPlayer);

        Vector3 spawnPos = playerSpawnPoint.position;
        Quaternion spawnRot = playerSpawnPoint.rotation;

        if (currentRespawnPoint != null)
        {
            spawnPos = currentRespawnPoint.transform.position;
            spawnRot = currentRespawnPoint.transform.rotation;
        }

        currentPlayer = Instantiate(playerPrefab, spawnPos, spawnRot);

        var health = currentPlayer.GetComponent<HealthSystem>();

        if (UIManager.instance != null)
        {
            UIManager.instance.SetPlayer(currentPlayer);
            UIManager.instance.SetExperienceSystems(
                Player.ExperienceSystem.instance,
                Player.LevelSystem.instance
            );
        }

        if (health != null)
            StartCoroutine(WatchPlayerHealth(health));
    }

    public void SetRespawnPoint(RespawnPoint point)
    {
        currentRespawnPoint = point;
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
        // Implementar a futuro
    }

    // ENEMIES

    private void PrepareEnemySpawnData()
    {
        enemySpawnData.Clear();
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

        ClearEnemies();
        PrepareEnemySpawnData();

        foreach (var data in enemySpawnData)
        {
            SpawnSingleEnemy(data);
        }
    }

    private void SpawnSingleEnemy(EnemySpawnData data)
    {
        var enemy = spawner.Spawn(data.enemyID, data.spawnPointID);

        if (enemy != null)
        {
            activeEnemies.Add(enemy);

            var health = enemy.GetComponent<HealthSystem>();
            if (health != null)
            {
                StartCoroutine(WatchEnemyHealth(enemy, health, data));
            }
        }
    }

    private IEnumerator WatchEnemyHealth(GameObject enemy, HealthSystem health, EnemySpawnData data)
    {
        while (health != null && health.IsAlive)
            yield return null;

        if (enemy != null)
            activeEnemies.Remove(enemy);

        yield return new WaitForSeconds(data.respawnTime);

        SpawnSingleEnemy(data);
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
        while (!async.isDone)
            yield return null;
    }

    // SAVE DATA

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
