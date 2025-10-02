using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform attackPivot;

    [Header("Attack Settings")]
    [SerializeField] private float attackWidth = 1f;
    [SerializeField] private float attackHeight = 1.5f;
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 0.8f;
    [SerializeField] private float pushForce = 4f;

    [Header("Layer Mask")]
    [SerializeField] private LayerMask hittableLayers;

    private bool _canAttack = true;
    
    public void OnAttack(InputAction.CallbackContext context)
    {
        if (context.performed && _canAttack)
        {
            PerformAttack();
        }
    }

    private void PerformAttack()
    {
        _canAttack = false;
        
        Vector3 halfExtents = new Vector3(attackWidth * 0.5f, attackHeight * 0.5f, attackRange * 0.5f);
        
        Vector3 boxCenter = attackPivot.position + attackPivot.forward * (attackRange * 0.5f);
        
        Collider[] hits = Physics.OverlapBox(boxCenter, halfExtents, attackPivot.rotation, hittableLayers);

        foreach (var hit in hits)
        {
            if (hit.TryGetComponent<HealthSystem>(out var health))
            {
                health.TakeDamage(attackDamage);
                
                if (hit.attachedRigidbody != null)
                {
                    Vector3 pushDir = (hit.transform.position - transform.position).normalized;
                    hit.attachedRigidbody.AddForce(pushDir * pushForce, ForceMode.Impulse);
                }
            }
        }

        // Cooldown
        StartCoroutine(ResetAttackCooldown());
    }

    private IEnumerator ResetAttackCooldown()
    {
        yield return new WaitForSeconds(attackCooldown);
        _canAttack = true;
    }
    
    private void OnDrawGizmosSelected()
    {
        if (attackPivot == null) return;

        Gizmos.color = Color.red;
        Vector3 halfExtents = new Vector3(attackWidth * 0.5f, attackHeight * 0.5f, attackRange * 0.5f);
        Vector3 boxCenter = attackPivot.position + attackPivot.forward * (attackRange * 0.5f);
        Gizmos.matrix = Matrix4x4.TRS(boxCenter, attackPivot.rotation, Vector3.one);
        Gizmos.DrawWireCube(Vector3.zero, halfExtents * 2);
    }
}