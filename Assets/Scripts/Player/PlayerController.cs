using System.Collections;
using UnityEngine;

namespace Redline.Player
{
    /// <summary>
    /// Movimento horizontal + pulo estilo run-and-gun (Contra-like).
    /// Também expõe para qual lado o personagem está olhando e a direção da mira
    /// (8 direções), para o PlayerShooting decidir a direção do tiro.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpForce = 12f;

        [Header("Detecção de chão")]
        [SerializeField] private Transform groundCheck;
        [SerializeField] private float groundCheckRadius = 0.15f;
        [SerializeField] private LayerMask groundLayer;

        [Header("Descer de plataformas")]
        [Tooltip("Tecla que faz o personagem descer da plataforma em que está (caixas, rampas, sacadas). Baixo + Pulo também funciona.")]
        [SerializeField] private KeyCode dropKey = KeyCode.C;
        [Tooltip("Por quanto tempo a plataforma deixa de segurar o personagem depois de descer.")]
        [SerializeField] private float dropDuration = 0.35f;

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private float horizontalInput;
        private bool jumpRequested;
        private bool dropRequested;
        private Collider2D[] myColliders;
        private bool facingRight = true;

        public bool FacingRight => facingRight;
        public bool IsAimingUp { get; private set; }

        /// <summary>
        /// Direção da mira em 8 direções (estilo Contra), já normalizada:
        /// frente/trás, cima, baixo e as quatro diagonais. Sem apertar cima
        /// ou baixo, é a direção para onde o personagem está olhando.
        /// </summary>
        public Vector2 AimDirection { get; private set; } = Vector2.right;
        public bool IsGrounded { get; private set; }

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            myColliders = GetComponents<Collider2D>();
        }

        private void Update()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            IsAimingUp = Input.GetAxisRaw("Vertical") > 0.5f;

            bool holdingDown = Input.GetAxisRaw("Vertical") < -0.5f;
            if (IsGrounded && (Input.GetKeyDown(dropKey) || (holdingDown && Input.GetButtonDown("Jump"))))
            {
                // Descer da plataforma (só funciona em plataformas de mão única; no chão firme não faz nada).
                dropRequested = true;
            }
            else if (Input.GetButtonDown("Jump") && IsGrounded)
            {
                jumpRequested = true;
            }

            if (horizontalInput > 0.01f)
            {
                facingRight = true;
            }
            else if (horizontalInput < -0.01f)
            {
                facingRight = false;
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = !facingRight;
            }

            AimDirection = ComputeAimDirection(horizontalInput, Input.GetAxisRaw("Vertical"));
        }

        private Vector2 ComputeAimDirection(float horizontal, float vertical)
        {
            float facing = facingRight ? 1f : -1f;
            bool holdingSide = Mathf.Abs(horizontal) > 0.01f;

            if (vertical > 0.5f)
            {
                // Cima sozinho = reto para cima; cima + lado = diagonal para cima.
                return holdingSide ? new Vector2(facing, 1f).normalized : Vector2.up;
            }

            if (vertical < -0.5f)
            {
                // Baixo sozinho = reto para baixo; baixo + lado = diagonal para baixo.
                return holdingSide ? new Vector2(facing, -1f).normalized : Vector2.down;
            }

            return new Vector2(facing, 0f);
        }

        private void FixedUpdate()
        {
            IsGrounded = groundCheck != null &&
                         Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

            rb.linearVelocity = new Vector2(horizontalInput * moveSpeed, rb.linearVelocity.y);

            if (dropRequested)
            {
                dropRequested = false;
                DropThroughPlatforms();
            }

            if (jumpRequested)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpRequested = false;
            }
        }

        /// <summary>
        /// Solta o personagem das plataformas de mão única que estão embaixo dos pés
        /// (as criadas pelo MapPlatforms), para ele cair até o que tiver embaixo.
        /// </summary>
        private void DropThroughPlatforms()
        {
            if (groundCheck == null)
            {
                return;
            }

            Collider2D[] under = Physics2D.OverlapCircleAll(groundCheck.position, groundCheckRadius + 0.1f, groundLayer);
            foreach (Collider2D platform in under)
            {
                PlatformEffector2D effector = platform.GetComponent<PlatformEffector2D>();
                if (platform.usedByEffector && effector != null && effector.useOneWay)
                {
                    StartCoroutine(IgnorePlatformFor(platform));
                }
            }
        }

        private IEnumerator IgnorePlatformFor(Collider2D platform)
        {
            foreach (Collider2D mine in myColliders)
            {
                if (mine != null) Physics2D.IgnoreCollision(mine, platform, true);
            }

            // Um empurrãozinho para baixo para sair de cima da plataforma na hora.
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, Mathf.Min(rb.linearVelocity.y, -2f));
            yield return new WaitForSeconds(dropDuration);

            foreach (Collider2D mine in myColliders)
            {
                if (mine != null && platform != null) Physics2D.IgnoreCollision(mine, platform, false);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheck == null)
            {
                return;
            }

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}
