using System;
using UnityEngine;

namespace Health
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private HealthData healthData;
        [SerializeField] private bool logHealthEvents = false;

        private float maxHealthMultiplier = 1f;

        public float MaxHealth => healthData != null ? healthData.MaxHealth * maxHealthMultiplier : 0f;
        public float CurrentHealth { get; private set; }

        public event Action OnDeath;
        public event Action<float> OnDamaged;

        private void Awake()
        {
            ResetHealth();
        }

        private void OnEnable()
        {
            ResetHealth();
        }

        public void TakeDamage(float damage)
        {
            if (CurrentHealth <= 0f) return;

            float before = CurrentHealth;
            CurrentHealth = Mathf.Max(0f, CurrentHealth - damage);
            OnDamaged?.Invoke(CurrentHealth);

            if (logHealthEvents)
                Debug.Log($"[Health] {name}: took {damage:F1} damage — {before:F1} → {CurrentHealth:F1} / {MaxHealth:F1} HP", this);

            if (CurrentHealth <= 0f)
            {
                if (logHealthEvents)
                    Debug.Log($"[Health] {name}: died (0 HP)", this);
                OnDeath?.Invoke();
            }
        }

        public void ResetHealth()
        {
            CurrentHealth = MaxHealth;
            OnDamaged?.Invoke(CurrentHealth);

            if (logHealthEvents)
                Debug.Log($"[Health] {name}: reset to {CurrentHealth:F1} / {MaxHealth:F1} HP", this);
        }

        public void MultiplyMaxHealth(float multiplier)
        {
            maxHealthMultiplier *= multiplier;
        }

        /// <summary>Current max-health multiplier, exposed so a save can snapshot it.</summary>
        public float MaxHealthMultiplier => maxHealthMultiplier;

        /// <summary>
        /// Restore health to an absolute snapshot (multiplier + current HP). Used when loading a
        /// saved game; overrides any multiplier accumulated during scene init.
        /// </summary>
        public void RestoreState(float savedCurrentHealth, float savedMaxHealthMultiplier)
        {
            maxHealthMultiplier = Mathf.Max(0.01f, savedMaxHealthMultiplier);
            CurrentHealth = Mathf.Clamp(savedCurrentHealth, 0f, MaxHealth);
            OnDamaged?.Invoke(CurrentHealth);
        }
    }
}
