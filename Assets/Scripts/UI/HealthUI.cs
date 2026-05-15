using Health;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class HealthUI : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        private HealthComponent playerHealth;

        private void Start()
        {
            var player = GameObject.FindGameObjectWithTag("Player");
            if (player == null || !player.TryGetComponent(out playerHealth))
            {
                Debug.LogWarning("HealthUI could not find the player's HealthComponent.", this);
                return;
            }

            playerHealth.OnDamaged += UpdateUI;
            UpdateUI(playerHealth.CurrentHealth);
        }

        private void OnDestroy()
        {
            if (playerHealth != null)
                playerHealth.OnDamaged -= UpdateUI;
        }

        private void UpdateUI(float currentHealth)
        {
            if (!fillImage) return;

            fillImage.fillAmount = playerHealth.MaxHealth > 0f
                ? Mathf.Clamp01(currentHealth / playerHealth.MaxHealth)
                : 0f;
        }
    }
}
