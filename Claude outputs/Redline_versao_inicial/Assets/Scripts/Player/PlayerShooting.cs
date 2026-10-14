using Redline.Combat;
using UnityEngine;

namespace Redline.Player
{
    /// <summary>
    /// Dispara o prefab de Bullet a partir do fire point correto, na direção que o
    /// personagem está olhando (ou para cima, se PlayerController.IsAimingUp).
    /// </summary>
    public class PlayerShooting : MonoBehaviour
    {
        [SerializeField] private PlayerController playerController;
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePointForward;
        [SerializeField] private Transform firePointUp;
        [SerializeField] private float fireRate = 6f;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private int bulletDamage = 10;
        [SerializeField] private LayerMask enemyLayer;

        private float nextFireTime;

        private void Reset()
        {
            playerController = GetComponent<PlayerController>();
        }

        private void Update()
        {
            bool fireHeld = Input.GetButton("Fire1");
            if (fireHeld && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + 1f / Mathf.Max(0.01f, fireRate);
            }
        }

        private void Fire()
        {
            if (bulletPrefab == null)
            {
                Debug.LogWarning("[PlayerShooting] bulletPrefab não configurado no Inspector.");
                return;
            }

            Transform origin;
            Vector2 direction;
            bool aimingUp = playerController != null && playerController.IsAimingUp;

            if (aimingUp)
            {
                origin = firePointUp != null ? firePointUp : transform;
                direction = Vector2.up;
            }
            else
            {
                origin = firePointForward != null ? firePointForward : transform;
                bool facingRight = playerController == null || playerController.FacingRight;
                direction = facingRight ? Vector2.right : Vector2.left;
            }

            GameObject bulletInstance = Instantiate(bulletPrefab, origin.position, Quaternion.identity);
            Bullet bullet = bulletInstance.GetComponent<Bullet>();
            if (bullet != null)
            {
                bullet.Init(direction, enemyLayer, bulletDamage, bulletSpeed);
            }
            else
            {
                Debug.LogWarning("[PlayerShooting] bulletPrefab não tem o componente Bullet.");
            }
        }
    }
}
