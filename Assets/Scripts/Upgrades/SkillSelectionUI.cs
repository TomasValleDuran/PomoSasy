using System;
using System.Collections.Generic;
using UnityEngine;

namespace Upgrades
{
    [DisallowMultipleComponent]
    public class SkillSelectionUI : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private SkillOptionCardView[] optionCards = Array.Empty<SkillOptionCardView>();

        public bool IsVisible => canvasGroup != null && canvasGroup.blocksRaycasts;

        private void Awake()
        {
            if (canvasGroup == null)
                canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Hide();
        }

        public void Show(IReadOnlyList<UpgradeOffer> offers, Action<UpgradeDefinition> onSelected)
        {
            if (offers == null || offers.Count == 0)
            {
                Hide();
                return;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;

            for (int i = 0; i < optionCards.Length; i++)
            {
                SkillOptionCardView card = optionCards[i];
                if (card == null)
                    continue;

                if (i >= offers.Count)
                {
                    card.gameObject.SetActive(false);
                    continue;
                }

                UpgradeOffer offer = offers[i];
                card.gameObject.SetActive(true);
                card.Bind(offer.Upgrade, offer.NextLevel, onSelected);
            }
        }

        public void Hide()
        {
            if (canvasGroup == null)
                return;

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }
    }
}
