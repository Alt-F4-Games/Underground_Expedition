using UnityEngine;
using UnityEngine.UI;

namespace Network.Cinematic
{
    public class SpectatorToggleUI : MonoBehaviour
    {
        [Header("Referencias UI")]
        [Tooltip("Arrastra aquí el componente Toggle de la UI")]
        [SerializeField] private Toggle _spectatorToggle;

        private void Start()
        {
            if (_spectatorToggle != null)
            {
                // Sincroniza el Toggle visual con el estado actual de la variable
                _spectatorToggle.isOn = RoomConfig.IsSpectator;

                // Escucha cada vez que se hace clic en la casilla
                _spectatorToggle.onValueChanged.AddListener(UpdateSpectatorStatus);
            }
            else
            {
                Debug.LogWarning("[Modo Director] Falla: No se asignó el Toggle en el Inspector.");
            }
        }

        private void UpdateSpectatorStatus(bool isSpectator)
        {
            RoomConfig.IsSpectator = isSpectator;
            Debug.Log($"[Modo Director] Entrar como espectador: {RoomConfig.IsSpectator}");
        }

        private void OnDestroy()
        {
            // Limpiamos la suscripción al evento para evitar errores en memoria
            if (_spectatorToggle != null)
            {
                _spectatorToggle.onValueChanged.RemoveListener(UpdateSpectatorStatus);
            }
        }
    }
}