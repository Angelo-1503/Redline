using UnityEngine;

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

        private void Awake()
        {
            audioSource = GetComponent<AudioSource>();

            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }

            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.volume = volume;
            audioSource.spatialBlend = 0f; // música 2D, não posicional
        }

        private void Start()
        {
            if (musicClip == null)
            {
                return;
            }

            audioSource.clip = musicClip;
            audioSource.Play();
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

            audioSource.clip = clip;
            audioSource.Play();
        }
    }
}
