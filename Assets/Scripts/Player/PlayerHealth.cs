using UnityEngine;

namespace Player
{ 
    public class PlayerHealth : HealthSystem
    {
        private PlayerController _playerController;
        private Animator _animator;

        private void Start()
        {
            _playerController = GetComponent<PlayerController>();
            _animator = GetComponent<Animator>();
        }

        public override void Death()
        {
            if (!_isAlive) return;

            Debug.Log("PLAYER has died.");

            _isAlive = false;

            // Desactivar controles del jugador
            if (_playerController != null)
                _playerController.enabled = false;

            // Animación de muerte si existe
            // Notificar al GameManager
            
        }

        public override void Revive()
        {
            base.Revive();

            // Reactivar controles después de revivir
            if (_playerController != null)
                _playerController.enabled = true;
        }
    }

}