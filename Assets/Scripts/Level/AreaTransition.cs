using System;
using System.Collections;
using Redline.CameraSystem;
using Redline.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Redline.Level
{
    /// <summary>
    /// Troca de cenário quando o jogador POUSA dentro de uma área (ex.: o teto
    /// da van na Fase 3): a tela escurece e o jogador reaparece no próximo
    /// trecho da fase, com a câmera já presa nele.
    ///
    /// Fica num objeto vazio da cena. A posição do objeto não importa: a área
    /// é configurada pelos campos abaixo, em coordenadas do mundo, e aparece
    /// como um retângulo laranja na Scene.
    ///
    /// ENQUANTO O PRÓXIMO TRECHO NÃO EXISTE: deixe "Teleport" desmarcado. A
    /// tela escurece e clareia com o jogador no mesmo lugar.
    /// </summary>
    public class AreaTransition : MonoBehaviour
    {
        [Header("Área (coordenadas do mundo, medidas no centro do jogador)")]
        [SerializeField] private float minX = 0f;
        [SerializeField] private float maxX = 0f;
        [SerializeField] private float minY = -100f;
        [SerializeField] private float maxY = 100f;
        [Tooltip("Tempo que o jogador precisa ficar parado na vertical (pousado) dentro da área.")]
        [SerializeField] private float landedTime = 0.2f;
        [Tooltip("Velocidade vertical máxima para considerar que ele pousou.")]
        [SerializeField] private float maxVerticalSpeed = 0.1f;

        [Header("Transição")]
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private float blackHold = 0.4f;

        [Header("Destino (próximo trecho)")]
        [Tooltip("Desmarcado: a tela escurece e clareia sem mudar o jogador de lugar (para testar sem o próximo trecho).")]
        [SerializeField] private bool teleport = false;
        [SerializeField] private Vector2 destination;
        [SerializeField] private float destinationStartX;
        [SerializeField] private float destinationEndX;

        /// <summary>Disparado quando a troca termina e o controle volta para o jogador.</summary>
        public event Action OnFinished;

        private Transform player;
        private Rigidbody2D body;
        private float landedTimer;
        private bool started;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                body = playerObj.GetComponent<Rigidbody2D>();
            }
            else
            {
                Debug.LogWarning("[AreaTransition] Não encontrou o objeto com a tag Player.");
            }
        }

        private void Update()
        {
            if (started || player == null)
            {
                return;
            }

            Vector3 p = player.position;
            bool inside = p.x >= minX && p.x <= maxX && p.y >= minY && p.y <= maxY;
            bool landed = body == null || Mathf.Abs(body.linearVelocity.y) <= maxVerticalSpeed;
            if (inside && landed)
            {
                landedTimer += Time.deltaTime;
                if (landedTimer >= landedTime)
                {
                    started = true;
                    StartCoroutine(Run());
                }
            }
            else
            {
                landedTimer = 0f;
            }
        }

        private IEnumerator Run()
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerShooting shooting = player.GetComponent<PlayerShooting>();

            // 1) Tira o controle do jogador e para ele.
            if (controller != null) controller.enabled = false;
            if (shooting != null) shooting.enabled = false;
            if (body != null) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);

            // 2) A tela escurece.
            Image black = CreateBlackOverlay();
            yield return Fade(black, 0f, 1f);

            // 3) Leva o jogador para o próximo trecho.
            if (teleport)
            {
                player.position = new Vector3(destination.x, destination.y, player.position.z);
                if (body != null) body.linearVelocity = Vector2.zero;
                Camera cam = Camera.main;
                CameraFollow follow = cam != null ? cam.GetComponent<CameraFollow>() : null;
                if (follow != null)
                {
                    follow.LockToRange(destinationStartX, destinationEndX);
                }
                if (cam != null)
                {
                    // Já coloca a câmera no lugar final, para ela não deslizar enquanto a tela clareia.
                    float half = follow != null ? follow.ViewHalfWidth : 0f;
                    float camMinX = destinationStartX + half;
                    float camMaxX = destinationEndX - half;
                    Vector3 camPos = cam.transform.position;
                    camPos.x = camMinX <= camMaxX
                        ? Mathf.Clamp(destination.x, camMinX, camMaxX)
                        : (destinationStartX + destinationEndX) * 0.5f;
                    cam.transform.position = camPos;
                }
            }
            yield return new WaitForSeconds(blackHold);

            // 4) Clareia e devolve o controle.
            yield return Fade(black, 1f, 0f);
            Destroy(black.transform.parent.gameObject);
            if (controller != null) controller.enabled = true;
            if (shooting != null) shooting.enabled = true;
            OnFinished?.Invoke();
        }

        private static Image CreateBlackOverlay()
        {
            GameObject canvasObj = new GameObject("TelaPretaDaArea");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            GameObject imageObj = new GameObject("Preto");
            imageObj.transform.SetParent(canvasObj.transform, false);
            Image image = imageObj.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0f);
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }

        private IEnumerator Fade(Image image, float fromAlpha, float toAlpha)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, fadeDuration))
            {
                image.color = new Color(0f, 0f, 0f, Mathf.Lerp(fromAlpha, toAlpha, t));
                yield return null;
            }
            image.color = new Color(0f, 0f, 0f, toAlpha);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(1f, 0.6f, 0.1f, 0.9f);
            float y0 = Mathf.Max(minY, -50f);
            float y1 = Mathf.Min(maxY, 50f);
            Vector3 center = new Vector3((minX + maxX) * 0.5f, (y0 + y1) * 0.5f, 0f);
            Gizmos.DrawWireCube(center, new Vector3(maxX - minX, y1 - y0, 0f));
            if (teleport)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(new Vector3(destination.x, destination.y, 0f), 0.3f);
            }
        }
    }
}
