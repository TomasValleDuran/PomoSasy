using UnityEngine;

namespace Health
{
    [CreateAssetMenu(fileName = "HealthData", menuName = "Scriptable Objects/Health/HealthData")]
    public class HealthData : ScriptableObject
    {
        [SerializeField] private float maxHealth = 100f;

        public float MaxHealth => maxHealth;
    }
}
