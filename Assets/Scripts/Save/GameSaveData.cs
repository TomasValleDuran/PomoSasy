using System;
using System.Collections.Generic;
using UnityEngine;

namespace Save
{
    /// <summary>Per-attack upgrade multipliers, keyed by the AttackData asset name.</summary>
    [Serializable]
    public class AttackModifierEntry
    {
        public string attackId;
        public float damageMultiplier = 1f;
        public float cooldownMultiplier = 1f;
        public float rangeMultiplier = 1f;
    }

    /// <summary>How many times each upgrade has been picked (so maxed ones aren't re-offered).</summary>
    [Serializable]
    public class UpgradeLevelEntry
    {
        public string upgradeId;
        public int level;
    }

    /// <summary>
    /// Full snapshot of the player's in-progress run. Serialized to JSON by <see cref="SaveSystem"/>.
    /// Enemies are intentionally NOT persisted (they respawn on Continue).
    /// </summary>
    [Serializable]
    public class GameSaveData
    {
        public int version = 1;
        public string savedAtUtc;

        // XP / level
        public int level = 1;
        public int totalXp;
        public int currentLevelXp;
        public int xpForNextLevel;

        // Economy
        public int money;

        // Wave progress (0-based index of the wave the snapshot was taken at the START of)
        public int currentWave;

        // Health (absolute snapshot — restored directly, no recomputation)
        public float currentHealth;
        public float maxHealthMultiplier = 1f;

        // Passive upgrade multipliers
        public float moveSpeedMultiplier = 1f;
        public float xpGainMultiplier = 1f;

        // Per-attack upgrade multipliers
        public List<AttackModifierEntry> attackModifiers = new();

        // Upgrade pick counts
        public List<UpgradeLevelEntry> upgradeLevels = new();

        // Currently equipped attacks (AttackData asset names)
        public List<string> equippedAttacks = new();

        // Player world position
        public Vector3 playerPosition;
        public bool hasPosition;
    }
}
