using UnityEngine;

namespace Redline.CameraSystem
{
    /// <summary>
    /// Fundo distante (céu / cidade ao longe) com efeito parallax: acompanha a
    /// câmera, mas desliza um pouco no sentido contrário conforme o jogador
    /// avança, dando sensação de profundidade.
    ///
    /// O deslocamento é calculado a partir do comprimento da fase, então a
    /// borda esquerda da imagem aparece no começo da fase e a borda direita
    /// no fim — nunca sobra um "buraco" nas laterais, qualquer que seja o
    /// tamanho da fase.
    ///
    /// CONFIGURAÇÃO NO INSPECTOR:
    /// - levelStartX / levelEndX: onde a fase começa e termina (em unidades).
    ///   Ao aumentar a fase, só atualize esses dois valores.
    /// - verticalOffset: sobe/desce o fundo em relação ao centro da câmera.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class ParallaxBackground : MonoBehaviour
    {
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float levelStartX = -10f;
        [SerializeField] private float levelEndX = 20f;
        [SerializeField] private float verticalOffset = 0f;

        private SpriteRenderer spriteRenderer;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
            }
        }

        private void LateUpdate()
        {
            if (targetCamera == null || spriteRenderer.sprite == null)
            {
                return;
            }

            Vector3 cameraPosition = targetCamera.transform.position;
            float halfViewWidth = targetCamera.orthographicSize * targetCamera.aspect;
            float halfLayerWidth = spriteRenderer.bounds.extents.x;
            float maxShift = Mathf.Max(0f, halfLayerWidth - halfViewWidth);

            float t = Mathf.InverseLerp(levelStartX + halfViewWidth, levelEndX - halfViewWidth, cameraPosition.x);
            float x = cameraPosition.x + Mathf.Lerp(maxShift, -maxShift, t);

            transform.position = new Vector3(x, cameraPosition.y + verticalOffset, transform.position.z);
        }
    }
}
