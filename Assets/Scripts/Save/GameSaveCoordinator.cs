using System;
using System.Collections;
using System.Collections.Generic;
using Attack;
using Controllers;
using Health;
using Spawner;
using Upgrades;
using UnityEngine;

namespace Save
{
    /// <summary>
    /// Lives in the Gameplay scene. Persists the run to disk at the START of every wave (so a
    /// player can't farm a partly-cleared wave's XP by quitting and continuing) and restores it a
    /// frame after load when the player chose "Continue". Only the latest wave-start snapshot is kept.
    /// </summary>
    public class GameSaveCoordinator : MonoBehaviour
    {
        public static GameSaveCoordinator Instance { get; private set; }

        private WaveSpawner _waveSpawner;

        private void Awake()
        {
            Instance = this;
        }

        private IEnumerator Start()
        {
            // Subscribe before the one-frame wait so the very first wave-start is captured even
            // if a wave has a startDelay of 0.
            _waveSpawner = FindFirstObjectByType<WaveSpawner>();
            if (_waveSpawner != null)
                _waveSpawner.OnWaveStarted += HandleWaveStarted;

            // Wait one frame so every Awake/Start ran (managers, loadout, UI subscriptions).
            yield return null;

            if (SaveSystem.ContinueRequested)
            {
                GameSaveData data = SaveSystem.Load();
                if (data != null)
                    RestoreState(data);
            }

            SaveSystem.ContinueRequested = false;
        }

        private void OnDestroy()
        {
            if (_waveSpawner != null)
                _waveSpawner.OnWaveStarted -= HandleWaveStarted;

            if (Instance == this)
                Instance = null;
        }

        // ---- Save triggers --------------------------------------------------

        private void HandleWaveStarted(int waveNumber, int totalWaves)
        {
            CaptureAndSave();
        }

        /// <summary>Capture and write the current run. No-op if the run is already over.</summary>
        public void CaptureAndSave()
        {
            if (GameManagerScript.Instance != null && GameManagerScript.Instance.IsGameOver)
                return;

            SaveSystem.Save(CaptureState());
        }

        // ---- Capture --------------------------------------------------------

        private GameSaveData CaptureState()
        {
            GameSaveData data = new GameSaveData
            {
                savedAtUtc = DateTime.UtcNow.ToString("O")
            };

            if (XpManagerScript.Instance != null)
                XpManagerScript.Instance.CaptureInto(data);

            if (WalletManagerScript.Instance != null)
                data.money = WalletManagerScript.Instance.CurrentMoney;

            // CurrentWaveNumber is 1-based and already points at the wave that just started.
            if (_waveSpawner != null)
                data.currentWave = Mathf.Max(0, _waveSpawner.CurrentWaveNumber - 1);

            Transform player = ResolvePlayer();
            if (player != null)
            {
                data.playerPosition = player.position;
                data.hasPosition = true;

                if (player.TryGetComponent(out HealthComponent health))
                {
                    data.currentHealth = health.CurrentHealth;
                    data.maxHealthMultiplier = health.MaxHealthMultiplier;
                }

                if (player.TryGetComponent(out PlayerUpgradeModifiers modifiers))
                {
                    data.moveSpeedMultiplier = modifiers.MoveSpeedMultiplier;
                    data.xpGainMultiplier = modifiers.XpGainMultiplier;
                    modifiers.CaptureAttackModifiers((attack, damage, cooldown, range) =>
                    {
                        data.attackModifiers.Add(new AttackModifierEntry
                        {
                            attackId = attack.name,
                            damageMultiplier = damage,
                            cooldownMultiplier = cooldown,
                            rangeMultiplier = range
                        });
                    });
                }

                if (player.TryGetComponent(out PlayerAttackLoadout loadout))
                {
                    for (int i = 0; i < loadout.Slots.Count; i++)
                    {
                        AttackData attackData = loadout.Slots[i].AttackData;
                        if (attackData != null)
                            data.equippedAttacks.Add(attackData.name);
                    }
                }
            }

            LevelUpSkillSelectionController selection = FindFirstObjectByType<LevelUpSkillSelectionController>();
            if (selection != null)
            {
                selection.CaptureLevels((id, level) =>
                    data.upgradeLevels.Add(new UpgradeLevelEntry { upgradeId = id, level = level }));
            }

            return data;
        }

