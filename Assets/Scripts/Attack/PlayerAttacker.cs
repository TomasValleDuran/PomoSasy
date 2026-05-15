using UnityEngine;
using Data;
using Health;

namespace Controllers
{
    [RequireComponent(typeof(Attack.PlayerAttackLoadout))]
    public class PlayerAttacker : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private Attack.PlayerAttackLoadout loadout;
        [Header("Targeting")]
        [SerializeField] [Min(0f)] private float targetingRange = 12f;
        [SerializeField] private LayerMask targetMask = ~0;

        private void Awake()
        {
            if (loadout == null)
                loadout = GetComponent<Attack.PlayerAttackLoadout>();
            if (loadout == null)
                loadout = gameObject.AddComponent<Attack.PlayerAttackLoadout>();

            loadout.Initialize(playerData);
        }

        private void Update()
        {
            if (loadout == null)
                return;

            Transform target = FindNearestTarget();
            var ctx = new Attack.AttackContext(
                transform,
                target,
                0f,
                0f,
                Time.deltaTime
            );

            for (int i = 0; i < loadout.Slots.Count; i++)
                loadout.Slots[i].Tick(ctx);
        }

        private Transform FindNearestTarget()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, targetingRange, targetMask);
            Transform nearest = null;
            float bestDistanceSqr = float.MaxValue;
            Transform selfRoot = transform.root;

            for (int i = 0; i < hits.Length; i++)
            {
                Collider2D hit = hits[i];
                if (hit == null || hit.transform.root == selfRoot)
                    continue;

                IDamageable damageable = hit.GetComponentInParent<IDamageable>();
                if (damageable is not MonoBehaviour mb || !mb.gameObject.activeInHierarchy)
                    continue;

                Vector2 delta = (Vector2)mb.transform.position - (Vector2)transform.position;
                float d2 = delta.sqrMagnitude;
                if (d2 >= bestDistanceSqr)
                    continue;

                bestDistanceSqr = d2;
                nearest = mb.transform;
            }

            return nearest;
        }
    }
}
