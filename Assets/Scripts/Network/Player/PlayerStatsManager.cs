using Events;
using Fusion;
using Tools.EventSystem;
using UnityEngine;

namespace Network
{
    public class PlayerStatsManager : NetworkBehaviour
    {
        [SerializeField] private int _plusMaxHealth = 10;
        [SerializeField] private float _plusMaxStamimna = 5;
        [SerializeField] private int _plusPlayerDamage;
        
        private PlayerStatsEvent  _playerStatsEvent = new ();
        
        public void ApplyStatsServer()
        {
            if (!HasStateAuthority) return;

            _playerStatsEvent.MaxHealth =  _plusMaxHealth;
            _playerStatsEvent.MaxStamina =  _plusMaxStamimna;
            _playerStatsEvent.PlayerDamage =  _plusPlayerDamage;
                                        
            EventController.Instance.TriggerEvent(_playerStatsEvent);
        }
        
    }
}