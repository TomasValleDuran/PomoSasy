using Enemy.Attacks;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyObject")]
    public class EnemyData : ScriptableObject
    {
        [Header("Stats")]
        [SerializeField] private float maxHealth = 50f;
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float attackDamage = 10f;

        [Header("Attack")]
        [SerializeField] private AttackBehavior attackBehavior;
        [SerializeField] private float windupDuration = 0.5f;
        [SerializeField] private float attackCooldown = 1.5f;
        [SerializeField] private float attackRange = 1f;

        [Header("Reward")]
        [SerializeField] private int pointsOnDeath = 10;

        public float MaxHealth => maxHealth;
        public float MoveSpeed => moveSpeed;
        public float AttackDamage => attackDamage;
        public float AttackRange => attackRange;
        public AttackBehavior AttackBehavior => attackBehavior;
        public float WindupDuration => windupDuration;
        public float AttackCooldown => attackCooldown;
        public int PointsOnDeath => pointsOnDeath;
    }
}
