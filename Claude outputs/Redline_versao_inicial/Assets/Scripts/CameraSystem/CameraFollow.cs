using UnityEngine;

namespace Redline.CameraSystem
{
    /// <summary>
    /// Segue o alvo (jogador) suavemente. Estilo side-scroller: normalmente
    /// trava o eixo Y para a câmera não "pular" junto com o personagem.
    /// </summary>
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private Transform target;
        [SerializeField] private float smoothTime = 0.15f;
        [SerializeField] private Vector3 offset = new Vector3(0f, 1f, -10f);
        [SerializeField] private bool lockY = true;
        [SerializeField] private float fixedY = 1f;

        private Vector3 currentVelocity;

        private void LateUpdate()
        {
            if (target == null)
            {
                return;
            }

            Vector3 desiredPosition = target.position + offset;
            if (lockY)
            {
                desiredPosition.y = fixedY;
            }

            transform.position = Vector3.SmoothDamp(transform.position, desiredPosition, ref currentVelocity, smoothTime);
        }
    }
}
