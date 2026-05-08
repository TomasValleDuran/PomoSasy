using UnityEngine;

namespace Player
{
    /// <summary>Legacy component replaced by per-attack visuals spawned from AttackSlot.</summary>
    public class PlayerAttackAreaVisualizer : MonoBehaviour
    {
        [SerializeField] private Transform radiusVisual;

        private void Awake()
        {
            if (radiusVisual != null && radiusVisual.TryGetComponent(out SpriteRenderer sr))
                sr.enabled = false;
            enabled = false;
        }
    }
}
