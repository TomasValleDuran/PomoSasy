using UnityEngine;

namespace Attack
{
    public abstract class AttackBehavior : ScriptableObject
    {
        public abstract bool Execute(in AttackContext ctx);

        public virtual GameObject CreateVisual(Transform attacker) => null;

        public virtual void OnEquip(Transform owner) { }

        public virtual void OnUnequip(Transform owner) { }
    }
}
