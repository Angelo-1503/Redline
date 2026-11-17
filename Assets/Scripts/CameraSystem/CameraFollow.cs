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
        [Tooltip("Se o trecho for mais estreito que a tela (ex.: monitor mais largo que 16:9), aproxima a câmera para o trecho ocupar a tela inteira, em vez de mostrar o fundo do lado.")]
        [SerializeField] private bool fitWidthIfNarrow = false;
        [Tooltip("Limita a borda inferior visível à base da arte do cenário.")]
        [SerializeField] private bool clampBottom;
        [SerializeField] private float minimumViewY;

        [Header("Desnível (escada)")]
        [Tooltip("Quanto a câmera desce, em unidades, entre descentStartX e descentEndX. 0 = fase toda no mesmo nível.")]
        [SerializeField] private float descentAmount = 0f;
        [Tooltip("X do mundo onde a descida começa (topo da escada).")]
        [SerializeField] private float descentStartX = 0f;
        [Tooltip("X do mundo onde a descida termina (pé da escada).")]
        [SerializeField] private float descentEndX = 0f;

        [Header("Segundo desnível (sacada da Fase 3)")]
        [Tooltip("Quanto a câmera desce A MAIS quando o jogador anda de leftDescentStartX até leftDescentEndX (pode ser para a esquerda ou para a direita). 0 = desligado.")]
        [SerializeField] private float leftDescentAmount = 0f;
        [Tooltip("X do mundo onde essa descida começa.")]
        [SerializeField] private float leftDescentStartX = 0f;
        [Tooltip("X do mundo onde ela termina (câmera já toda abaixada).")]
        [SerializeField] private float leftDescentEndX = 0f;
        [Tooltip("À esquerda deste X a descida deixa de valer (para não afetar outros trechos da fase).")]
        [SerializeField] private float leftDescentMinX = 0f;
        [Tooltip("À direita deste X a descida deixa de valer.")]
        [SerializeField] private float leftDescentMaxX = 100000f;

        private Vector3 currentVelocity;
        private Camera cam;
        private float baseOrthoSize;

        private void Awake()
        {
            cam = GetComponent<Camera>();
            if (cam != null)
            {
                baseOrthoSize = cam.orthographicSize;
            }
        }

        /// <summary>Metade da largura do que a câmera mostra, em unidades do mundo.</summary>
        public float ViewHalfWidth => cam != null ? cam.orthographicSize * cam.aspect : 0f;

        /// <summary>
        /// Prende a câmera num trecho da fase (ex.: a arena do chefão). Se o
        /// trecho for mais estreito que a tela, a câmera encosta a borda direita
        /// da tela em endX, para não mostrar nada depois do fim da fase.
        /// Com alignLeftIfNarrow é o contrário: a borda esquerda da tela encosta em
        /// startX e a sobra aparece só à direita (usado pelas portas, para cenários
        /// fechados como o túnel da Fase 4, que continuam para a direita).
        /// </summary>
        public void LockToRange(float startX, float endX, bool alignLeftIfNarrow = false)
        {
            clampToLevel = true;
            if (fitWidthIfNarrow)
            {
                // A câmera se aproxima para caber no trecho (já agora, para a porta posicionar certo).
                levelStartX = startX;
                levelEndX = endX;
                if (cam != null && cam.orthographic && cam.aspect > 0f)
                {
                    // Só aproxima, nunca afasta: o zoom fica o mesmo na fase inteira (senão um trecho
                    // parece maior que o outro).
                    cam.orthographicSize = Mathf.Min(cam.orthographicSize, (endX - startX) / (2f * cam.aspect));
                }
            }
            else if (alignLeftIfNarrow)
            {
                levelStartX = startX;
                levelEndX = Mathf.Max(endX, startX + ViewHalfWidth * 2f);
            }
            else
            {
                levelEndX = endX;
                levelStartX = Mathf.Min(startX, endX - ViewHalfWidth * 2f);
            }
        }

        /// <summary>
        /// Quanto a câmera deve estar abaixo de fixedY para um jogador na posição x.
        /// Antes da escada é 0, depois dela é descentAmount, e no meio desce aos poucos.
        /// </summary>
        private float DescentAt(float x)
        {
            float total = 0f;
            if (!Mathf.Approximately(descentAmount, 0f) && descentEndX > descentStartX)
            {
                float t = Mathf.InverseLerp(descentStartX, descentEndX, x);
                total += descentAmount * Mathf.SmoothStep(0f, 1f, t);
            }

            // Segunda descida: vale só entre leftDescentMinX e leftDescentMaxX e cresce de leftDescentStartX para leftDescentEndX.
            if (!Mathf.Approximately(leftDescentAmount, 0f) && !Mathf.Approximately(leftDescentStartX, leftDescentEndX)
                && x >= leftDescentMinX && x <= leftDescentMaxX)
            {
                float t = Mathf.InverseLerp(leftDescentStartX, leftDescentEndX, x);
                total += leftDescentAmount * Mathf.SmoothStep(0f, 1f, t);
            }

            return total;
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

            if (fitWidthIfNarrow && clampToLevel && cam != null && cam.orthographic && cam.aspect > 0f)
            {
                float sizeToFit = (levelEndX - levelStartX) / (2f * cam.aspect);
                cam.orthographicSize = Mathf.Min(cam.orthographicSize, sizeToFit);
            }

            if (clampToLevel && cam != null && cam.orthographic)
            {
                float halfWidth = cam.orthographicSize * cam.aspect;
                float minX = levelStartX + halfWidth;
                float maxX = levelEndX - halfWidth;
                // Se a fase for mais estreita que a tela, encosta na esquerda
                // (a sobra aparece só à direita, onde a fase vai continuar).
                desiredPosition.x = minX <= maxX
                    ? Mathf.Clamp(desiredPosition.x, minX, maxX)
                    : minX;
            }

            float minimumCameraY = minimumViewY + (cam != null ? cam.orthographicSize : 0f);
            if (clampBottom && cam != null && cam.orthographic)
                desiredPosition.y = Mathf.Max(desiredPosition.y, minimumCameraY);
            Vector3 position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
            if (clampBottom && cam != null && cam.orthographic && position.y < minimumCameraY)
            {
                position.y = minimumCameraY;
                currentVelocity.y = 0f;
            }
            transform.position = position;
        }
    }
}
