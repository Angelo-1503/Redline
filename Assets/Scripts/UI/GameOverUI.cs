using Redline.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Redline.UI
{
    /// <summary>
    /// Mostra o painel de Game Over quando o jogador morre, pausa o jogo
    /// (Time.timeScale = 0) e permite reiniciar a fase atual pelo botão Restart
    /// (ligue o método Restart() ao OnClick do botão no Inspector).
    /// </summary>
    public class GameOverUI : MonoBehaviour
    {
        [SerializeField] private GameObject gameOverPanel;

        private void Start()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(false);
            }

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            Health playerHealth = playerObj != null ? playerObj.GetComponent<Health>() : null;

            if (playerHealth != null)
            {
                playerHealth.OnDied += HandlePlayerDied;
            }
            else
            {
                Debug.LogWarning("[GameOverUI] Não encontrou o Health do jogador.");
            }
        }

        private void HandlePlayerDied()
        {
            if (gameOverPanel != null)
            {
                gameOverPanel.SetActive(true);
            }

            Time.timeScale = 0f;
        }

        /// <summary>Ligado ao botão "Restart" no Canvas (Button → OnClick).</summary>
        public void Restart()
        {
            Time.timeScale = 1f;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
}
