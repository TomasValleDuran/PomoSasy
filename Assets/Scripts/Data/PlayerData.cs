using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 200f;
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float attackDamage = 10f;
        [SerializeField] private float dashSpeed = 10f;
    
        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float DashSpeed => dashSpeed;
    }
}
