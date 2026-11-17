using System.Collections;
using UnityEngine;
using Redline.Settings;

namespace Redline.Audio
{
    /// <summary>
    /// Toca a música de fundo da fase em loop.
    ///
    /// O AudioSource é adicionado por código (AddComponent) em vez de já vir
    /// configurado na cena, para não arriscar uma serialização manual
    /// incorreta do componente nativo do Unity — a mesma lógica usada em
    /// Redline.Effects.SimpleBurstEffect para o ParticleSystem.
    ///
    /// CONFIGURAÇÃO: no objeto GameManager da cena, arraste um AudioClip no
    /// campo "Music Clip" deste componente. Sem clipe configurado, o
    /// componente simplesmente fica em silêncio (não gera erro).
    /// </summary>
    public class MusicManager : MonoBehaviour
    {
        [Header("Música de Fundo")]
        [SerializeField] private AudioClip musicClip;
        [Range(0f, 1f)]
        [SerializeField] private float volume = 0.5f;

        private AudioSource audioSource;
        private float fadeGain = 1f;
        private Coroutine fadeRoutine;

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = volume * GameSettings.MusicVolume;
            audioSource.spatialBlend = 0f; // música 2D, não posicional
        }

        private void OnEnable() { GameSettings.Changed += ApplyVolume; }
        private void OnDisable() { GameSettings.Changed -= ApplyVolume; }
        private void ApplyVolume()
        {
            if (audioSource != null) audioSource.volume = volume * GameSettings.MusicVolume * fadeGain;
        }

        private void Start()
        {
            if (musicClip == null)
            {
                return;
            }

            PlayClip(musicClip);
        }

        /// <summary>Troca a música tocando (por exemplo, ao entrar na luta do chefe).</summary>
        public void PlayClip(AudioClip clip, bool restartIfSame = false)
        {
            if (clip == null || audioSource == null)
            {
                return;
            }

            if (!restartIfSame && audioSource.clip == clip && audioSource.isPlaying)
            {
                return;
            }

            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeGain = 1f;
            ApplyVolume();
            audioSource.clip = clip;
            audioSource.Play();
        }

        /// <summary>Encerra a trilha sem permitir que ajustes de volume a reativem durante o final.</summary>
        public void FadeOut(float duration)
        {
            if (fadeRoutine != null) StopCoroutine(fadeRoutine);
            fadeRoutine = StartCoroutine(FadeToSilence(duration));
        }

        private IEnumerator FadeToSilence(float duration)
        {
            float initialGain = fadeGain;
            for (float elapsed = 0f; elapsed < duration; elapsed += Time.deltaTime)
            {
                fadeGain = Mathf.Lerp(initialGain, 0f, elapsed / duration);
                ApplyVolume();
                yield return null;
            }
            fadeGain = 0f;
            ApplyVolume();
            if (audioSource != null) audioSource.Stop();
            fadeRoutine = null;
        }
    }
}
