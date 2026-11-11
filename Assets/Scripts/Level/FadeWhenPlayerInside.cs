using UnityEngine;

namespace Redline.Level
{
    /// <summary>
    /// Esmaece um sprite (ex.: a fachada de um prédio desenhada na frente do
    /// jogador) enquanto o jogador está dentro de um trecho, como um "raio X"
    /// para mostrar o túnel por dentro. Quando ele sai, o sprite volta.
    /// </summary>
    public class FadeWhenPlayerInside : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer target;
        [SerializeField] private Transform player;

        [Header("Trecho (coordenadas do mundo)")]
        [SerializeField] private float minX = 0f;
        [SerializeField] private float maxX = 10f;
        [SerializeField] private float minY = -100f;
        [SerializeField] private float maxY = 100f;

        [Header("Esmaecer")]
        [Tooltip("Transparência do sprite com o jogador dentro (0 = some de vez).")]
        [Range(0f, 1f)]
        [SerializeField] private float insideAlpha = 0f;
        [Tooltip("Quanto da transparência muda por segundo.")]
        [SerializeField] private float fadeSpeed = 4f;

        private void Awake()
        {
            if (target == null)
            {
                target = GetComponent<SpriteRenderer>();
            }

            if (player == null)
            {
                GameObject found = GameObject.FindGameObjectWithTag("Player");
                if (found != null)
                {
                    player = found.transform;
                }
            }
        }

        private void Update()
        {
            if (target == null || player == null)
            {
                return;
            }

            Vector3 p = player.position;
            bool inside = p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY;
            Color c = target.color;
            c.a = Mathf.MoveTowards(c.a, inside ? insideAlpha : 1f, fadeSpeed * Time.deltaTime);
            target.color = c;
        }
    }
}
