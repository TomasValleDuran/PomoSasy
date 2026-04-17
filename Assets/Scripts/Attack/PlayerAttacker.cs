using Data;
using UnityEngine;

namespace Controllers
{
    public class PlayerAttacker : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;

        private float _cooldownTimer;

        private void Update()
        {
            if (playerData?.AttackData?.AttackBehavior == null)
                return;

            _cooldownTimer -= Time.deltaTime;
            if (_cooldownTimer > 0f)
                return;

            playerData.AttackData.AttackBehavior.Execute(transform, null, playerData.AttackData.Damage, playerData.AttackData.AttackRange);
            _cooldownTimer = playerData.AttackData.Cooldown;
        }
    }
}
