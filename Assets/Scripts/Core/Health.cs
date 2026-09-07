using System;
using UnityEngine;

namespace Redline.Core
{
    /// <summary>
    /// Componente genérico de vida. Usado tanto pelo jogador quanto pelos inimigos.
    /// Outros scripts causam dano chamando TakeDamage(); quem quiser reagir à morte
    /// (GameManager, efeitos, spawner de ondas) deve assinar o evento OnDied.
    /// </summary>
    public class Health : MonoBehaviour
    {
        [Header("Configuração")]
        [SerializeField] private int maxHealth = 100;
        [SerializeField] private bool destroyOnDeath = true;
        [Tooltip("Tempo em segundos de invulnerabilidade após tomar dano. 0 = desativado.")]
        [SerializeField] private float invulnerabilityDuration = 0f;

        public int MaxHealth => maxHealth;
        public int CurrentHealth { get; private set; }
        public bool IsDead { get; private set; }

        /// <summary>Disparado sempre que a vida muda: (vidaAtual, vidaMaxima).</summary>
        public event Action<int, int> OnHealthChanged;

        /// <summary>Disparado uma única vez quando a vida chega a zero.</summary>
        public event Action OnDied;

        private float invulnerableUntil = -1f;

        private void Awake()
        {
            CurrentHealth = maxHealth;
        }

        public void TakeDamage(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            if (invulnerabilityDuration > 0f && Time.time < invulnerableUntil)
            {
                return;
            }

            CurrentHealth = Mathf.Max(0, CurrentHealth - amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);

            if (invulnerabilityDuration > 0f)
            {
                invulnerableUntil = Time.time + invulnerabilityDuration;
            }

            if (CurrentHealth <= 0)
            {
                Die();
            }
        }

        public void Heal(int amount)
        {
            if (IsDead || amount <= 0)
            {
                return;
            }

            CurrentHealth = Mathf.Min(maxHealth, CurrentHealth + amount);
            OnHealthChanged?.Invoke(CurrentHealth, maxHealth);
        }

        private void Die()
        {
            if (IsDead)
            {
                return;
            }

            IsDead = true;
            OnDied?.Invoke();

            if (destroyOnDeath)
            {
                Destroy(gameObject);
            }
        }
    }
}
