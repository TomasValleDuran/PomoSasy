using Attack;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float dashSpeed = 10f;

        [Header("Attack")]
        [SerializeField] private AttackData attackData;

        public float MoveSpeed => moveSpeed;
        public float DashSpeed => dashSpeed;
        public AttackData AttackData => attackData;
    }
}
