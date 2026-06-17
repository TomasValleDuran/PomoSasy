using System;
using UnityEngine;

namespace Controllers
{
    public class WalletManagerScript : MonoBehaviour
    {
        public event Action<int> OnMoneyChanged;

        private int _value = 0;
        public int CurrentMoney => _value;
        public static WalletManagerScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Add(int amount)
        {
            _value += amount;
            OnMoneyChanged?.Invoke(_value);
        }


        public void Subtract(int amount)
        {
            _value -= amount;
            OnMoneyChanged?.Invoke(_value);
        }

        /// <summary>Set the wallet to an absolute value (used when restoring a saved game).</summary>
        public void RestoreMoney(int amount)
        {
            _value = Mathf.Max(0, amount);
            OnMoneyChanged?.Invoke(_value);
        }
    }
}