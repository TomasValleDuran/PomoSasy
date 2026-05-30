using Data;
using Input;
using UnityEngine;
using Upgrades;

namespace Controllers
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private PlayerData playerData;
        [SerializeField] private InputHandler inputHandler;
        [SerializeField] private PlayerUpgradeModifiers upgradeModifiers;

        private void Start()
        {
            if (upgradeModifiers == null)
                upgradeModifiers = GetComponent<PlayerUpgradeModifiers>();
            StartCoroutine(RegisterWhenReady());
        }

        private System.Collections.IEnumerator RegisterWhenReady()
        {
            while (!GameManagerScript.Instance)
                yield return null;

            GameManagerScript.Instance.RegisterPlayer(transform);
        }
        private void Update()
        {
            Move();
        }

        private void Move()
        {
            Vector2 input = inputHandler.MoveInput.normalized;

            Vector3 movement = new Vector3(input.x, input.y, 0f);
            float moveSpeedMultiplier = upgradeModifiers != null ? upgradeModifiers.MoveSpeedMultiplier : 1f;
            transform.position += movement * (playerData.MoveSpeed * moveSpeedMultiplier * Time.deltaTime);
        }
    }
}