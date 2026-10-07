using Redline.Core;
using UnityEngine;

namespace Redline.Combat
{
    /// <summary>
    /// Projétil simples: viaja em linha reta na direção definida em Init(),
    /// causa dano ao primeiro objeto do layer configurado que tocar, e se destrói.
    /// O Collider2D deste objeto precisa estar marcado como "Is Trigger".
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 20f;
        [SerializeField] private int damage = 10;
        [SerializeField] private float lifeTime = 2f;
        [SerializeField] private LayerMask hitMask;

        [Header("Efeitos (opcional)")]
        [Tooltip("Partícula instanciada no exato momento/posição em que o tiro sai.")]
        [SerializeField] private GameObject muzzleEffectPrefab;
        [Tooltip("Partícula instanciada no ponto de impacto, ao acertar um alvo válido.")]
        [SerializeField] private GameObject impactEffectPrefab;

        [Header("Áudio (opcional)")]
        [Tooltip("Som tocado no momento em que o tiro sai.")]
        [SerializeField] private AudioClip shootClip;
        [Tooltip("Som tocado ao atingir um alvo válido.")]
        [SerializeField] private AudioClip impactClip;

        private Rigidbody2D rb;
        private Vector2 direction = Vector2.right;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        /// <summary>Configura o projétil no momento em que é instanciado (chamado por PlayerShooting).</summary>
        public void Init(Vector2 travelDirection, LayerMask targetMask, int bulletDamage, float bulletSpeed)
        {
            direction = travelDirection.sqrMagnitude > 0.0001f ? travelDirection.normalized : Vector2.right;
            hitMask = targetMask;
            damage = bulletDamage;
            speed = bulletSpeed;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void Start()
        {
            if (muzzleEffectPrefab != null)
            {
                Instantiate(muzzleEffectPrefab, transform.position, transform.rotation);
            }

            if (shootClip != null)
            {
                AudioSource.PlayClipAtPoint(shootClip, transform.position);
            }

            Destroy(gameObject, lifeTime);
        }

        private void FixedUpdate()
        {
            rb.linearVelocity = direction * speed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (((1 << other.gameObject.layer) & hitMask) == 0)
            {
                return;
            }

            Health health = other.GetComponentInParent<Health>();
            if (health != null)
            {
                health.TakeDamage(damage);
            }

            if (impactEffectPrefab != null)
            {
                Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
            }

            if (impactClip != null)
            {
                AudioSource.PlayClipAtPoint(impactClip, transform.position);
            }

            Destroy(gameObject);
        }
    }
}
