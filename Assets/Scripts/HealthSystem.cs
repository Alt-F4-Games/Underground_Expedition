using System;
using UnityEngine;

public class HealthSystem : MonoBehaviour
{
   [Header("Stats")]
   [SerializeField] protected int _maxHealth = 100;
   protected int _currentHealth;
   protected bool _isAlive;
   
   public int CurrentHealth => _currentHealth;
   public int MaxHealth => _maxHealth;

   public void Awake()
   {
      _currentHealth = _maxHealth;
      Debug.Log("Health:" +  _currentHealth);
   }

   public virtual void TakeDamage(int damage)
   {
      if (damage < 0)
      {
         Debug.LogError("Damage amount cannot be negative");
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
      Debug.Log($"{gameObject.name} has died.");
      _isAlive = false;
   }

   public virtual void Heal(int heal)
   {
      if (heal <= 0)
      {
         Debug.LogError("Heal amount cannot be negative or zero");
         return;
      }

      if (_currentHealth < _maxHealth && _isAlive)
      {
         _currentHealth += heal;
         Debug.Log($"{gameObject.name} has healed: {heal}. Actual health: {_currentHealth}");
         
         if (_currentHealth > _maxHealth) {_currentHealth = _maxHealth;}
         
      }
      else
      {
         Debug.Log(
            _isAlive
         ? $"{gameObject.name} has fully healed."
         : $"{gameObject.name} cannot heal, because it´s dead.");
         return ;
      }
   }
   
   public virtual void Revive()
   {
      if (!_isAlive)
      {
         _isAlive = true;
         Debug.Log($"{gameObject.name} has revived.");
      }
   }
}
