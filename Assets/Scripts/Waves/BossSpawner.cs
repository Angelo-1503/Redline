using UnityEngine;

namespace Redline.Waves
{
    /// <summary>
    /// Liga o WaveSpawner ao chefão: quando todas as ondas comuns acabam
    /// (evento OnAllWavesCleared), ativa o objeto do boss na cena.
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

        private void Start()
        {
            if (bossObject != null)
            {
                bossObject.SetActive(false);
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

        private void OnDestroy()
        {
            if (waveSpawner != null)
            {
                waveSpawner.OnAllWavesCleared -= ActivateBoss;
            }
        }

        private void ActivateBoss()
        {
            if (bossObject == null)
            {
                Debug.LogWarning("[BossSpawner] Boss Object não configurado.");
                return;
            }

            Debug.Log("[BossSpawner] Todas as ondas concluídas — ativando o boss.");
            bossObject.SetActive(true);
        }
    }
}
