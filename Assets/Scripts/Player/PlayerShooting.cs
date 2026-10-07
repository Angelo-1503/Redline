using Redline.Combat;
using UnityEngine;

namespace Redline.Player
{
    /// <summary>
    /// Dispara o prefab de Bullet na direção da mira do PlayerController, em
    /// 8 direções (estilo Contra): frente/trás, cima, baixo e as diagonais.
    /// Segure cima ou baixo para mirar na vertical; junto com esquerda/direita,
    /// o tiro sai na diagonal.
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

            bool facingRight = playerController == null || playerController.FacingRight;
            Vector2 direction = playerController != null
                ? playerController.AimDirection
                : (facingRight ? Vector2.right : Vector2.left);

            Vector3 spawnPosition = GetMuzzlePosition(direction);

            GameObject bulletInstance = Instantiate(bulletPrefab, spawnPosition, Quaternion.identity);
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

        /// <summary>
        /// Ponto de onde o tiro sai para a direção de mira dada. A arma gira em
        /// volta de um pivô na altura do fire point da frente: o tiro sai a uma
        /// distância fixa desse pivô, na direção da mira. Para a frente, isso dá
        /// exatamente o fire point da frente (espelhado quando olha para a esquerda).
        /// </summary>
        private Vector3 GetMuzzlePosition(Vector2 direction)
        {
            if (direction == Vector2.up && firePointUp != null)
            {
                return firePointUp.position;
            }

            float gunHeight = 0.4f;
            float muzzleDistance = 0.72f;
            if (firePointForward != null)
            {
                Vector3 local = transform.InverseTransformPoint(firePointForward.position);
                gunHeight = local.y;
                muzzleDistance = Mathf.Abs(local.x);
            }

            Vector3 localMuzzle = new Vector3(direction.x * muzzleDistance, gunHeight + direction.y * muzzleDistance, 0f);
            return transform.TransformPoint(localMuzzle);
        }
    }
}
