using Redline.Factions;
using UnityEngine;

namespace Redline.Waves
{
    /// <summary>
    /// Configuração de uma onda de inimigos: quais prefabs podem aparecer,
    /// quantos inimigos no total, e o intervalo entre cada spawn individual.
    /// </summary>
    [System.Serializable]
    public class Wave
    {
        public string waveName = "Onda";
        [Tooltip("Funções que podem aparecer nesta onda. O prefab da facção inimiga é escolhido pelo LevelFaction da cena.")]
        public EnemyRole[] enemyRoles;
        [Tooltip("Alternativa: prefabs fixos. Só é usado se a cena não tiver LevelFaction ou se enemyRoles estiver vazio.")]
        public GameObject[] enemyPrefabs;
        public int enemyCount = 3;
        public float spawnInterval = 1f;
    }
}
