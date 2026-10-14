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

        private Rigidbody2D rb;
        private SpriteRenderer spriteRenderer;

        private float horizontalInput;
        private bool jumpRequested;
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
        }

        private void Update()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            IsAimingUp = Input.GetAxisRaw("Vertical") > 0.5f;

            if (Input.GetButtonDown("Jump") && IsGrounded)
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

            if (jumpRequested)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpRequested = false;
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
