using Attack;
using System.Collections.Generic;
using UnityEngine;

namespace Data
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "Scriptable Objects/PlayerData")]
    public class PlayerData : ScriptableObject
    {
        [SerializeField] private float moveSpeed = 5f;
        [SerializeField] private float dashSpeed = 10f;

        [Header("Attack")]
        [SerializeField] private List<AttackData> startingAttacks = new();

        public float MoveSpeed => moveSpeed;
        public float DashSpeed => dashSpeed;
        public IReadOnlyList<AttackData> StartingAttacks => startingAttacks;
        public AttackData AttackData => startingAttacks.Count > 0 ? startingAttacks[0] : null;
    }
}
