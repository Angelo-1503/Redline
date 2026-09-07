using Redline.Core;
using UnityEngine;

namespace Redline.Enemies
{
    /// <summary>
    /// Inimigo mínimo para a fatia vertical: anda em direção ao jogador e causa
    /// dano por contato. A vida e a morte são tratadas pelo componente Health
    /// (que já deve estar no mesmo GameObject).
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class EnemyBasic : MonoBehaviour
    {
        [SerializeField] private float moveSpeed = 2.5f;
        [SerializeField] private int contactDamage = 10;
        [SerializeField] private float contactDamageCooldown = 1f;
        [SerializeField] private LayerMask playerLayer;

        private Rigidbody2D rb;
        private Transform playerTransform;
        private float nextContactDamageTime;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();

            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void FixedUpdate()
        {
            if (playerTransform == null)
            {
                return;
            }

            float direction = Mathf.Sign(playerTransform.position.x - transform.position.x);
            rb.linearVelocity = new Vector2(direction * moveSpeed, rb.linearVelocity.y);

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * (direction >= 0f ? 1f : -1f);
            transform.localScale = scale;
        }

        private void OnCollisionStay2D(Collision2D collision)
        {
            TryDamagePlayer(collision.collider);
        }

        private void OnTriggerStay2D(Collider2D other)
        {
            TryDamagePlayer(other);
        }

        private void TryDamagePlayer(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & playerLayer) == 0)
            {
                return;
            }

            if (Time.time < nextContactDamageTime)
            {
                return;
            }

            Health health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(contactDamage);
                nextContactDamageTime = Time.time + contactDamageCooldown;
            }
        }
    }
}
