using System.Collections;
using Redline.Player;
using Redline.Core;
using UnityEngine;
using Redline.Settings;
using UnityEngine.UI;

namespace Redline.Level
{
    /// <summary>
    /// Abertura de cada fase: a cena começa com a tela preta e o título da
    /// fase (ex.: "FASE 2" / "Nuke"); depois a tela clareia e o jogador ganha o
    /// controle. Opcional: um aviso que aparece logo depois, no meio da tela
    /// (ex.: "BOMB HAS BEEN PLANTED" na Fase 6), com um áudio opcional.
    ///
    /// FadeToBlack() é usado pelo BossSpawner para escurecer a tela antes de
    /// carregar a próxima fase, então a troca de fase fica: escurece -> carrega
    /// -> título da fase nova -> clareia.
    /// </summary>
    public class LevelIntro : MonoBehaviour
    {
        [SerializeField] private string title = "FASE 1";
        [SerializeField] private string subtitle = "";
        [SerializeField] private float titleHold = 1.8f;
        [SerializeField] private float fadeDuration = 0.9f;

        [Header("Áudio da abertura")]
        [SerializeField] private AudioClip introClip;
        [Range(0f, 1f)]
        [SerializeField] private float introVolume = 0.9f;

        [Header("Aviso depois da abertura (opcional)")]
        [SerializeField] private string announcement = "";
        [SerializeField] private Color announcementColor = new Color(1f, 0.6f, 0.15f, 1f);
        [SerializeField] private float announcementHold = 2.5f;
        [Tooltip("Som tocado junto com o aviso (ex.: a voz \"bomb has been planted\").")]
        [SerializeField] private AudioClip announcementClip;
        [SerializeField] private float announcementVolume = 1f;

        private Canvas canvas;

        private void Start()
        {
            StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            PlayerController controller = playerObj != null ? playerObj.GetComponent<PlayerController>() : null;
            PlayerShooting shooting = playerObj != null ? playerObj.GetComponent<PlayerShooting>() : null;
            Health health = playerObj != null ? playerObj.GetComponent<Health>() : null;
            bool wasInvulnerable = health != null && health.IsInvulnerable;
            bool controllerWasEnabled = controller != null && controller.enabled;
            bool shootingWasEnabled = shooting != null && shooting.enabled;
            if (health != null) health.IsInvulnerable = true;
            if (controller != null) controller.enabled = false;
            if (shooting != null) shooting.enabled = false;

            canvas = CreateCanvas("AberturaDaFase", 900);
            Image black = CreateBlack(canvas.transform);
            Text titleText = CreateText(canvas.transform, title, 84, new Vector2(0.5f, 0.56f), Color.white);
            Text subText = CreateText(canvas.transform, subtitle, 44, new Vector2(0.5f, 0.44f), new Color(1f, 0.75f, 0.4f, 1f));

            PlayAudioClip(introClip, introVolume);
            yield return FadeTexts(new[] { titleText, subText }, 0f, 1f, 0.4f);
            yield return new WaitForSeconds(titleHold);
            yield return FadeTexts(new[] { titleText, subText }, 1f, 0f, 0.4f);

            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, fadeDuration))
            {
                black.color = new Color(0f, 0f, 0f, 1f - t);
                yield return null;
            }
            black.color = new Color(0f, 0f, 0f, 0f);

            if (health != null) health.IsInvulnerable = wasInvulnerable;
            if (health == null || !health.IsDead)
            {
                if (controller != null) controller.enabled = controllerWasEnabled;
                if (shooting != null) shooting.enabled = shootingWasEnabled;
            }

            if (!string.IsNullOrEmpty(announcement))
            {
                PlayAudioClip(announcementClip, announcementVolume);
                Text banner = CreateText(canvas.transform, announcement, 56, new Vector2(0.5f, 0.78f), announcementColor);
                banner.GetComponent<Outline>().effectDistance = new Vector2(3f, -3f);
                yield return FadeTexts(new[] { banner }, 0f, 1f, 0.25f);
                yield return new WaitForSeconds(announcementHold);
                yield return FadeTexts(new[] { banner }, 1f, 0f, 0.6f);
            }

            Destroy(canvas.gameObject);
        }

        /// <summary>Escurece a tela inteira (usado antes de trocar de cena).</summary>
        public static IEnumerator FadeToBlack(float duration)
        {
            Canvas c = CreateCanvas("TrocaDeFase", 2000);
            Image black = CreateBlack(c.transform);
            black.color = new Color(0f, 0f, 0f, 0f);
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, duration))
            {
                black.color = new Color(0f, 0f, 0f, t);
                yield return null;
            }
            black.color = Color.black;
        }

        private static IEnumerator FadeTexts(Text[] texts, float from, float to, float duration)
        {
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, duration))
            {
                foreach (Text text in texts) SetAlpha(text, Mathf.Lerp(from, to, t));
                yield return null;
            }
            foreach (Text text in texts) SetAlpha(text, to);
        }

        private static void SetAlpha(Text text, float a)
        {
            Color c = text.color;
            c.a = a;
            text.color = c;
            Outline outline = text.GetComponent<Outline>();
            if (outline != null) outline.effectColor = new Color(0f, 0f, 0f, a);
        }

        private void PlayAudioClip(AudioClip clip, float volume)
        {
            if (clip == null) return;
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.PlayOneShot(clip, volume * GameSettings.EffectsVolume);
            Destroy(source, clip.length + 0.1f);
        }

        private static Canvas CreateCanvas(string name, int order)
        {
            GameObject obj = new GameObject(name);
            Canvas c = obj.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = order;
            CanvasScaler scaler = obj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            return c;
        }

        private static Image CreateBlack(Transform parent)
        {
            GameObject obj = new GameObject("Preto");
            obj.transform.SetParent(parent, false);
            Image image = obj.AddComponent<Image>();
            image.color = Color.black;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }

        private static Text CreateText(Transform parent, string content, int size, Vector2 anchor, Color color)
        {
            GameObject obj = new GameObject("Texto");
            obj.transform.SetParent(parent, false);
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = FontStyle.Bold;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            color.a = 0f;
            text.color = color;
            Outline outline = obj.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0f);
            outline.effectDistance = new Vector2(2f, -2f);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(1600f, 140f);
            return text;
        }
    }
}
