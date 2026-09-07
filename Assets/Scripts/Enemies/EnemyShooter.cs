using Redline.Combat;
using UnityEngine;

namespace Redline.Enemies
{
    /// <summary>
    /// Segundo tipo de inimigo: fica parado (não persegue) e atira Bullets no jogador
    /// quando ele está dentro do alcance de detecção. Reaproveita o mesmo Bullet.cs
    /// do jogador — só muda o layer do prefab e o LayerMask alvo (Player em vez de Enemy).
    /// Não precisa de Rigidbody2D: o Bullet que colide já tem um, e isso já é
    /// suficiente para o Unity disparar os eventos de trigger.
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class EnemyShooter : MonoBehaviour
    {
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float fireRate = 1f;
        [SerializeField] private float bulletSpeed = 10f;
        [SerializeField] private int bulletDamage = 10;
        [SerializeField] private float detectionRange = 8f;
        [SerializeField] private LayerMask playerLayer;

        private Transform playerTransform;
        private float nextFireTime;

        private void Awake()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        private void Update()
        {
            if (playerTransform == null || bulletPrefab == null)
            {
                return;
            }

            float distance = Vector2.Distance(transform.position, playerTransform.position);
            if (distance > detectionRange)
            {
                return;
            }

            if (Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            }
        }

        private void Fire()
        {
            Transform origin = firePoint != null ? firePoint : transform;
            Vector2 direction = (Vector2)(playerTransform.position - origin.position);

            GameObject bulletInstance = Instantiate(bulletPrefab, origin.position, Quaternion.identity);
            Bullet bullet = bulletInstance.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Init(direction, playerLayer, bulletDamage, bulletSpeed);
            }
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, detectionRange);
        }
    }
}
