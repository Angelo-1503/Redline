using System;
using Redline.Core;
using Redline.Factions;
using UnityEngine;

namespace Redline.Waves
{
    /// <summary>
    /// Spawner por POSIÇÃO, no estilo Contra: em vez de ondas que nascem sempre
    /// nos mesmos pontos, a fase é dividida em "encontros". Cada encontro tem
    /// uma linha de gatilho (triggerX); quando o jogador passa dessa linha, os
    /// inimigos daquele encontro nascem nas posições configuradas — normalmente
    /// mais à frente, fora da tela, ou atrás do jogador.
    ///
    /// Os inimigos são pedidos por FUNÇÃO (corpo-a-corpo / atirador) e o
    /// LevelFaction da cena devolve o prefab da facção inimiga, então o mesmo
    /// roteiro de encontros funciona nas fases Blue e Red.
    ///
    /// CONFIGURAÇÃO NO INSPECTOR (cada item de "encounters"):
    /// - name: só para organizar e aparecer no Console.
    /// - triggerX: posição X (do mundo) que o jogador precisa cruzar.
    /// - spawns: lista de inimigos. Para cada um:
    ///     role   = Melee (corpo-a-corpo) ou Shooter (atirador);
    ///     x      = posição X (do mundo) onde ele nasce;
    ///     height = altura acima da rua (0 = no chão; use a altura da
    ///              plataforma para colocar um atirador em cima dela).
    ///
    /// NO EDITOR: com este objeto selecionado, cada gatilho aparece como uma
    /// linha amarela e cada inimigo como um círculo (vermelho = corpo-a-corpo,
    /// azul = atirador), ligado ao gatilho dele.
    /// </summary>
    public class EncounterSpawner : MonoBehaviour
    {
        [Serializable]
        public class SpawnEntry
        {
            public EnemyRole role = EnemyRole.Melee;
            [Tooltip("Posição X (do mundo) onde o inimigo nasce.")]
            public float x;
            [Tooltip("Altura acima da rua. 0 = no chão.")]
            public float height;
        }

        [Serializable]
        public class Encounter
        {
            public string name = "Encontro";
            [Tooltip("Posição X (do mundo) que o jogador precisa cruzar para disparar este encontro.")]
            public float triggerX;
            public SpawnEntry[] spawns = new SpawnEntry[0];
        }

        [SerializeField] private Encounter[] encounters = new Encounter[0];
        [SerializeField] private Transform player;

        [Header("Posicionamento")]
        [Tooltip("Altura (Y do mundo) da superfície da rua.")]
        [SerializeField] private float roadY = -2.5f;
        [Tooltip("Distância do pé do inimigo até o pivô dele (metade da altura do colisor).")]
        [SerializeField] private float feetToPivot = 0.7f;

        private bool[] triggered;
        private int pendingEncounters;
        private int aliveEnemies;
        private bool allClearedRaised;

        /// <summary>Disparado quando um encontro começa: (índice, nome).</summary>
        public event Action<int, string> OnEncounterStarted;

        /// <summary>Disparado quando todos os encontros já aconteceram e não sobrou inimigo vivo.</summary>
        public event Action OnAllEncountersCleared;

        public int AliveEnemies => aliveEnemies;

        private void Awake()
        {
            triggered = new bool[encounters.Length];
            pendingEncounters = encounters.Length;
        }

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

            if (LevelFaction.Instance == null)
            {
                Debug.LogWarning("[EncounterSpawner] Não há LevelFaction na cena — nenhum inimigo será criado.");
            }
        }

        private void Update()
        {
            if (player == null)
            {
                return;
            }

            float playerX = player.position.x;
            for (int i = 0; i < encounters.Length; i++)
            {
                if (!triggered[i] && playerX >= encounters[i].triggerX)
                {
                    StartEncounter(i);
                }
            }
        }

        private void StartEncounter(int index)
        {
            triggered[index] = true;
            pendingEncounters--;

            Encounter encounter = encounters[index];
            Debug.Log($"[EncounterSpawner] {encounter.name} ({encounter.spawns.Length} inimigos).");
            OnEncounterStarted?.Invoke(index, encounter.name);

            foreach (SpawnEntry entry in encounter.spawns)
            {
                Spawn(entry);
            }

            CheckAllCleared();
        }

        private void Spawn(SpawnEntry entry)
        {
            LevelFaction level = LevelFaction.Instance;
            GameObject prefab = level != null ? level.GetEnemyPrefab(entry.role) : null;
            if (prefab == null)
            {
                return;
            }

            Vector3 position = new Vector3(entry.x, roadY + entry.height + feetToPivot, 0f);
            GameObject enemy = Instantiate(prefab, position, Quaternion.identity);

            Health health = enemy.GetComponent<Health>();
            if (health != null)
            {
                aliveEnemies++;
                health.OnDied += HandleEnemyDied;
            }
        }

        private void HandleEnemyDied()
        {
            aliveEnemies = Mathf.Max(0, aliveEnemies - 1);
            CheckAllCleared();
        }

        private void CheckAllCleared()
        {
            if (allClearedRaised || pendingEncounters > 0 || aliveEnemies > 0)
            {
                return;
            }

            allClearedRaised = true;
            Debug.Log("[EncounterSpawner] Todos os encontros concluídos.");
            OnAllEncountersCleared?.Invoke();
        }

        private void OnDrawGizmos()
        {
            if (encounters == null)
            {
                return;
            }

            foreach (Encounter encounter in encounters)
            {
                Vector3 bottom = new Vector3(encounter.triggerX, roadY, 0f);
                Vector3 top = bottom + Vector3.up * 6f;
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(bottom, top);

                if (encounter.spawns == null)
                {
                    continue;
                }

                foreach (SpawnEntry entry in encounter.spawns)
                {
                    Vector3 position = new Vector3(entry.x, roadY + entry.height + feetToPivot, 0f);
                    Gizmos.color = entry.role == EnemyRole.Melee
                        ? new Color(1f, 0.25f, 0.2f)
                        : new Color(0.2f, 0.7f, 1f);
                    Gizmos.DrawWireSphere(position, 0.45f);
                    Gizmos.color = new Color(1f, 1f, 0f, 0.25f);
                    Gizmos.DrawLine(top, position);
                }
            }
        }
    }
}
