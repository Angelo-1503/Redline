using Redline.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Redline.UI
{
    /// <summary>
    /// Atualiza uma barra de vida (Image do tipo "Filled") conforme o Health
    /// alvo muda. Se "Target Health" for deixado vazio, procura automaticamente
    /// o Health do GameObject com a tag "Player".
    /// </summary>
    public class HealthBarUI : MonoBehaviour
    {
        [SerializeField] private Health targetHealth;
        [SerializeField] private Image fillImage;

        private void Start()
        {
            if (targetHealth == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    targetHealth = playerObj.GetComponent<Health>();
                }
            }

            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged += HandleHealthChanged;
                HandleHealthChanged(targetHealth.CurrentHealth, targetHealth.MaxHealth);
            }
            else
            {
                Debug.LogWarning("[HealthBarUI] Não encontrou nenhum Health para acompanhar.");
            }
        }

        private void OnDestroy()
        {
            if (targetHealth != null)
            {
                targetHealth.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (fillImage != null && max > 0)
            {
                fillImage.fillAmount = (float)current / max;
            }
        }
    }
}
