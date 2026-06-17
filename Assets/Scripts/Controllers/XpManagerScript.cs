using System;
using Health;
using UnityEngine;
using Upgrades;

namespace Controllers
{
    public class XpManagerScript : MonoBehaviour
    {
        public event Action<int, int> OnXpChanged;
        public event Action<int> OnLevelChanged;

        private int _value = 0;
        private int _xpForNextLevel = PomoSasyConstants.Config.Leveling.BaseXpForLevelUp;
        private int _currentLevelXp = 0;
        private int _level = 1;
        public int CurrentXp => _value;
        public int CurrentLevel => _level;
        public int XpForNextLevel => _xpForNextLevel;
        public int CurrentLevelXp => _currentLevelXp;
        public static XpManagerScript Instance { get; private set; }

        private HealthComponent _playerHealth;

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            OnLevelChanged += RestorePlayerHealth;
        }

        private void OnDestroy()
        {
            OnLevelChanged -= RestorePlayerHealth;
        }

        public void Add(int amount)
        {
            int effectiveAmount = GetEffectiveXpAmount(amount);
            _value += effectiveAmount;
            _currentLevelXp += effectiveAmount;
            LevelUp();
            OnXpChanged?.Invoke(_currentLevelXp, _xpForNextLevel);  
            Debug.Log($"Added {effectiveAmount} XP. Total XP: {_value}. Current Level: {_level}. Current XP: {_currentLevelXp}/{_xpForNextLevel}");
        }
        
        public bool CheckLevelUp()
        {
            return _level < PomoSasyConstants.Config.Leveling.MaxLevel && _currentLevelXp >= _xpForNextLevel;
        }

        public void LevelUp()
        {
            while (CheckLevelUp())
            {
                _currentLevelXp -= _xpForNextLevel;
                _level++;
                OnLevelChanged?.Invoke(_level);

                _xpForNextLevel = Mathf.CeilToInt(_xpForNextLevel * PomoSasyConstants.Config.Leveling.XpPerLevelMultiplier);
            }
        }

        /// <summary>Copy current XP/level state into a save snapshot.</summary>
        public void CaptureInto(Save.GameSaveData data)
        {
            if (data == null)
                return;

            data.level = _level;
            data.totalXp = _value;
            data.currentLevelXp = _currentLevelXp;
            data.xpForNextLevel = _xpForNextLevel;
        }

        /// <summary>
        /// Restore XP/level by setting fields directly. Fires NO events on purpose, so it does not
        /// re-trigger the level-up popup or the per-level health scaling. The UI is refreshed
        /// separately (see GameSaveCoordinator / XpUI.ForceRefresh).
        /// </summary>
        public void RestoreState(int level, int totalXp, int currentLevelXp, int xpForNextLevel)
        {
            _level = Mathf.Max(1, level);
            _value = Mathf.Max(0, totalXp);
            _currentLevelXp = Mathf.Max(0, currentLevelXp);
            _xpForNextLevel = Mathf.Max(1, xpForNextLevel);
        }

        private void RestorePlayerHealth(int level)
        {
            if (_playerHealth == null)
            {
                var player = GameObject.FindGameObjectWithTag("Player");
                if (player == null || !player.TryGetComponent(out _playerHealth))
                {
                    Debug.LogWarning("XpManagerScript could not find the player's HealthComponent.");
                    return;
                }
            }

            _playerHealth.MultiplyMaxHealth(PomoSasyConstants.Config.Leveling.PlayerHealthMultiplierPerLevel);
            _playerHealth.ResetHealth();
        }

        private int GetEffectiveXpAmount(int amount)
        {
            if (amount <= 0)
                return 0;

            if (GameManagerScript.Instance == null || GameManagerScript.Instance.Player == null)
                return amount;

            if (!GameManagerScript.Instance.Player.TryGetComponent(out PlayerUpgradeModifiers modifiers))
                return amount;

            return Mathf.Max(1, Mathf.RoundToInt(amount * modifiers.XpGainMultiplier));
        }
    }
}
