using UnityEngine;

namespace Attack
{
    [CreateAssetMenu(fileName = "AttackData", menuName = "Scriptable Objects/Attack/AttackData")]
    public class AttackData : ScriptableObject
    {
        [Header("Combat")]
        [SerializeField] [Min(0)] private float damage = 10f;
        [SerializeField] [Min(0)] private float cooldown = 0.5f;
        [SerializeField] private AttackBehavior attackBehavior;

        [Header("Range")]
        [Tooltip("Enemy: engage / melee strike distance. Player BasicAttack: overlap radius (world units).")]
        [SerializeField] [Min(0)] private float attackRange = 1f;
        
        [Header("Enemy AI (optional)")]
        [Tooltip("Wind-up before the attack executes. Use 0 for instant attacks (e.g. player).")]
        [SerializeField] [Min(0)] private float windupDuration;

        [Header("Audio")]
        [Tooltip("Played when this specific AttackData finishes executing. This is per attack, not per behavior type.")]
        [SerializeField] private AudioClip attackSfx;

        public float Damage => damage;
        public float Cooldown => cooldown;
        public AttackBehavior AttackBehavior => attackBehavior;
        public float WindupDuration => windupDuration;
        public float AttackRange => attackRange;
        public AudioClip AttackSfx => attackSfx;
    }
}
