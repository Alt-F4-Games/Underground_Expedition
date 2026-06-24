using Audio.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Audio.BGM
{
    public class BGMAudioManager : MonoBehaviour
    {
        [SerializeField] private AudioCollection audioCollection;
        [SerializeField] private BGMDatabase database;

        private AudioEmitter _currentEmitter;
        private string _currentMusicKey;

        public static BGMAudioManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void OnDestroy()
        {
            StopCurrentMusic();
        }
        
        private void OnApplicationQuit()
        {
            Debug.Log("[BGM] Application Quit");
        }

        private void Start()
        {
            EvaluateCurrentScene();
        }

        private void EvaluateCurrentScene()
        {
            string sceneName = SceneManager.GetActiveScene().name;

            Debug.Log($"[BGM] Evaluating scene: {sceneName}");

            if (!database.TryGetMusic(sceneName, out string musicKey))
            {
                Debug.Log($"[BGM] No music configured for scene {sceneName}");
                return;
            }

            Debug.Log($"[BGM] Playing {musicKey}");

            PlayMusic(musicKey);
        }

        private void PlayMusic(string audioKey)
        {
            Debug.Log($"[BGM] Requested music: {audioKey}");

            if (_currentMusicKey == audioKey)
                return;

            StopCurrentMusic();

            if (!audioCollection.TryGetAudio(audioKey, out var sound))
            {
                Debug.LogError($"[BGM] Audio key not found: {audioKey}");
                return;
            }

            Debug.Log($"[BGM] Found FMOD event for: {audioKey}");

            _currentEmitter = AudioManager.Instance.CreateEmitter(sound, transform);

            _currentEmitter.Play();

            _currentMusicKey = audioKey;

            Debug.Log($"[BGM] Started emitter for: {audioKey}");
        }

        private void StopCurrentMusic()
        {
            if (_currentEmitter == null)
                return;

            Debug.Log($"[BGM] Stopping {_currentMusicKey}");

            _currentEmitter.Stop();
            _currentEmitter.Release();

            _currentEmitter = null;
            _currentMusicKey = null;
        }
        
        public void RefreshMusic()
        {
            EvaluateCurrentScene();
        }
    }
}