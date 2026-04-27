using System;
using UnityEngine;

namespace Controllers
{
    public class WalletManagerScript : MonoBehaviour
    {
        public event Action<int> OnMoneyChanged;

        private int value = 0;
        public int CurrentMoney => value;
        public static WalletManagerScript Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void Add(int amount)
        {
            value += amount;
            Debug.Log(amount);
            OnMoneyChanged?.Invoke(value);
        }


        public void Subtract(int amount)
        {
            value -= amount;
            OnMoneyChanged?.Invoke(value);
        }
    }
}