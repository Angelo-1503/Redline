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
        public GameObject[] enemyPrefabs;
        public int enemyCount = 3;
        public float spawnInterval = 1f;
    }
}
