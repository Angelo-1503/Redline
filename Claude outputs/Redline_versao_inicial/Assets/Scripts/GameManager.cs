using System;
using Redline.Core;
using UnityEngine;

namespace Redline
{
    /// <summary>
    /// Ponto central mínimo do jogo para a fatia vertical: encontra o jogador,
    /// escuta a morte dele e registra "Game Over". Em iterações futuras, isso
    /// vira o lugar natural para controlar ondas de inimigos, chefão, UI de
    /// game over/restart, etc. (ver docs/05_proximos_passos.md).
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [SerializeField] private GameObject player;

        public event Action OnPlayerDied;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }

        private void Start()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            Health playerHealth = player != null ? player.GetComponent<Health>() : null;
            if (playerHealth != null)
            {
                playerHealth.OnDied += HandlePlayerDied;
            }
            else
            {
                Debug.LogWarning("[GameManager] Não encontrou um Health no objeto do jogador.");
            }
        }

        private void HandlePlayerDied()
        {
            Debug.Log("[GameManager] Jogador morreu — Game Over (placeholder).");
            OnPlayerDied?.Invoke();

            // TODO próxima iteração: mostrar UI de Game Over e permitir reiniciar
            // (por exemplo com SceneManager.LoadScene(SceneManager.GetActiveScene().name)).
        }
    }
}
