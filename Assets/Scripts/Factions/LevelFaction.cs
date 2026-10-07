using System;
using Redline.Animations;
using UnityEngine;

namespace Redline.Factions
{
    /// <summary>
    /// Define de que lado o jogador está nesta fase e, a partir disso, quais
    /// inimigos podem aparecer. Regra do jogo: as fases se intercalam —
    /// fase 1 o jogador é Blue e enfrenta Reds, fase 2 é Red e enfrenta Blues,
    /// fase 3 volta a ser Blue, e assim por diante.
    ///
    /// Fica em um objeto da cena (no GameManager). O WaveSpawner pergunta a
    /// ele qual prefab usar para cada função (corpo-a-corpo / atirador), então
    /// as ondas são configuradas uma vez só e funcionam para os dois lados.
    ///
    /// CONFIGURAÇÃO NO INSPECTOR:
    /// - levelNumber: número da fase (1, 2, 3...). Ímpar = Blue, par = Red.
    /// - blue / red: prefabs de inimigo de cada facção e a aparência do
    ///   jogador quando ele joga por aquela facção.
    /// - player: o SpriteRenderer do jogador. Se vazio, procura pela tag "Player".
    /// </summary>
    public class LevelFaction : MonoBehaviour
    {
        [Serializable]
        public class FactionSetup
        {
            [Header("Inimigos desta facção")]
            public GameObject meleePrefab;
            public GameObject shooterPrefab;

            [Header("Jogador jogando por esta facção")]
            [Tooltip("Sprite parado do jogador. Enquanto estiver vazio, o jogador usa o placeholder atual, pintado com a cor abaixo.")]
            public Sprite playerIdle;
            public Sprite[] playerRun;
            [Tooltip("Cor aplicada ao placeholder do jogador enquanto não houver sprite.")]
            public Color placeholderColor = Color.white;
        }

        public static LevelFaction Instance { get; private set; }

        [SerializeField] private int levelNumber = 1;
        [SerializeField] private FactionSetup blue = new FactionSetup();
        [SerializeField] private FactionSetup red = new FactionSetup();
        [SerializeField] private SpriteRenderer player;

        public int LevelNumber => levelNumber;
        public Faction PlayerFaction => levelNumber % 2 == 1 ? Faction.Blue : Faction.Red;
        public Faction EnemyFaction => PlayerFaction.Opposite();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Debug.LogWarning("[LevelFaction] Mais de um LevelFaction na cena — usando o primeiro.");
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        private void Start()
        {
            Debug.Log($"[LevelFaction] Fase {levelNumber}: jogador {PlayerFaction}, inimigos {EnemyFaction}.");
            ApplyPlayerLook();
        }

        /// <summary>
        /// Prefab da facção inimiga para a função pedida pela onda.
        /// </summary>
        public GameObject GetEnemyPrefab(EnemyRole role)
        {
            FactionSetup setup = GetSetup(EnemyFaction);
            GameObject prefab = role == EnemyRole.Melee ? setup.meleePrefab : setup.shooterPrefab;

            if (prefab == null)
            {
                Debug.LogWarning($"[LevelFaction] Sem prefab de {role} para a facção {EnemyFaction}.");
            }

            return prefab;
        }

        private FactionSetup GetSetup(Faction faction)
        {
            return faction == Faction.Blue ? blue : red;
        }

        private void ApplyPlayerLook()
        {
            if (player == null)
            {
                GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
                if (playerObj != null)
                {
                    player = playerObj.GetComponentInChildren<SpriteRenderer>();
                }
            }

            if (player == null)
            {
                return;
            }

            FactionSetup setup = GetSetup(PlayerFaction);

            if (setup.playerIdle == null)
            {
                // Ainda sem arte do jogador: mantém o placeholder e só pinta com a cor da facção.
                player.color = setup.placeholderColor;
                return;
            }

            player.color = Color.white;
            player.sprite = setup.playerIdle;

            if (setup.playerRun != null && setup.playerRun.Length > 0)
            {
                SpriteAnimator animator = player.GetComponent<SpriteAnimator>();
                if (animator == null)
                {
                    animator = player.gameObject.AddComponent<SpriteAnimator>();
                }

                animator.SetSprites(setup.playerIdle, setup.playerRun);
            }
        }
    }
}
