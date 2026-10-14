using System.Collections;
using Redline.Combat;
using Redline.Core;
using UnityEngine;

namespace Redline.Enemies
{
    /// <summary>
    /// Chefão de fim de fase. Reaproveita Health.cs (dano/morte) e Bullet.cs (projéteis)
    /// já usados pelos inimigos comuns — só adiciona um comportamento mais elaborado:
    ///
    ///  - Movimento de "kiting": tenta manter uma distância preferida do jogador
    ///    (se afasta se ele chegar perto demais, se aproxima se ele fugir).
    ///  - Ataque simples: atira um projétil mirado no jogador em intervalos regulares.
    ///  - Fase 2 ("enfurecido", abaixo de X% de vida): atira mais rápido, se move mais
    ///    rápido, e passa a intercalar com um ataque em leque de 3 projéteis.
    ///  - Dano por contato, igual ao EnemyBasic, caso o jogador encoste nele.
    ///
    /// Configure a vida alta (ex: 300) no componente Health deste GameObject —
    /// o script não mexe em MaxHealth, só lê CurrentHealth/MaxHealth pra decidir a fase.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Health))]
    public class BossEnemy : MonoBehaviour
    {
        [Header("Movimento")]
        [SerializeField] private float moveSpeed = 2f;
        [SerializeField] private float enragedMoveSpeedMultiplier = 1.4f;
        [SerializeField] private float preferredDistance = 5f;
        [SerializeField] private float distanceBuffer = 0.75f;

        [Header("Ataque simples")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 0.7f;
        [SerializeField] private float enragedFireRateMultiplier = 1.8f;
        [SerializeField] private float bulletSpeed = 9f;
        [SerializeField] private int bulletDamage = 10;
        [SerializeField] private LayerMask playerLayer;

        [Header("Ataque em leque (fase enfurecida)")]
        [SerializeField] private int burstBulletCount = 3;
        [SerializeField] private float burstSpreadAngle = 25f;
        [SerializeField] private float burstShotInterval = 0.12f;
        [SerializeField] private float burstCooldown = 3f;

        [Header("Fases")]
        [Range(0.1f, 0.9f)]
        [SerializeField] private float enragedHealthPercent = 0.5f;

        [Header("Dano por contato")]
        [SerializeField] private int contactDamage = 15;
        [SerializeField] private float contactDamageCooldown = 1f;

        private Rigidbody2D rb;
        private Health health;
        private Transform playerTransform;

        private float nextFireTime;
        private float nextBurstTime;
        private float nextContactDamageTime;
        private bool isBursting;

        private bool IsEnraged => health != null && health.MaxHealth > 0
            && health.CurrentHealth <= health.MaxHealth * enragedHealthPercent;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            health = GetComponent<Health>();

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

            float distanceX = playerTransform.position.x - transform.position.x;
            float absDistance = Mathf.Abs(distanceX);
            float currentSpeed = IsEnraged ? moveSpeed * enragedMoveSpeedMultiplier : moveSpeed;

            float moveDir;
            if (absDistance > preferredDistance + distanceBuffer)
            {
                moveDir = Mathf.Sign(distanceX);
            }
            else if (absDistance < preferredDistance - distanceBuffer)
            {
                moveDir = -Mathf.Sign(distanceX);
            }
            else
            {
                moveDir = 0f;
            }

            rb.linearVelocity = new Vector2(moveDir * currentSpeed, rb.linearVelocity.y);

            if (Mathf.Abs(distanceX) > 0.01f)
            {
                Vector3 scale = transform.localScale;
                scale.x = Mathf.Abs(scale.x) * (distanceX >= 0f ? 1f : -1f);
                transform.localScale = scale;
            }
        }

        private void Update()
        {
            if (playerTransform == null || bulletPrefab == null || health == null || health.IsDead)
            {
                return;
            }

            bool enraged = IsEnraged;
            float currentFireRate = enraged ? fireRate * enragedFireRateMultiplier : fireRate;

            if (Time.time >= nextFireTime)
            {
                FireSingleShot();
                nextFireTime = Time.time + 1f / Mathf.Max(0.01f, currentFireRate);
            }

            if (enraged && !isBursting && Time.time >= nextBurstTime)
            {
                StartCoroutine(FireBurst());
                nextBurstTime = Time.time + burstCooldown;
            }
        }

        private void FireSingleShot()
        {
            Vector2 direction = AimDirection();
            SpawnBullet(direction);
        }

        private IEnumerator FireBurst()
        {
            isBursting = true;
            Vector2 baseDirection = AimDirection();
            float baseAngle = Mathf.Atan2(baseDirection.y, baseDirection.x) * Mathf.Rad2Deg;

            int half = burstBulletCount / 2;
            for (int i = 0; i < burstBulletCount; i++)
            {
                float offset = (i - half) * burstSpreadAngle;
                float angleRad = (baseAngle + offset) * Mathf.Deg2Rad;
                Vector2 shotDirection = new Vector2(Mathf.Cos(angleRad), Mathf.Sin(angleRad));

                SpawnBullet(shotDirection);
                yield return new WaitForSeconds(burstShotInterval);
            }

            isBursting = false;
        }

        private Vector2 AimDirection()
        {
            Transform origin = firePoint != null ? firePoint : transform;
            return (Vector2)(playerTransform.position - origin.position);
        }

        private void SpawnBullet(Vector2 direction)
        {
            Transform origin = firePoint != null ? firePoint : transform;
            GameObject bulletInstance = Instantiate(bulletPrefab, origin.position, Quaternion.identity);
            Bullet bullet = bulletInstance.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Init(direction, playerLayer, bulletDamage, bulletSpeed);
            }
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

            Health playerHealth = other.GetComponentInParent<Health>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
                nextContactDamageTime = Time.time + contactDamageCooldown;
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(transform.position, preferredDistance);
        }
    }
}
