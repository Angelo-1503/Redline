using System;
using System.Collections;
using Redline.Core;
using UnityEngine;

namespace Redline.Waves
{
    /// <summary>
    /// Controla a progressão de ondas de inimigos: spawna os inimigos de uma onda
    /// em pontos aleatórios, espera todos morrerem, e libera a próxima onda depois
    /// de um intervalo. Quando não há mais ondas configuradas, dispara OnAllWavesCleared
    /// (gancho natural para, na próxima iteração, liberar o chefão).
    /// </summary>
    public class WaveSpawner : MonoBehaviour
    {
        [SerializeField] private Wave[] waves;
        [SerializeField] private Transform[] spawnPoints;
        [SerializeField] private float timeBetweenWaves = 3f;
        [SerializeField] private float firstWaveDelay = 1f;

        private int currentWaveIndex = -1;
        private int aliveEnemies;
        private bool spawningInProgress;

        public event Action<int, string> OnWaveStarted;
        public event Action OnAllWavesCleared;

        private void Start()
        {
            StartCoroutine(StartFirstWaveAfterDelay());
        }

        private IEnumerator StartFirstWaveAfterDelay()
        {
            yield return new WaitForSeconds(firstWaveDelay);
            StartNextWave();
        }

        private void StartNextWave()
        {
            currentWaveIndex++;

            if (waves == null || currentWaveIndex >= waves.Length)
            {
                Debug.Log("[WaveSpawner] Todas as ondas concluídas.");
                OnAllWavesCleared?.Invoke();
                return;
            }

            Wave wave = waves[currentWaveIndex];
            OnWaveStarted?.Invoke(currentWaveIndex, wave.waveName);
            StartCoroutine(SpawnWave(wave));
        }

        private IEnumerator SpawnWave(Wave wave)
        {
            spawningInProgress = true;
            Debug.Log($"[WaveSpawner] Iniciando {wave.waveName} ({wave.enemyCount} inimigos).");

            for (int i = 0; i < wave.enemyCount; i++)
            {
                SpawnEnemy(wave);
                yield return new WaitForSeconds(wave.spawnInterval);
            }

            spawningInProgress = false;
            CheckWaveCleared();
        }

        private void SpawnEnemy(Wave wave)
        {
            if (wave.enemyPrefabs == null || wave.enemyPrefabs.Length == 0)
            {
                Debug.LogWarning("[WaveSpawner] Onda sem prefabs de inimigo configurados.");
                return;
            }

            if (spawnPoints == null || spawnPoints.Length == 0)
            {
                Debug.LogWarning("[WaveSpawner] Nenhum spawn point configurado.");
                return;
            }

            GameObject prefab = wave.enemyPrefabs[UnityEngine.Random.Range(0, wave.enemyPrefabs.Length)];
            Transform point = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)];

            GameObject enemyInstance = Instantiate(prefab, point.position, Quaternion.identity);
            aliveEnemies++;

            Health health = enemyInstance.GetComponent<Health>();
            if (health != null)
            {
                health.OnDied += HandleEnemyDied;
            }
            else
            {
                Debug.LogWarning($"[WaveSpawner] Prefab '{prefab.name}' não tem componente Health — não será contado na onda.");
            }
        }

        private void HandleEnemyDied()
        {
            aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
            CheckWaveCleared();
        }

        private void CheckWaveCleared()
        {
            if (aliveEnemies <= 0 && !spawningInProgress)
            {
                StartCoroutine(NextWaveAfterDelay());
            }
        }

        private IEnumerator NextWaveAfterDelay()
        {
            // Evita chamar a próxima onda mais de uma vez caso dois inimigos morram no mesmo frame.
            spawningInProgress = true;
            yield return new WaitForSeconds(timeBetweenWaves);
            StartNextWave();
        }
    }
}
