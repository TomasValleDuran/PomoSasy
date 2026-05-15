using System;
using Health;
using UnityEngine;

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
            _value += amount;
            _currentLevelXp += amount;
            LevelUp();
            OnXpChanged?.Invoke(_currentLevelXp, _xpForNextLevel);  
            Debug.Log($"Added {amount} XP. Total XP: {_value}. Current Level: {_level}. Current XP: {_currentLevelXp}/{_xpForNextLevel}");
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
    }
}
