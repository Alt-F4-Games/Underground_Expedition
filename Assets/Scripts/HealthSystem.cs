using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour, IDamageable
{
   [Header("Stats")]
   [SerializeField] protected int _maxHealth = 100;
   protected int _currentHealth;
   protected bool _isAlive;
   
   public event Action<int, int> OnHealthChanged;
   public int CurrentHealth => _currentHealth;
   public int MaxHealth => _maxHealth;
   public bool IsAlive => _isAlive;
   
   public void Awake()
   {
      _isAlive = true;
      _currentHealth = _maxHealth;
      Debug.Log("Health:" +  _currentHealth);
   }

   public virtual void TakeDamage(int damage)
   {
      if (damage <= 0)
      {
         Debug.LogError("Damage amount cannot be negative or zero");
         return;
      }
      
      if(!_isAlive) return;
      
      _currentHealth = Mathf.Max(_currentHealth - damage, 0);     //returns 0 if current is negative
      Debug.Log($"{gameObject.name} has taken: {damage} of damage");
      Debug.Log("Health:" + _currentHealth);

      if (_currentHealth <= 0)
      {
         Death();
      }
   }

   public virtual void Death()
   {
      if (!_isAlive) return;
      Debug.Log($"{gameObject.name} has died.");
      Destroy(gameObject);
      _isAlive = false;
   }

   public virtual void Heal(int heal)
   {
      if (heal <= 0)
      {
         Debug.LogError("Heal amount cannot be negative or zero");
         return;
      }

      if (!_isAlive)
      {
         Debug.Log($"{gameObject.name} cannot heal because it is dead.");
         return;
      }

      if (_currentHealth < _maxHealth)
      {
         _currentHealth = Mathf.Min(_currentHealth + heal, _maxHealth); // Limita inmediatamente
         Debug.Log($"{gameObject.name} has heal {heal}. Actual health: {_currentHealth}");
      }
      else
      {
         Debug.Log($"{gameObject.name} is fully healed.");
      }
   }
   
   public virtual void Revive()
   {
      if (!_isAlive)
      {
         _isAlive = true;
         _currentHealth = _maxHealth;
         Debug.Log($"{gameObject.name} has revived.");
      }
   }

   public void SetMaxHealth(int maxHealth)
   {
      if (maxHealth <= 0)
      {
         Debug.LogError("Max health cannot be negative or zero");
         return;
      }
      _maxHealth = maxHealth;
      _currentHealth = Mathf.Min(_currentHealth, _maxHealth);
   }
}
