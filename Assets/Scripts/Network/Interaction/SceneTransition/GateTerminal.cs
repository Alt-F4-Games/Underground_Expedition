using Fusion;
using UnityEngine;
using Network.SceneManagement;

namespace Network.Interaction.SceneTransition
{
    public class GateTerminal : InteractableBase
    {
        [Header("Scene")]
        [SerializeField] private string sceneName;
        
        private bool _sceneLoadTriggered = false;

        public override bool CanInteract(PlayerRef player)
        {
            if (Runner == null || !Runner.IsRunning) return false;
            
            return Runner.IsServer && player == Runner.LocalPlayer;
        }

        public override void OnInteract(NetworkPlayerController player)
        {
            if (Runner == null || !Runner.IsRunning) return;
            
            if (!Runner.IsServer) return;
            if (player.Object.InputAuthority != Runner.LocalPlayer) return;
            
            if (_sceneLoadTriggered) return;
            _sceneLoadTriggered = true;

            int buildIndex = UnityEngine.SceneManagement.SceneUtility.GetBuildIndexByScenePath(sceneName);
            if (buildIndex >= 0)
            {
                Runner.LoadScene(SceneRef.FromIndex(buildIndex));
            }
            else
            {
                Debug.LogError($"[GateTerminal] La escena '{sceneName}' no se encuentra en el Build Settings.");
                _sceneLoadTriggered = false; 
            }
        }

        public override string GetInteractPrompt()
        {
            if (Runner != null && Runner.IsServer)
            {
                return "Force Scene Transition";
            }
            
            return "Waiting for Host...";
        }
    }
}