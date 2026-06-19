using UnityEngine;

namespace Attack
{
    public abstract class AttackBehavior : ScriptableObject
    {
        public abstract bool Execute(in AttackContext ctx);

        /// <summary>
        /// Gate checked by <see cref="AttackSlot"/> before firing. When this returns false the slot
        /// stays idle: it does not execute, spend cooldown, play audio, or trigger the attack
        /// animation. Default is true so attacks that don't need a target (enemy AI, dashes) keep
        /// firing as before; the player's auto-attacks override it so they only fire when there is
        /// actually an enemy within range.
        /// </summary>
        public virtual bool HasTargetInRange(in AttackContext ctx) => true;

        public virtual GameObject CreateVisual(Transform attacker) => null;

        public virtual void OnEquip(Transform owner) { }

        public virtual void OnUnequip(Transform owner) { }

        /// <summary>True when <paramref name="ctx"/> has a live target within <paramref name="range"/> world units.</summary>
        protected static bool TargetWithinRange(in AttackContext ctx, float range)
        {
            if (ctx.attacker == null || ctx.target == null)
                return false;
            if (range <= 0f)
                return true; // unbounded range: any known target counts
            return Vector2.Distance(ctx.attacker.position, ctx.target.position) <= range;
        }
    }
}
