using System.Collections.Generic;
using Attack;
using Controllers;
using UnityEngine;

namespace Upgrades
{
    public readonly struct UpgradeOffer
    {
        public readonly UpgradeDefinition Upgrade;
        public readonly int NextLevel;

        public UpgradeOffer(UpgradeDefinition upgrade, int nextLevel)
        {
            Upgrade = upgrade;
            NextLevel = nextLevel;
        }
    }

    [DisallowMultipleComponent]
    public class LevelUpSkillSelectionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private XpManagerScript xpManager;
        [SerializeField] private SkillSelectionUI selectionUI;
        [SerializeField] private List<UpgradeDefinition> availableUpgrades = new();

        [Header("Behavior")]
        [SerializeField] [Range(1, 3)] private int optionsPerSelection = 3;
        [SerializeField] private bool blockPlayerControlsWhileSelecting = true;

        private readonly Dictionary<string, int> _upgradeLevels = new();
        private readonly List<UpgradeOffer> _bufferedOffers = new(3);
        private int _pendingSelections;
        private bool _isShowingSelection;
        private bool _pauseRequestedBySelection;
        private Controllers.PlayerController _playerController;
        private Controllers.PlayerAttacker _playerAttacker;

        private void Awake()
        {
            if (xpManager == null)
                xpManager = XpManagerScript.Instance;
            if (selectionUI != null)
                selectionUI.Hide();
        }

        private void OnEnable()
        {
            if (xpManager == null)
                xpManager = XpManagerScript.Instance;
            if (xpManager == null)
                return;

            xpManager.OnLevelChanged += HandleLevelChanged;
        }

        private void OnDisable()
        {
            if (xpManager != null)
                xpManager.OnLevelChanged -= HandleLevelChanged;

            SetSelectionPause(false);
        }

        private void HandleLevelChanged(int level)
        {
            _pendingSelections++;
            if (!_isShowingSelection)
                ShowNextSelection();
        }

        private void ShowNextSelection()
        {
            if (selectionUI == null)
            {
                SetSelectionPause(false);
                return;
            }

            List<UpgradeDefinition> candidates = GetAvailableCandidates();
            if (candidates.Count == 0)
            {
                _pendingSelections = 0;
                _isShowingSelection = false;
                selectionUI.Hide();
                SetPlayerControlBlocked(false);
                return;
            }

            _bufferedOffers.Clear();
            int count = Mathf.Min(optionsPerSelection, candidates.Count);
            for (int i = 0; i < count; i++)
            {
                int randomIndex = Random.Range(0, candidates.Count);
                UpgradeDefinition selected = candidates[randomIndex];
                candidates.RemoveAt(randomIndex);
                _bufferedOffers.Add(new UpgradeOffer(selected, GetCurrentLevel(selected) + 1));
            }

            _isShowingSelection = true;
            SetPlayerControlBlocked(blockPlayerControlsWhileSelecting);
            selectionUI.Show(_bufferedOffers, HandleUpgradeSelected);
        }

        private List<UpgradeDefinition> GetAvailableCandidates()
        {
            var candidates = new List<UpgradeDefinition>(availableUpgrades.Count);
            for (int i = 0; i < availableUpgrades.Count; i++)
            {
                UpgradeDefinition upgrade = availableUpgrades[i];
                if (upgrade == null)
                    continue;
                if (upgrade.Kind == UpgradeKind.Attack && upgrade.TargetAttack == null)
                    continue;

                if (GetCurrentLevel(upgrade) >= upgrade.MaxLevel)
                    continue;

                candidates.Add(upgrade);
            }

            return candidates;
        }

        private int GetCurrentLevel(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
                return 0;

            return _upgradeLevels.TryGetValue(upgrade.UpgradeId, out int level) ? level : 0;
        }

        // ---- Save / restore -------------------------------------------------

        /// <summary>The configured upgrade pool, exposed so a save can resolve unlockable attacks.</summary>
        public IReadOnlyList<UpgradeDefinition> AvailableUpgrades => availableUpgrades;

