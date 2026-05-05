using Fusion;
using UnityEngine;
using Network.SceneManagement;

namespace Network.Interaction.SceneTransition
{
    public class GateTerminal : InteractableBase
    {
        [Header("References")]
        [SerializeField] private PlayerAreaGate areaGate;

        [Header("Scene")]
        [SerializeField] private string sceneName;

        public override bool CanInteract(PlayerRef player)
        {
            if (areaGate == null) return false;

            return areaGate.AreAllPlayersInside;
        }

        public override void OnInteract(NetworkPlayerController player)
        {
            if (Runner == null || !Runner.IsRunning) return;

            if (!Runner.IsServer) return;

            if (player.Object.InputAuthority != Runner.LocalPlayer) return;

            if (areaGate == null || !areaGate.AreAllPlayersInside) return;

            Runner.LoadScene(sceneName);
        }

        public override string GetInteractPrompt()
        {
            if (areaGate == null) return base.GetInteractPrompt();

            return areaGate.AreAllPlayersInside
                ? "Enter next area"
                : "Waiting for all players...";
        }
    }
}