using UnityEngine;

namespace Redline.CameraSystem
{
    /// <summary>
    /// Segue o alvo (jogador) suavemente. Estilo side-scroller: normalmente
    /// trava o eixo Y para a câmera não "pular" junto com o personagem.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
        [SerializeField] private bool lockY = true;
        [SerializeField] private float fixedY = 1f;

        [Header("Limites da fase")]
        [Tooltip("Impede a câmera de mostrar além do começo/fim da fase.")]
        [SerializeField] private bool clampToLevel = true;
        [SerializeField] private float levelStartX = -10f;
        [SerializeField] private float levelEndX = 20f;

        [Header("Desnível (escada)")]
        [Tooltip("Quanto a câmera desce, em unidades, entre descentStartX e descentEndX. 0 = fase toda no mesmo nível.")]
        [SerializeField] private float descentAmount = 0f;
        [Tooltip("X do mundo onde a descida começa (topo da escada).")]
        [SerializeField] private float descentStartX = 0f;
        [Tooltip("X do mundo onde a descida termina (pé da escada).")]
        [SerializeField] private float descentEndX = 0f;

        private Vector3 currentVelocity;
        private Camera cam;

        private void Awake()
        {
            cam = GetComponent<Camera>();
        }

        /// <summary>Metade da largura do que a câmera mostra, em unidades do mundo.</summary>
        public float ViewHalfWidth => cam != null ? cam.orthographicSize * cam.aspect : 0f;

        /// <summary>
        /// Prende a câmera num trecho da fase (ex.: a arena do chefão). Se o
        /// trecho for mais estreito que a tela, a câmera encosta a borda direita
        /// da tela em endX, para não mostrar nada depois do fim da fase.
        /// </summary>
        public void LockToRange(float startX, float endX)
        {
            clampToLevel = true;
            levelEndX = endX;
            levelStartX = Mathf.Min(startX, endX - ViewHalfWidth * 2f);
        }

        /// <summary>
        /// Quanto a câmera deve estar abaixo de fixedY para um jogador na posição x.
        /// Antes da escada é 0, depois dela é descentAmount, e no meio desce aos poucos.
        /// </summary>
        private float DescentAt(float x)
        {
            if (Mathf.Approximately(descentAmount, 0f) || descentEndX <= descentStartX)
            {
                return 0f;
            }

            float t = Mathf.InverseLerp(descentStartX, descentEndX, x);
            return descentAmount * Mathf.SmoothStep(0f, 1f, t);
        }

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            if (lockY)
            {
                desiredPosition.y = fixedY - DescentAt(target.position.x);
            }

            if (clampToLevel && cam != null && cam.orthographic)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                float minX = levelStartX + halfWidth;
                float maxX = levelEndX - halfWidth;
                // Se a fase for mais estreita que a tela, centraliza.
                desiredPosition.x = minX <= maxX
                    ? Mathf.Clamp(desiredPosition.x, minX, maxX)
                    : (levelStartX + levelEndX) * 0.5f;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        }
    }
}
