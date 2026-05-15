using System;
using Controllers;
using TMPro;
using UnityEngine;

namespace UI
{
    public class WalletUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text moneyText;

        private void Start()
        {
            WalletManagerScript.Instance.OnMoneyChanged += UpdateUI;
            UpdateUI(WalletManagerScript.Instance.CurrentMoney);
        }

        private void OnDestroy()
        {
            if (WalletManagerScript.Instance != null) WalletManagerScript.Instance.OnMoneyChanged -= UpdateUI;
        }

        private void UpdateUI(int amount)
        {
            moneyText.text = amount.ToString();
        }
    }
}