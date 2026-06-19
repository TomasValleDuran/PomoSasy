using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Upgrades
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Button))]
    public class SkillOptionCardView : MonoBehaviour
    {
        [SerializeField] private TMP_Text optionNameText;
        [SerializeField] private TMP_Text optionLevelText;
        [SerializeField] private Image optionIconImage;
        [SerializeField] private TMP_Text optionDescriptionText;
        [SerializeField] private Button button;

        private UpgradeDefinition _boundUpgrade;
        private Action<UpgradeDefinition> _onSelected;

        private void Awake()
        {
            if (button == null)
                button = GetComponent<Button>();
        }

        private void OnEnable()
        {
            if (button != null)
                button.onClick.AddListener(HandleButtonClicked);
        }

        private void OnDisable()
        {
            if (button != null)
                button.onClick.RemoveListener(HandleButtonClicked);
        }

        public void Bind(UpgradeDefinition upgrade, int nextLevel, Action<UpgradeDefinition> onSelected)
        {
            _boundUpgrade = upgrade;
            _onSelected = onSelected;

            if (optionNameText != null)
                optionNameText.text = upgrade != null ? upgrade.DisplayName : string.Empty;

            if (optionLevelText != null)
                optionLevelText.text = upgrade != null ? $"Next Level: {nextLevel}" : string.Empty;

            if (optionDescriptionText != null)
                optionDescriptionText.text = upgrade != null ? upgrade.Description : string.Empty;

            if (optionIconImage != null)
            {
                optionIconImage.sprite = upgrade != null ? upgrade.Icon : null;
                optionIconImage.enabled = optionIconImage.sprite != null;
            }

            if (button != null)
                button.interactable = upgrade != null;
        }

        private void HandleButtonClicked()
        {
            if (_boundUpgrade == null)
                return;

            UIAudioPlayer.PlayButtonClick();
            _onSelected?.Invoke(_boundUpgrade);
        }
    }
}
