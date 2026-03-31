using UnityEngine;

namespace Network.Enemies
{
    public class NetworkExplodeState : INetworkState
    {
        private NetworkSkullController _enemy;
        private float _explodeTimer;
        private bool _hasExploded;  
    
        public void Enter(NetworkEnemyController enemy)
        {
            _enemy = (NetworkSkullController)enemy;

            _enemy.Agent.isStopped = true;

            _explodeTimer = _enemy.AttackCooldown;
            _hasExploded = false;
        
            _enemy.PlayExplodeChargeFeedback(); // Red Flash ¿?
        }

        public void Update()
        {
            if (_hasExploded) return;

            _explodeTimer -= Time.deltaTime;

            if (_explodeTimer <= 0f)
            {
                Explode();
            }
        }
    
        private void Explode()
        {
            if (!_enemy.HasStateAuthority) return;
            
            _hasExploded = true;

            float radius = _enemy.AttackRange;
            int damage = _enemy.AttackDamage;
            float duration = _enemy.StunDuration;

            Collider[] hits = Physics.OverlapSphere(_enemy.transform.position, radius, _enemy.PlayerLayer);

            foreach (var hit in hits)
            {
                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);
                }

                if (hit.TryGetComponent(out IStunnable stunnable))
                {
                    stunnable.ApplyStun(duration);
                }
            }
        
            // VFx 
            _enemy.PlayExplosionFeedback();
            
            // TO DO: Research how to add a delay so it waits for the animation to play before despawning.
            _enemy.Despawn();
        }
    
        public void ResetTimer()
        {
            _explodeTimer = _enemy.AttackCooldown;
        }
    
        public void Exit()
        {
            // Logic when exiting attack state (empty for now)
        }

        NetworkEnemyState INetworkState.GetStateType() => NetworkEnemyState.Exploding;
    }
}
