using System;
using UnityEngine;

namespace Health
{
    public class HealthComponent : MonoBehaviour, IDamageable
    {
        [SerializeField] private HealthData healthData;
        [SerializeField] private bool logHealthEvents = true;

        public float MaxHealth => healthData != null ? healthData.MaxHealth : 0f;
        public float CurrentHealth { get; private set; }

        public event Action OnDeath;
        public event Action<float> OnDamaged;

        private void Awake()
        {
            CurrentHealth = MaxHealth;
            if (logHealthEvents)
                Debug.Log($"[Health] {name}: initialized — {CurrentHealth:F1} / {MaxHealth:F1} HP", this);
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
    }
}
