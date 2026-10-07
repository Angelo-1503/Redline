using System;
using System.Collections;
using Redline.CameraSystem;
using Redline.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Redline.Waves
{
    /// <summary>
    /// Cuida do chefão e do fim da fase.
    ///
    /// QUANDO O CHEFÃO APARECE — há dois modos:
    ///  - Arena (useArenaTrigger marcado): quando o jogador cruza a posição
    ///    arenaTriggerX, ou seja, quando chega na arena no fim da fase. É o
    ///    modo usado com o EncounterSpawner (fase com rolagem).
    ///  - Ondas (useArenaTrigger desmarcado): quando todas as ondas do
    ///    WaveSpawner acabam (evento OnAllWavesCleared). É o modo antigo.
    ///
    /// ARENA (lockCameraOnBoss marcado): quando o chefão aparece, a câmera
    /// para de seguir a fase e fica presa na arena (entre arenaStartX e
    /// arenaEndX). Uma parede invisível na borda esquerda da tela impede o
    /// jogador de voltar, e o chefão também não sai da área visível.
    ///
    /// FIM DA FASE: quando o chefão morre, espera nextSceneDelay segundos e
    /// carrega a cena nextSceneName (ela precisa estar em File > Build
    /// Profiles / Build Settings). Se nextSceneName estiver vazio, nada é
    /// carregado.
    ///
    /// O boss NÃO é instanciado em tempo de execução — ele já fica montado
    /// na cena (com Health, BossEnemy, etc. configurados) e desativado
    /// (checkbox desmarcada no Hierarchy), igual fizemos com o GameOverPanel.
    /// Isso permite arrastar o Health do boss direto pra Health Bar UI dele
    /// no Inspector, sem precisar de código extra pra ligar tudo em runtime.
    /// </summary>
    public class BossSpawner : MonoBehaviour
    {
        [SerializeField] private WaveSpawner waveSpawner;
        [SerializeField] private GameObject bossObject;

        [Header("Gatilho por posição (fase com rolagem)")]
        [Tooltip("Marcado: o chefão aparece quando o jogador cruza arenaTriggerX. Desmarcado: aparece quando as ondas do WaveSpawner acabam.")]
        [SerializeField] private bool useArenaTrigger;
        [Tooltip("Posição X (do mundo) que o jogador precisa cruzar para o chefão aparecer.")]
        [SerializeField] private float arenaTriggerX;
        [SerializeField] private Transform player;

        [Header("Arena")]
        [Tooltip("Marcado: prende a câmera na arena quando o chefão aparece.")]
        [SerializeField] private bool lockCameraOnBoss;
        [SerializeField] private CameraFollow cameraFollow;
        [Tooltip("Posição X (do mundo) onde a arena começa.")]
        [SerializeField] private float arenaStartX;
        [Tooltip("Posição X (do mundo) onde a arena (e a fase) termina.")]
        [SerializeField] private float arenaEndX;

        [Header("Fim da fase")]
        [Tooltip("Nome da cena carregada quando o chefão morre. Vazio = não carrega nada.")]
        [SerializeField] private string nextSceneName = "";
        [SerializeField] private float nextSceneDelay = 2.5f;

        private bool bossActivated;
        private bool arenaLocked;
        private float arenaLeftEdge;
        private Rigidbody2D bossBody;
        private Health bossHealth;

        /// <summary>Disparado quando o chefão aparece.</summary>
        public event Action OnBossActivated;

        /// <summary>Disparado quando o chefão morre.</summary>
        public event Action OnBossDefeated;

        private void Start()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
            }

            if (bossObject != null)
            {
                // GetComponent funciona mesmo com o objeto desativado.
                bossBody = bossObject.GetComponent<Rigidbody2D>();
                bossHealth = bossObject.GetComponent<Health>();
                if (bossHealth != null)
                {
                    bossHealth.OnDied += HandleBossDied;
                }

                bossObject.SetActive(false);
            }

            if (useArenaTrigger)
            {
                return;
            }

            if (waveSpawner != null)
            {
                waveSpawner.OnAllWavesCleared += ActivateBoss;
            }
            else
            {
                Debug.LogWarning("[BossSpawner] Wave Spawner não configurado.");
            }
        }

        private void Update()
        {
            if (!useArenaTrigger || bossActivated || player == null)
            {
                return;
            }

            if (player.position.x >= arenaTriggerX)
            {
                ActivateBoss();
            }
        }

        private void FixedUpdate()
        {
            // Mantém o chefão dentro da área visível da arena (ele recua para
            // manter distância do jogador e poderia sair pela esquerda).
            if (!arenaLocked || bossBody == null || bossHealth == null || bossHealth.IsDead)
            {
                return;
            }

            float minX = arenaLeftEdge + 1f;
            if (bossBody.position.x < minX)
            {
                bossBody.position = new Vector2(minX, bossBody.position.y);
                Vector2 velocity = bossBody.linearVelocity;
                bossBody.linearVelocity = new Vector2(Mathf.Max(0f, velocity.x), velocity.y);
            }
        }

        private void OnDestroy()
        {
            if (waveSpawner != null)
            {
                waveSpawner.OnAllWavesCleared -= ActivateBoss;
            }

            if (bossHealth != null)
            {
                bossHealth.OnDied -= HandleBossDied;
            }
        }

        private void ActivateBoss()
        {
            if (bossActivated)
            {
                return;
            }

            if (bossObject == null)
            {
                Debug.LogWarning("[BossSpawner] Boss Object não configurado.");
                return;
            }

            bossActivated = true;
            Debug.Log("[BossSpawner] Ativando o boss.");

            if (lockCameraOnBoss)
            {
                LockArena();
            }

            bossObject.SetActive(true);
            OnBossActivated?.Invoke();
        }

        private void LockArena()
        {
            if (cameraFollow == null && Camera.main != null)
            {
                cameraFollow = Camera.main.GetComponent<CameraFollow>();
            }

            if (cameraFollow == null)
            {
                Debug.LogWarning("[BossSpawner] Camera Follow não configurado — a câmera não será presa na arena.");
                return;
            }

            cameraFollow.LockToRange(arenaStartX, arenaEndX);
            arenaLeftEdge = Mathf.Min(arenaStartX, arenaEndX - cameraFollow.ViewHalfWidth * 2f);
            arenaLocked = true;

            // Parede invisível na borda esquerda da tela: só segura o jogador,
            // para os inimigos comuns que ficaram para trás ainda conseguirem entrar.
            GameObject wall = new GameObject("LimiteArena");
            wall.transform.position = new Vector3(arenaLeftEdge - 0.5f, 5f, 0f);
            BoxCollider2D box = wall.AddComponent<BoxCollider2D>();
            box.size = new Vector2(1f, 30f);
            if (player != null)
            {
                box.excludeLayers = ~(1 << player.gameObject.layer);
            }
        }

        private void HandleBossDied()
        {
            Debug.Log("[BossSpawner] Chefão derrotado.");
            OnBossDefeated?.Invoke();

            if (!string.IsNullOrEmpty(nextSceneName))
            {
                StartCoroutine(LoadNextScene());
            }
        }

        private IEnumerator LoadNextScene()
        {
            yield return new WaitForSeconds(nextSceneDelay);

            Health playerHealth = player != null ? player.GetComponent<Health>() : null;
            if (playerHealth != null && playerHealth.IsDead)
            {
                // O jogador morreu junto com o chefão: fica na tela de Game Over.
                yield break;
            }

            if (!Application.CanStreamedLevelBeLoaded(nextSceneName))
            {
                Debug.LogWarning($"[BossSpawner] A cena '{nextSceneName}' não está na lista de cenas do build (File > Build Profiles).");
                yield break;
            }

            SceneManager.LoadScene(nextSceneName);
        }

        private void OnDrawGizmos()
        {
            if (useArenaTrigger)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawLine(new Vector3(arenaTriggerX, -2.5f, 0f), new Vector3(arenaTriggerX, 4.5f, 0f));
            }

            if (lockCameraOnBoss)
            {
                Gizmos.color = new Color(1f, 0f, 1f, 0.5f);
                Gizmos.DrawLine(new Vector3(arenaStartX, 5f, 0f), new Vector3(arenaEndX, 5f, 0f));
            }
        }
    }
}
