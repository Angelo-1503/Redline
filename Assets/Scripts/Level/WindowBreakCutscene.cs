using System;
using System.Collections;
using System.Collections.Generic;
using Redline.CameraSystem;
using Redline.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Redline.Level
{
    /// <summary>
    /// Cena do janelão da Fase 2 (a sala do "vidro" da Nuke).
    ///
    /// Quando o jogador chega na frente do janelão, ele perde o controle por
    /// alguns segundos: o personagem atira sozinho no vidro, o vidro racha e
    /// estilhaça, o personagem pula para dentro da janela (encolhendo e
    /// sumindo, como o Mario entrando no cano), a tela escurece e o jogador é
    /// levado para a arena do chefão.
    ///
    /// Fica no objeto "Vidro" da cena, que tem um SpriteRenderer desenhado por
    /// cima do janelão da tela 9. O SpriteRenderer começa sem sprite (o vidro
    /// inteiro já está pintado na tela) e vai recebendo os quadros rachados.
    ///
    /// ENQUANTO A ARENA NÃO EXISTE: deixe "Teleport To Arena" desmarcado. A
    /// cena roda inteira e, depois de escurecer, devolve o jogador na frente
    /// do vidro quebrado. Quando a arena estiver pronta, marque a opção e
    /// preencha "Arena Spawn", "Arena Start X" e "Arena End X".
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class WindowBreakCutscene : MonoBehaviour
    {
        [Header("Gatilho")]
        [Tooltip("X do mundo a partir do qual a cena começa (o jogador precisa estar no piso da sala).")]
        [SerializeField] private float triggerX = 0f;

        [Header("Vidro")]
        [Tooltip("Quadros do vidro rachando, na ordem. Um tiro para cada quadro.")]
        [SerializeField] private Sprite[] crackFrames = new Sprite[0];
        [Tooltip("Quadro final, com o vidro estilhaçado.")]
        [SerializeField] private Sprite brokenFrame;
        [Tooltip("Tamanho do vidro em unidades (largura, altura). O canto de baixo à esquerda é a posição deste objeto.")]
        [SerializeField] private Vector2 windowSize = new Vector2(5f, 2f);
        [Tooltip("Onde cada tiro acerta, em unidades a partir do canto de baixo à esquerda do vidro.")]
        [SerializeField] private Vector2[] impactPoints = new Vector2[0];
        [SerializeField] private float shotInterval = 0.3f;
        [SerializeField] private Color shardColor = new Color(0.82f, 0.93f, 0.97f, 0.9f);

        [Header("Pulo para dentro da janela")]
        [SerializeField] private float jumpDuration = 0.55f;
        [SerializeField] private float jumpArcHeight = 0.8f;
        [SerializeField] private float endScale = 0.45f;

        [Header("Transição")]
        [SerializeField] private float fadeDuration = 0.6f;
        [SerializeField] private float blackHold = 0.4f;

        [Header("Destino (arena do chefão)")]
        [Tooltip("Desmarcado: depois de escurecer, o jogador volta para a frente do vidro (para testar a cena sem a arena).")]
        [SerializeField] private bool teleportToArena = false;
        [SerializeField] private Vector2 arenaSpawn;
        [SerializeField] private float arenaStartX;
        [SerializeField] private float arenaEndX;

        /// <summary>Disparado quando a cena termina e o controle volta para o jogador.</summary>
        public event Action OnFinished;

        private SpriteRenderer glassRenderer;
        private Transform player;
        private bool started;
        private static Sprite shardSprite;

        private void Awake()
        {
            glassRenderer = GetComponent<SpriteRenderer>();
        }

        private void Start()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogWarning("[WindowBreakCutscene] Não encontrou o objeto com a tag Player.");
            }
        }

        private void Update()
        {
            if (started || player == null)
            {
                return;
            }

            // Só dispara com o jogador no piso da sala (acima da base do vidro menos a mureta).
            bool inFrontOfWindow = player.position.x >= triggerX;
            bool onRoomFloor = player.position.y >= transform.position.y - 1.5f;
            if (inFrontOfWindow && onRoomFloor)
            {
                started = true;
                StartCoroutine(RunCutscene());
            }
        }

        private IEnumerator RunCutscene()
        {
            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerShooting shooting = player.GetComponent<PlayerShooting>();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            SpriteRenderer playerSprite = player.GetComponentInChildren<SpriteRenderer>();

            // 1) Tira o controle do jogador e vira o personagem para a direita.
            if (controller != null) controller.enabled = false;
            if (shooting != null) shooting.enabled = false;
            if (body != null) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            if (playerSprite != null) playerSprite.flipX = false;
            yield return new WaitForSeconds(0.35f);
            if (body != null) body.linearVelocity = Vector2.zero;

            // 2) Um tiro para cada quadro de rachadura.
            for (int i = 0; i < crackFrames.Length; i++)
            {
                Vector3 hitPoint = GetImpactPoint(i);
                yield return FireAt(shooting, hitPoint);
                glassRenderer.sprite = crackFrames[i];
                SpawnShards(hitPoint, 6, 2.5f);
                yield return new WaitForSeconds(shotInterval);
            }

            // 3) O vidro estilhaça.
            if (brokenFrame != null)
            {
                glassRenderer.sprite = brokenFrame;
            }
            Vector3 center = transform.position + new Vector3(windowSize.x * 0.5f, windowSize.y * 0.5f, 0f);
            for (int i = 0; i < 5; i++)
            {
                Vector3 p = transform.position + new Vector3(
                    UnityEngine.Random.Range(0.15f, 0.85f) * windowSize.x,
                    UnityEngine.Random.Range(0.25f, 0.8f) * windowSize.y, 0f);
                SpawnShards(p, 7, 4f);
            }
            yield return new WaitForSeconds(0.45f);

            // 4) O personagem pula para dentro da janela, encolhendo e sumindo.
            Vector3 from = player.position;
            Vector3 originalScale = player.localScale;
            Color originalColor = playerSprite != null ? playerSprite.color : Color.white;
            if (body != null) body.simulated = false;
            Vector3 to = center + new Vector3(0f, -windowSize.y * 0.15f, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, jumpDuration))
            {
                Vector3 pos = Vector3.Lerp(from, to, t);
                pos.y += Mathf.Sin(t * Mathf.PI) * jumpArcHeight;
                player.position = pos;
                player.localScale = originalScale * Mathf.Lerp(1f, endScale, t);
                if (playerSprite != null)
                {
                    Color c = originalColor;
                    c.a = originalColor.a * Mathf.Clamp01((1f - t) / 0.4f);
                    playerSprite.color = c;
                }
                yield return null;
            }
            if (playerSprite != null)
            {
                Color c = originalColor;
                c.a = 0f;
                playerSprite.color = c;
            }

            // 5) A tela escurece.
            Image black = CreateBlackOverlay();
            yield return Fade(black, 0f, 1f);

            // 6) Leva o jogador para a arena (ou devolve para a sala, se a arena ainda não existe).
            player.localScale = originalScale;
            if (playerSprite != null) playerSprite.color = originalColor;
            if (teleportToArena)
            {
                player.position = new Vector3(arenaSpawn.x, arenaSpawn.y, from.z);
                Camera cam = Camera.main;
                CameraFollow follow = cam != null ? cam.GetComponent<CameraFollow>() : null;
                if (follow != null)
                {
                    follow.LockToRange(arenaStartX, arenaEndX);
                }
                if (cam != null)
                {
                    // Já coloca a câmera no lugar final, para ela não deslizar enquanto a tela clareia.
                    float half = follow != null ? follow.ViewHalfWidth : 0f;
                    float minX = arenaStartX + half;
                    float maxX = arenaEndX - half;
                    Vector3 camPos = cam.transform.position;
                    camPos.x = minX <= maxX
                        ? Mathf.Clamp(arenaSpawn.x, minX, maxX)
                        : (arenaStartX + arenaEndX) * 0.5f;
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

            // 7) Clareia e devolve o controle.
            yield return Fade(black, 1f, 0f);
            Destroy(black.transform.parent.gameObject);
            if (controller != null) controller.enabled = true;
            if (shooting != null) shooting.enabled = true;
            OnFinished?.Invoke();
        }

        private Vector3 GetImpactPoint(int index)
        {
            Vector2 local = impactPoints != null && index < impactPoints.Length
                ? impactPoints[index]
                : new Vector2(windowSize.x * 0.5f, windowSize.y * 0.5f);
            return transform.position + new Vector3(local.x, local.y, 0f);
        }

        /// <summary>Dispara um projétil de verdade do jogador até o ponto do vidro e espera ele chegar.</summary>
        private IEnumerator FireAt(PlayerShooting shooting, Vector3 hitPoint)
        {
            if (shooting == null)
            {
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            Vector2 direction = (Vector2)(hitPoint - player.position);
            GameObject bullet = shooting.FireInDirection(direction);
            if (bullet == null)
            {
                yield return new WaitForSeconds(0.1f);
                yield break;
            }

            // Espera o projétil chegar no vidro (ou no máximo meio segundo) e some com ele.
            float timeout = 0.5f;
            while (bullet != null && timeout > 0f)
            {
                Vector2 toTarget = (Vector2)(hitPoint - bullet.transform.position);
                if (Vector2.Dot(toTarget, direction) <= 0f)
                {
                    break;
                }
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (bullet != null)
            {
                Destroy(bullet);
            }
        }

        // ------------------------------------------------------------------
        // Cacos de vidro: quadradinhos claros que voam, giram, caem e somem.
        // Feitos com SpriteRenderer simples, sem depender de prefab nem de
        // material configurado no Inspector.
        // ------------------------------------------------------------------
        private class Shard
        {
            public Transform transform;
            public SpriteRenderer renderer;
            public Vector2 velocity;
            public float spin;
            public float life;
            public float maxLife;
        }

        private void SpawnShards(Vector3 origin, int count, float speed)
        {
            if (shardSprite == null)
            {
                Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
                tex.SetPixel(0, 0, Color.white);
                tex.filterMode = FilterMode.Point;
                tex.Apply();
                shardSprite = Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
            }

            List<Shard> shards = new List<Shard>(count);
            for (int i = 0; i < count; i++)
            {
                GameObject go = new GameObject("CacoDeVidro");
                go.transform.position = origin + (Vector3)(UnityEngine.Random.insideUnitCircle * 0.2f);
                float size = UnityEngine.Random.Range(0.05f, 0.14f);
                go.transform.localScale = new Vector3(size, size * UnityEngine.Random.Range(0.5f, 1.6f), 1f);
                go.transform.rotation = Quaternion.Euler(0f, 0f, UnityEngine.Random.Range(0f, 360f));

                SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                sr.sprite = shardSprite;
                sr.color = shardColor;
                sr.sortingOrder = glassRenderer.sortingOrder + 1;

                float angle = UnityEngine.Random.Range(20f, 160f) * Mathf.Deg2Rad;
                float life = UnityEngine.Random.Range(0.5f, 0.9f);
                shards.Add(new Shard
                {
                    transform = go.transform,
                    renderer = sr,
                    velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * (speed * UnityEngine.Random.Range(0.4f, 1f)),
                    spin = UnityEngine.Random.Range(-540f, 540f),
                    life = life,
                    maxLife = life
                });
            }
            StartCoroutine(AnimateShards(shards));
        }

        private IEnumerator AnimateShards(List<Shard> shards)
        {
            const float gravity = 14f;
            bool anyAlive = true;
            while (anyAlive)
            {
                anyAlive = false;
                float dt = Time.deltaTime;
                foreach (Shard s in shards)
                {
                    if (s.transform == null)
                    {
                        continue;
                    }

                    s.life -= dt;
                    if (s.life <= 0f)
                    {
                        Destroy(s.transform.gameObject);
                        s.transform = null;
                        continue;
                    }

                    anyAlive = true;
                    s.velocity.y -= gravity * dt;
                    s.transform.position += (Vector3)(s.velocity * dt);
                    s.transform.Rotate(0f, 0f, s.spin * dt);
                    Color c = shardColor;
                    c.a = shardColor.a * Mathf.Clamp01(s.life / (s.maxLife * 0.5f));
                    s.renderer.color = c;
                }
                yield return null;
            }
        }

        // ------------------------------------------------------------------
        // Tela preta por cima de tudo, criada por código.
        // ------------------------------------------------------------------
        private static Image CreateBlackOverlay()
        {
            GameObject canvasObj = new GameObject("TelaPretaDaCena");
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

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0.4f, 0.9f, 1f, 0.9f);
            Vector3 size = new Vector3(windowSize.x, windowSize.y, 0f);
            Gizmos.DrawWireCube(transform.position + size * 0.5f, size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(new Vector3(triggerX, transform.position.y - 1.5f, 0f), new Vector3(triggerX, transform.position.y + windowSize.y, 0f));
        }
    }
}
