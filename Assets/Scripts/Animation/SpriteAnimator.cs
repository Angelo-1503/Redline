using UnityEngine;

namespace Redline.Animations
{
    /// <summary>
    /// Animador de sprites simples, quadro a quadro, para os personagens.
    /// Alterna entre o sprite parado (idle) e o ciclo de corrida conforme a
    /// velocidade horizontal do objeto.
    ///
    /// A velocidade vem do Rigidbody2D quando existe (EnemyBasic, Player) ou,
    /// se não houver Rigidbody2D (EnemyShooter), é calculada pela variação de
    /// posição entre frames. Assim o mesmo componente serve para todos.
    ///
    /// Roda no LateUpdate, depois da lógica de movimento, e só troca o
    /// "sprite" do SpriteRenderer: não mexe em material, cor nem flipX, então
    /// convive com o DamageFlash e com o espelhamento por localScale/flipX.
    ///
    /// CONFIGURAÇÃO NO INSPECTOR:
    /// - idleSprite: sprite parado. Se vazio, usa o sprite que já está no
    ///   SpriteRenderer.
    /// - runFrames: frames da corrida, em ordem (ex.: Insurgencia_run_0..5).
    /// - runFps: velocidade da animação (10–14 fica bom para 6 frames).
    /// </summary>
    public class SpriteAnimator : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;

        [Header("Sprites")]
        [SerializeField] private Sprite idleSprite;
        [SerializeField] private Sprite[] runFrames;

        [Header("Corrida")]
        [SerializeField] private float runFps = 12f;
        [Tooltip("Velocidade horizontal mínima (unidades/s) para considerar que está correndo.")]
        [SerializeField] private float moveThreshold = 0.1f;

        private Rigidbody2D rb;
        private Vector3 lastPosition;
        private float frameTimer;

        public bool IsRunning { get; private set; }

        private void Awake()
        {
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (idleSprite == null && spriteRenderer != null)
            {
                idleSprite = spriteRenderer.sprite;
            }

            rb = GetComponent<Rigidbody2D>();
            lastPosition = transform.position;
        }

        /// <summary>
        /// Troca os sprites em tempo de execução (usado pelo LevelFaction para
        /// aplicar a aparência do jogador conforme a facção da fase).
        /// </summary>
        public void SetSprites(Sprite idle, Sprite[] run)
        {
            idleSprite = idle;
            runFrames = run;
            frameTimer = 0f;

            if (spriteRenderer != null && idleSprite != null)
            {
                spriteRenderer.sprite = idleSprite;
            }
        }

        private void LateUpdate()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            float horizontalSpeed = GetHorizontalSpeed();
            IsRunning = horizontalSpeed > moveThreshold && runFrames != null && runFrames.Length > 0;

            if (IsRunning)
            {
                frameTimer += Time.deltaTime * runFps;
                int frame = (int)frameTimer % runFrames.Length;
                spriteRenderer.sprite = runFrames[frame];
            }
            else
            {
                frameTimer = 0f;
                if (idleSprite != null)
                {
                    spriteRenderer.sprite = idleSprite;
                }
            }
        }

        private float GetHorizontalSpeed()
        {
            if (rb != null)
            {
                return Mathf.Abs(rb.linearVelocity.x);
            }

            Vector3 position = transform.position;
            float speed = Time.deltaTime > 0f
                ? Mathf.Abs(position.x - lastPosition.x) / Time.deltaTime
                : 0f;
            lastPosition = position;
            return speed;
        }
    }
}
