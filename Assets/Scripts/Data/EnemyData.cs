using Attack;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyObject")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyType;

        [Header("Stats")]
        [SerializeField] private float moveSpeed = 2f;

        [Header("Attack")]
        [SerializeField] private AttackData attackData;

        [Header("Reward")]
        [SerializeField] private int pointsOnDeath = 10;

        public string EnemyType => string.IsNullOrWhiteSpace(enemyType) ? name : enemyType;
        public float MoveSpeed => moveSpeed;
        public AttackData AttackData => attackData;
        public int PointsOnDeath => pointsOnDeath;
    }
}
