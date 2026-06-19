using Attack;
using Movement;
using UnityEngine;
using UnityEngine.Serialization;

namespace Data
{
    [CreateAssetMenu(fileName = "EnemyData", menuName = "Scriptable Objects/EnemyObject")]
    public class EnemyData : ScriptableObject
    {
        [Header("Identity")]
        [SerializeField] private string enemyType;

        [Header("Stats")]
        [SerializeField] private float moveSpeed = 2f;

        [Header("AI")]
        [Tooltip("Movement strategy. Leave empty to chase the player straight (default).")]
        [SerializeField] private MovementPolicy movementPolicy;

        [Header("Attack")]
        [SerializeField] private AttackData attackData;

        [FormerlySerializedAs("pointsOnDeath")]
        [Header("Reward")]
        [SerializeField] private int moneyOnDeath = 10;
        [SerializeField] private int xpOnDeath = 5;

        public string EnemyType => string.IsNullOrWhiteSpace(enemyType) ? name : enemyType;
        public float MoveSpeed => moveSpeed;
        public MovementPolicy MovementPolicy => movementPolicy;
        public AttackData AttackData => attackData;
        public int MoneyOnDeath => moneyOnDeath;
        public int XpOnDeath => xpOnDeath;
    }
}