        // ---- Restore --------------------------------------------------------

        private void RestoreState(GameSaveData data)
        {
            // XP first: sets fields only (no events), so it won't re-trigger level-up popups or health scaling.
            if (XpManagerScript.Instance != null)
                XpManagerScript.Instance.RestoreState(data.level, data.totalXp, data.currentLevelXp, data.xpForNextLevel);

            if (WalletManagerScript.Instance != null)
                WalletManagerScript.Instance.RestoreMoney(data.money);

            Transform player = ResolvePlayer();
            Dictionary<string, AttackData> attackRegistry = BuildAttackRegistry(player);

            if (player != null)
            {
                if (player.TryGetComponent(out PlayerUpgradeModifiers modifiers))
                {
                    modifiers.ResetAll();
                    modifiers.SetMoveSpeedMultiplier(data.moveSpeedMultiplier);
                    modifiers.SetXpGainMultiplier(data.xpGainMultiplier);

                    for (int i = 0; i < data.attackModifiers.Count; i++)
                    {
                        AttackModifierEntry entry = data.attackModifiers[i];
                        if (attackRegistry.TryGetValue(entry.attackId, out AttackData attack))
                            modifiers.SetAttackMultipliers(attack, entry.damageMultiplier, entry.cooldownMultiplier, entry.rangeMultiplier);
                    }
                }

                if (player.TryGetComponent(out PlayerAttackLoadout loadout))
                {
                    List<AttackData> attacks = new List<AttackData>();
                    for (int i = 0; i < data.equippedAttacks.Count; i++)
                    {
                        if (attackRegistry.TryGetValue(data.equippedAttacks[i], out AttackData attack))
                            attacks.Add(attack);
                    }

                    if (attacks.Count > 0)
                        loadout.RebuildFrom(attacks);
                }

                // Health LAST: absolute snapshot overrides any side effects of the steps above.
                if (player.TryGetComponent(out HealthComponent health))
                    health.RestoreState(data.currentHealth, data.maxHealthMultiplier);

                if (data.hasPosition)
                    player.position = data.playerPosition;
            }

            LevelUpSkillSelectionController selection = FindFirstObjectByType<LevelUpSkillSelectionController>();
            if (selection != null)
            {
                selection.ClearLevels();
                for (int i = 0; i < data.upgradeLevels.Count; i++)
                    selection.RestoreLevel(data.upgradeLevels[i].upgradeId, data.upgradeLevels[i].level);
            }

            // Refresh the XP UI explicitly (RestoreState fired no events on purpose).
            UI.XpUI xpUI = FindFirstObjectByType<UI.XpUI>();
            if (xpUI != null)
                xpUI.ForceRefresh();
        }

        // ---- Helpers --------------------------------------------------------

        private static Transform ResolvePlayer()
        {
            if (GameManagerScript.Instance != null && GameManagerScript.Instance.Player != null)
                return GameManagerScript.Instance.Player;

            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            return playerObject != null ? playerObject.transform : null;
        }

        /// <summary>
        /// Maps AttackData asset name -> asset, covering every attack a save could reference:
        /// the attacks currently on the loadout (starting attacks) plus every unlockable attack
        /// declared by the upgrade list.
        /// </summary>
        private Dictionary<string, AttackData> BuildAttackRegistry(Transform player)
        {
            Dictionary<string, AttackData> registry = new Dictionary<string, AttackData>();

            if (player != null && player.TryGetComponent(out PlayerAttackLoadout loadout))
            {
                for (int i = 0; i < loadout.Slots.Count; i++)
                {
                    AttackData attackData = loadout.Slots[i].AttackData;
                    if (attackData != null)
                        registry[attackData.name] = attackData;
                }
            }

            LevelUpSkillSelectionController selection = FindFirstObjectByType<LevelUpSkillSelectionController>();
            if (selection != null)
            {
                IReadOnlyList<UpgradeDefinition> upgrades = selection.AvailableUpgrades;
                for (int i = 0; i < upgrades.Count; i++)
                {
                    UpgradeDefinition upgrade = upgrades[i];
                    if (upgrade != null && upgrade.TargetAttack != null)
                        registry[upgrade.TargetAttack.name] = upgrade.TargetAttack;
                }
            }

            return registry;
        }
    }
}
