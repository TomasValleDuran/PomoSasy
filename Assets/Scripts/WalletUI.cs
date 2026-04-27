using Controllers;
using UnityEngine;

using TMPro;
using UnityEngine;

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
        Debug.Log(amount);
        moneyText.text = amount.ToString();
    }
}