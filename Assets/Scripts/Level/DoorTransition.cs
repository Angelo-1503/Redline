using System;
using System.Collections;
using Redline.CameraSystem;
using Redline.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Redline.Level
{
    /// <summary>
    /// Passagem por uma porta do cenário (ex.: a boca do rato na Fase 3).
    ///
    /// Quando o jogador chega na frente da porta, ele perde o controle por um
    /// instante: o personagem anda até a porta, entra nela (encolhendo e
    /// sumindo, como se fosse para o fundo do cenário), a tela escurece e o
    /// jogador reaparece no próximo trecho da fase, com a câmera já presa nele.
    ///
    /// Fica num objeto vazio da cena. A posição do objeto não importa: tudo é
    /// configurado pelos campos abaixo, em coordenadas do mundo.
    ///
    /// ENQUANTO O PRÓXIMO TRECHO NÃO EXISTE: deixe "Teleport" desmarcado. A
    /// cena roda inteira e, depois de escurecer, devolve o jogador na frente
    /// da porta. Quando o trecho estiver pronto, marque a opção e preencha
    /// "Destination", "Destination Start X" e "Destination End X".
    /// </summary>
    public class DoorTransition : MonoBehaviour
    {
        [Header("Gatilho")]
        [Tooltip("X do mundo a partir do qual a passagem começa.")]
        [SerializeField] private float triggerX = 0f;
        [Tooltip("Marcado: a porta fica à esquerda e a passagem começa quando o jogador chega com X MENOR ou igual a Trigger X.")]
        [SerializeField] private bool triggerFromRight = false;
        [Tooltip("A passagem só começa com o jogador a no máximo esta distância (em X) da porta. Evita disparar de longe, em outro trecho da fase.")]
        [SerializeField] private float maxDistance = 4f;
        [Tooltip("O jogador precisa estar com Y entre estes dois valores (serve para só disparar no piso da porta).")]
        [SerializeField] private float minPlayerY = -100f;
        [SerializeField] private float maxPlayerY = 100f;

        [Header("Porta")]
        [Tooltip("Ponto do mundo onde o personagem some: o meio da porta, na altura do chão dela.")]
        [SerializeField] private Vector2 doorPoint;
        [SerializeField] private float walkSpeed = 3.5f;
        [SerializeField] private float enterDuration = 0.6f;
        [Tooltip("Tamanho final do personagem ao sumir (1 = tamanho normal).")]
        [SerializeField] private float endScale = 0.6f;
        [Tooltip("Quanto o personagem sobe na tela ao entrar (dá a sensação de ir para o fundo).")]
        [SerializeField] private float enterRise = 0.25f;

        [Header("Transição")]
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private float blackHold = 0.4f;

        [Header("Destino (próximo trecho)")]
        [Tooltip("Desmarcado: depois de escurecer, o jogador volta para a frente da porta (para testar sem o próximo trecho).")]
        [SerializeField] private bool teleport = false;
        [SerializeField] private Vector2 destination;
        [SerializeField] private float destinationStartX;
        [SerializeField] private float destinationEndX;

        /// <summary>Disparado quando a passagem termina e o controle volta para o jogador.</summary>
        public event Action OnFinished;

        private Transform player;
        private bool started;

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[DoorTransition] Não encontrou o objeto com a tag Player.");
            }
        }

        private void Update()
        {
            if (started || player == null)
            {
                return;
            }

            Vector3 p = player.position;
            bool reached = triggerFromRight ? p.x <= triggerX : p.x >= triggerX;
            bool near = Mathf.Abs(p.x - doorPoint.x) <= maxDistance;
            if (reached && near && p.y >= minPlayerY && p.y <= maxPlayerY)
            {
                started = true;
                StartCoroutine(Run());
            }
        }

        private IEnumerator Run()
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerShooting shooting = player.GetComponent<PlayerShooting>();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            SpriteRenderer sprite = player.GetComponentInChildren<SpriteRenderer>();

            // 1) Tira o controle do jogador.
            if (controller != null) controller.enabled = false;
            if (shooting != null) shooting.enabled = false;

            // 2) Anda sozinho até a frente da porta (com a física ligada, para a animação de corrida tocar).
            float direction = Mathf.Sign(doorPoint.x - player.position.x);
            if (sprite != null) sprite.flipX = direction < 0f;
            float timeout = 2.5f;
            while (timeout > 0f && Mathf.Abs(doorPoint.x - player.position.x) > 0.08f
                   && Mathf.Sign(doorPoint.x - player.position.x) == direction)
            {
                if (body != null)
                {
                    body.linearVelocity = new Vector2(direction * walkSpeed, body.linearVelocity.y);
                }
                else
                {
                    player.position += new Vector3(direction * walkSpeed * Time.deltaTime, 0f, 0f);
                }
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
            }
            yield return new WaitForSeconds(0.15f);

            // 3) Entra na porta: encolhe, sobe um pouco e some.
            Vector3 from = player.position;
            Vector3 originalScale = player.localScale;
            Color originalColor = sprite != null ? sprite.color : Color.white;
            if (body != null) body.simulated = false;
            Vector3 to = new Vector3(doorPoint.x, from.y + enterRise, from.z);
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, enterDuration))
            {
                player.position = Vector3.Lerp(from, to, t);
                player.localScale = originalScale * Mathf.Lerp(1f, endScale, t);
                if (sprite != null)
                {
                    Color c = originalColor;
                    c.a = originalColor.a * Mathf.Clamp01((1f - t) / 0.5f);
                    sprite.color = c;
                }
                yield return null;
            }
            if (sprite != null)
            {
                Color c = originalColor;
                c.a = 0f;
                sprite.color = c;
            }

            // 4) A tela escurece.
            Image black = CreateBlackOverlay();
            yield return Fade(black, 0f, 1f);

            // 5) Leva o jogador para o próximo trecho (ou devolve para a frente da porta).
            player.localScale = originalScale;
            if (sprite != null) sprite.color = originalColor;
            if (teleport)
            {
                player.position = new Vector3(destination.x, destination.y, from.z);
                Camera cam = Camera.main;
                CameraFollow follow = cam != null ? cam.GetComponent<CameraFollow>() : null;
                if (follow != null)
                {
                    follow.LockToRange(destinationStartX, destinationEndX, true);
                }
                if (cam != null)
                {
                    // Já coloca a câmera no lugar final, para ela não deslizar enquanto a tela clareia.
                    float half = follow != null ? follow.ViewHalfWidth : 0f;
                    float minX = destinationStartX + half;
                    float maxX = destinationEndX - half;
                    Vector3 camPos = cam.transform.position;
                    // Trecho mais estreito que a tela: encosta a câmera na esquerda (igual ao LockToRange acima).
                    camPos.x = minX <= maxX
                        ? Mathf.Clamp(destination.x, minX, maxX)
                        : minX;
                    cam.transform.position = camPos;
                }
            }
            else
            {
                player.position = from;
            }
            if (body != null)
            {
                body.simulated = true;
                body.linearVelocity = Vector2.zero;
            }
            yield return new WaitForSeconds(blackHold);

            // 6) Clareia e devolve o controle.
            yield return Fade(black, 1f, 0f);
            Destroy(black.transform.parent.gameObject);
            if (controller != null) controller.enabled = true;
            if (shooting != null) shooting.enabled = true;
            OnFinished?.Invoke();
        }

        private static Image CreateBlackOverlay()
        {
            GameObject canvasObj = new GameObject("TelaPretaDaPorta");
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
            Gizmos.color = Color.yellow;
            float y0 = Mathf.Max(minPlayerY, doorPoint.y - 1f);
            float y1 = Mathf.Min(maxPlayerY, doorPoint.y + 3f);
            Gizmos.DrawLine(new Vector3(triggerX, y0, 0f), new Vector3(triggerX, y1, 0f));
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
            Gizmos.DrawWireSphere(new Vector3(doorPoint.x, doorPoint.y, 0f), 0.3f);
            if (teleport)
            {
                Gizmos.color = Color.magenta;
                Gizmos.DrawWireSphere(new Vector3(destination.x, destination.y, 0f), 0.3f);
            }
        }
    }
}
