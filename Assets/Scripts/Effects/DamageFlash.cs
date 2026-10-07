using System.Collections;
using Redline.Core;
using UnityEngine;

namespace Redline.Effects
{
    /// <summary>
    /// Feedback visual de dano via SHADER (efeito de CG em tempo real, calculado
    /// na GPU): troca temporariamente o material do SpriteRenderer para o
    /// material "SpriteDamageFlash" (shader "Redline/SpriteDamageFlash", em
    /// Assets/Shaders/SpriteDamageFlash.shader) e anima a propriedade
    /// _FlashAmount via MaterialPropertyBlock sempre que o Health deste objeto
    /// muda, restaurando o material normal ao final do flash.
    ///
    /// Esse desenho em duas camadas existe por causa da iluminação 2D
    /// (Light2D) do projeto: o material do dia a dia (normalMaterial) pode ser
    /// um material "Lit" (ex.: Universal Render Pipeline/2D/Sprite-Lit-Default),
    /// que reage às luzes 2D da cena, enquanto o material de flash
    /// (flashMaterial) é propositalmente um shader simples (unlit), calculado
    /// à mão, só para o breve instante do flash de dano. Assim o personagem
    /// fica iluminado normalmente na maior parte do tempo, e só perde a
    /// interação com a luz durante a fração de segundo do flash — o que não é
    /// perceptível a olho nu.
    ///
    /// CONFIGURAÇÃO NO INSPECTOR:
    /// - flashMaterial: arraste Assets/Materials/SpriteDamageFlash.mat aqui.
    /// - normalMaterial: opcional. Se deixado vazio, o script captura
    ///   automaticamente, no Awake, o material que já estiver no
    ///   SpriteRenderer no momento — ou seja, se você trocar o material do
    ///   SpriteRenderer no Editor para um material "Lit" (para a luz 2D
    ///   funcionar de verdade), não precisa mexer neste campo: ele vai
    ///   assumir esse material Lit como o estado normal automaticamente.
    /// </summary>
    [RequireComponent(typeof(Health))]
    public class DamageFlash : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private Color flashColor = Color.white;
        [SerializeField] private float flashDuration = 0.15f;

        [Header("Materiais")]
        [Tooltip("Material com o shader Redline/SpriteDamageFlash, usado apenas durante o flash.")]
        [SerializeField] private Material flashMaterial;
        [Tooltip("Material usado no estado normal (fora do flash). Se vazio, é capturado automaticamente do SpriteRenderer no Awake.")]
        [SerializeField] private Material normalMaterial;

        private static readonly int FlashAmountID = Shader.PropertyToID("_FlashAmount");
        private static readonly int FlashColorID = Shader.PropertyToID("_FlashColor");

        private Health health;
        private MaterialPropertyBlock propertyBlock;
        private Coroutine flashRoutine;

        private void Awake()
        {
            health = GetComponent<Health>();

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (normalMaterial == null && spriteRenderer != null)
            {
                normalMaterial = spriteRenderer.sharedMaterial;
            }

            propertyBlock = new MaterialPropertyBlock();
        }

        private void OnEnable()
        {
            if (health != null)
            {
                health.OnHealthChanged += HandleHealthChanged;
            }
        }

        private void OnDisable()
        {
            if (health != null)
            {
                health.OnHealthChanged -= HandleHealthChanged;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
                flashRoutine = null;
            }

            RestoreNormalMaterial();
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (spriteRenderer == null)
            {
                return;
            }

            if (flashRoutine != null)
            {
                StopCoroutine(flashRoutine);
            }

            flashRoutine = StartCoroutine(Flash());
        }

        private IEnumerator Flash()
        {
            // Enquanto o flash está ativo, usa o material do shader de dano
            // (que sabe interpolar para flashColor); fora do flash, o objeto
            // fica no material normal (que pode ser um material Lit, afetado
            // pelas luzes 2D da cena).
            if (flashMaterial != null)
            {
                spriteRenderer.material = flashMaterial;
            }

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetColor(FlashColorID, flashColor);
            spriteRenderer.SetPropertyBlock(propertyBlock);

            float elapsed = 0f;

            while (elapsed < flashDuration)
            {
                elapsed += Time.deltaTime;
                float amount = 1f - Mathf.Clamp01(elapsed / flashDuration);

                spriteRenderer.GetPropertyBlock(propertyBlock);
                propertyBlock.SetFloat(FlashAmountID, amount);
                spriteRenderer.SetPropertyBlock(propertyBlock);

                yield return null;
            }

            RestoreNormalMaterial();
            flashRoutine = null;
        }

        private void RestoreNormalMaterial()
        {
            if (spriteRenderer == null)
            {
                return;
            }

            spriteRenderer.GetPropertyBlock(propertyBlock);
            propertyBlock.SetFloat(FlashAmountID, 0f);
            spriteRenderer.SetPropertyBlock(propertyBlock);

            if (normalMaterial != null)
            {
                spriteRenderer.material = normalMaterial;
            }
        }
    }
}
