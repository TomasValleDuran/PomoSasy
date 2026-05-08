using UnityEngine;

namespace Attack
{
    public readonly struct AttackContext
    {
        public readonly Transform attacker;
        public readonly Transform target;
        public readonly float damage;
        public readonly float range;
        public readonly float deltaTime;

        public AttackContext(Transform attacker, Transform target, float damage, float range, float deltaTime)
        {
            this.attacker = attacker;
            this.target = target;
            this.damage = damage;
            this.range = range;
            this.deltaTime = deltaTime;
        }
    }
}