        /// <summary>Visit each upgrade's pick count (used to snapshot into a save).</summary>
        public void CaptureLevels(System.Action<string, int> visitor)
        {
            if (visitor == null)
                return;

            foreach (KeyValuePair<string, int> kv in _upgradeLevels)
                visitor(kv.Key, kv.Value);
        }

        public void ClearLevels()
        {
            _upgradeLevels.Clear();
        }

        public void RestoreLevel(string upgradeId, int level)
        {
            if (string.IsNullOrEmpty(upgradeId))
                return;

            _upgradeLevels[upgradeId] = Mathf.Max(0, level);
        }

        private void HandleUpgradeSelected(UpgradeDefinition upgrade)
        {
            if (upgrade == null)
                return;

            int currentLevel = GetCurrentLevel(upgrade);
            if (!TryUnlockAttackIfNeeded(upgrade, currentLevel))
                return;

            PlayerUpgradeApplier applier = ResolveOrCreatePlayerApplier();
            if (applier == null || !applier.ApplyUpgrade(upgrade))
                return;

            int newLevel = currentLevel + 1;
            _upgradeLevels[upgrade.UpgradeId] = Mathf.Min(newLevel, upgrade.MaxLevel);

            _pendingSelections = Mathf.Max(0, _pendingSelections - 1);

            if (_pendingSelections > 0)
            {
                ShowNextSelection();
                return;
            }

            _isShowingSelection = false;
            selectionUI.Hide();
            SetPlayerControlBlocked(false);
        }

        private bool TryUnlockAttackIfNeeded(UpgradeDefinition upgrade, int currentLevel)
        {
            if (upgrade.Kind != UpgradeKind.Attack || currentLevel > 0)
                return true;

            if (upgrade.TargetAttack == null)
                return false;

            if (GameManagerScript.Instance == null || GameManagerScript.Instance.Player == null)
                return false;

            Transform player = GameManagerScript.Instance.Player;
            if (!player.TryGetComponent(out PlayerAttackLoadout loadout))
                return false;

            if (HasAttack(loadout, upgrade.TargetAttack))
                return true;

            loadout.AddAttack(upgrade.TargetAttack);
            return true;
        }

        private static bool HasAttack(PlayerAttackLoadout loadout, AttackData attackData)
        {
            if (loadout == null || attackData == null)
                return false;

            for (int i = 0; i < loadout.Slots.Count; i++)
            {
                if (loadout.Slots[i].AttackData == attackData)
                    return true;
            }

            return false;
        }

        private PlayerUpgradeApplier ResolveOrCreatePlayerApplier()
        {
            if (GameManagerScript.Instance == null || GameManagerScript.Instance.Player == null)
                return null;

            Transform player = GameManagerScript.Instance.Player;
            if (!player.TryGetComponent(out PlayerUpgradeApplier applier))
                applier = player.gameObject.AddComponent<PlayerUpgradeApplier>();

            if (!player.TryGetComponent(out PlayerUpgradeModifiers _))
                player.gameObject.AddComponent<PlayerUpgradeModifiers>();

            return applier;
        }

        private void SetPlayerControlBlocked(bool blocked)
        {
            if (!blockPlayerControlsWhileSelecting || GameManagerScript.Instance == null || GameManagerScript.Instance.Player == null)
            {
                SetSelectionPause(false);
                return;
            }

            if (_playerController == null)
                GameManagerScript.Instance.Player.TryGetComponent(out _playerController);
            if (_playerAttacker == null)
                GameManagerScript.Instance.Player.TryGetComponent(out _playerAttacker);

            if (_playerController != null)
                _playerController.enabled = !blocked;
            if (_playerAttacker != null)
                _playerAttacker.enabled = !blocked;

            SetSelectionPause(blocked);
        }

        private void SetSelectionPause(bool paused)
        {
            if (GameManagerScript.Instance == null)
                return;

            if (paused)
            {
                if (_pauseRequestedBySelection)
                    return;

                GameManagerScript.Instance.RequestPause();
                _pauseRequestedBySelection = true;
                return;
            }

            if (!_pauseRequestedBySelection)
                return;

            GameManagerScript.Instance.ReleasePause();
            _pauseRequestedBySelection = false;
        }
    }
}
