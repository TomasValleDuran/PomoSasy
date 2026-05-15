using Controllers;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
    public class XpUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text _levelTMP;
        [SerializeField] private Image _xpImage;

        private void Start()
        {
            XpManagerScript.Instance.OnXpChanged += UpdateXp;
            XpManagerScript.Instance.OnLevelChanged += UpdateLevel;

            UpdateXp(
                XpManagerScript.Instance.CurrentLevelXp,
                XpManagerScript.Instance.XpForNextLevel
            );
            UpdateLevel(XpManagerScript.Instance.CurrentLevel);
        }

        private void OnDestroy()
        {
            if (!XpManagerScript.Instance) return;
            XpManagerScript.Instance.OnXpChanged -= UpdateXp;
            XpManagerScript.Instance.OnLevelChanged -= UpdateLevel;
        }
        
        private void UpdateLevel(int level)
        {
            _levelTMP.text = level.ToString();
        }

        private void UpdateXp(int xp, int xpForNextLevel)
        {
            var percentage = (float)xp / xpForNextLevel;
            Debug.Log(percentage);
            _xpImage.fillAmount = percentage;
        }
    }
}