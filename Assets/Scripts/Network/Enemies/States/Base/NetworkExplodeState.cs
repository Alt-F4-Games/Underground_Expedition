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
            float force = _enemy.ExplosionForce;

            Collider[] hits = Physics.OverlapSphere(_enemy.transform.position, radius, _enemy.PlayerLayer);

            foreach (var hit in hits)
            {
                // TEST ============================================
                var characterController = hit.GetComponent<CharacterController>();
                if (characterController != null)
                {
                    Vector3 dir = (hit.transform.position - _enemy.transform.position).normalized;
                    
                    characterController.attachedRigidbody.AddForce(dir * force, ForceMode.Impulse);
                }
                // =================================================
                
                if (hit.TryGetComponent(out IDamageable damageable))
                {
                    damageable.TakeDamage(damage);
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
