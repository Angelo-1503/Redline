using System.Collections;
using Redline.Core;
using Redline.Menu;
using UnityEngine.SceneManagement;
using Redline.Player;
using Redline.Waves;
using UnityEngine;
using Redline.Settings;
using UnityEngine.UI;

namespace Redline.Level
{
    /// <summary>
    /// Final da Fase 6. Quando o chefão morre, o jogador perde o controle, anda
    /// até a C4 e se ajoelha para desarmar. Toca o áudio da C4 (bips acelerando
    /// e a explosão): o LED pisca junto com os bips, a barra de desarme enche,
    /// mas não dá tempo — na explosão a tela fica branca, escurece e aparece a
    /// frase final.
    ///
    /// O áudio (bombClip) é o arquivo do CS com "bomb has been planted" no
    /// começo: ele é tocado a partir de clipStart, pulando a voz (ela só toca
    /// no começo da fase, pelo LevelIntro). Os tempos abaixo são segundos
    /// DENTRO do arquivo.
    /// Durante a fase, o LED pisca e toca um bip baixinho (um pedaço do mesmo arquivo).
    /// </summary>
    public class BombDefuseEnding : MonoBehaviour
    {
        [SerializeField] private BossSpawner bossSpawner;
        [SerializeField] private Transform player;
        [Tooltip("Sprite do LED vermelho da C4 (filho da C4).")]
        [SerializeField] private SpriteRenderer led;

        [Header("Áudio da C4 (bips + explosão)")]
        [SerializeField] private AudioClip bombClip;
        [SerializeField] private float bombVolume = 0.9f;
        [Tooltip("Segundo do arquivo onde começa a tocar (depois da voz).")]
        [SerializeField] private float clipStart = 1.75f;
        [Tooltip("Segundo do arquivo onde a bomba explode.")]
        [SerializeField] private float explosionTime = 10.95f;
        [Tooltip("Segundos do arquivo em que cada bip toca (o LED acende junto).")]
        [SerializeField] private float[] beepTimes =
        {
            1.88f, 2.59f, 3.37f, 4.08f, 4.70f, 5.17f, 5.64f, 6.19f, 6.66f, 7.13f, 7.60f, 7.92f, 8.23f, 8.46f, 8.78f
        };
        [Tooltip("Depois do último bip da lista, o LED pisca neste intervalo até explodir.")]
        [SerializeField] private float fastBlinkInterval = 0.12f;

        [Header("Bip durante a fase")]
        [Tooltip("Segundos entre os bips enquanto a fase acontece.")]
        [SerializeField] private float idleBeepInterval = 1.5f;
        [Tooltip("Volume do bip durante a fase (0 = sem som).")]
        [SerializeField] private float idleBeepVolume = 0.25f;
        [Tooltip("Espera no começo da fase antes do primeiro bip (depois da abertura e do aviso).")]
        [SerializeField] private float idleBeepStartDelay = 7f;
        [Tooltip("Pedaço do arquivo usado como bip: começo e duração.")]
        [SerializeField] private float idleBeepClipStart = 1.86f;
        [SerializeField] private float idleBeepClipLength = 0.16f;

        [Header("Desarme")]
        [Tooltip("Espera depois que o chefão morre, antes de tirar o controle.")]
        [SerializeField] private float delayAfterBoss = 1.5f;
        [Tooltip("Distância (em X) da C4 onde o jogador se ajoelha.")]
        [SerializeField] private float kneelOffsetX = 0.55f;
        [SerializeField] private float walkSpeed = 3f;
        [Tooltip("Quanto o personagem abaixa ao se ajoelhar (1 = não abaixa).")]
        [Range(0.5f, 1f)]
        [SerializeField] private float kneelScaleY = 0.78f;
        [Tooltip("Até onde a barra de desarme chega quando a bomba explode (0 a 1).")]
        [Range(0f, 1f)]
        [SerializeField] private float defuseReached = 0.9f;

        [Header("Frase final")]
        [SerializeField] private string finalText = "O tempo venceu você.";
        [Tooltip("Texto menor que aparece embaixo da frase, uma linha de cada vez.")]
        [TextArea(4, 10)]
        [SerializeField] private string finalSubtext =
            "Você atravessou o fogo e enfrentou os melhores do mundo.\n" +
            "Quando tantos ficaram pelo caminho, você continuou.\n" +
            "Fez do topo a sua casa e das vitórias, uma era.\n" +
            "Mas o último adversário nunca esteve na sua mira.\n" +
            "O tempo levou a última rodada. Não levou a sua história.\n" +
            "O auge passa. O legado fica.";
        [Tooltip("Espera entre a frase e o texto de baixo, e entre cada linha dele.")]
        [SerializeField] private float subtextLineDelay = 1.6f;
        [SerializeField] private float subtextLineFade = 1.2f;
        [Tooltip("Tempo do clarão branco até a tela ficar preta.")]
        [SerializeField] private float flashToBlack = 1.4f;
        [SerializeField] private float blackHold = 2f;
        [SerializeField] private float textFadeDuration = 2.5f;

        private AudioSource bombSource;
        private AudioSource idleSource;
        private bool ending;
        private float idleTimer;

        private void Awake()
        {
            bombSource = CreateSource();
            idleSource = CreateSource();
            if (led != null) led.enabled = false;
        }

        private AudioSource CreateSource()
        {
            AudioSource source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.clip = bombClip;
            return source;
        }

        private void Start()
        {
            if (player == null)
            {
                GameObject found = GameObject.FindGameObjectWithTag("Player");
                if (found != null) player = found.transform;
            }

            if (bossSpawner != null)
            {
                bossSpawner.OnBossDefeated += HandleBossDefeated;
            }
            else
            {
                Debug.LogWarning("[BombDefuseEnding] Boss Spawner não configurado.");
            }
        }

        private void OnDestroy()
        {
            if (bossSpawner != null) bossSpawner.OnBossDefeated -= HandleBossDefeated;
        }

        private void Update()
        {
            if (ending || Time.timeScale <= 0f || Time.timeSinceLevelLoad < idleBeepStartDelay) return;
            float interval = Mathf.Max(0.05f, idleBeepInterval);

            // Bip baixinho + LED enquanto a fase acontece.
            float before = idleTimer;
            idleTimer += Time.deltaTime;
            if (before == 0f || Mathf.Floor(idleTimer / interval) > Mathf.Floor(before / interval))
            {
                PlayIdleBeep();
            }
            if (led != null) led.enabled = idleTimer % interval < 0.12f;
        }

        private void PlayIdleBeep()
        {
            if (bombClip == null || idleBeepVolume <= 0f) return;
            idleSource.Stop();
            idleSource.volume = idleBeepVolume * GameSettings.EffectsVolume;
            idleSource.time = Mathf.Clamp(idleBeepClipStart, 0f, bombClip.length - 0.05f);
            idleSource.Play();
            idleSource.SetScheduledEndTime(AudioSettings.dspTime + idleBeepClipLength);
        }

        private void HandleBossDefeated()
        {
            if (!ending) StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            ending = true;
            idleSource.Stop();
            if (led != null) led.enabled = false;
            yield return new WaitForSeconds(delayAfterBoss);

            if (player == null) yield break;
            Health health = player.GetComponent<Health>();
            if (health != null && health.IsDead) yield break;

            PlayerController controller = player.GetComponent<PlayerController>();
            PlayerShooting shooting = player.GetComponent<PlayerShooting>();
            Rigidbody2D body = player.GetComponent<Rigidbody2D>();
            SpriteRenderer sprite = player.GetComponentInChildren<SpriteRenderer>();

            // A partir daqui o final é uma sequência narrativa, sem dano de inimigos restantes.
            if (health != null) health.IsInvulnerable = true;

            // 1) Tira o controle e a música vai sumindo.
            if (controller != null) controller.enabled = false;
            if (shooting != null) shooting.enabled = false;
            StartCoroutine(FadeOtherAudio(2f));

            // 2) Anda até o lado da C4 (do lado em que ele estiver).
            float bombX = transform.position.x;
            float side = player.position.x <= bombX ? -1f : 1f;
            float targetX = bombX + side * kneelOffsetX;
            float direction = Mathf.Sign(targetX - player.position.x);
            if (sprite != null) sprite.flipX = direction < 0f;
            float timeout = 8f;
            while (timeout > 0f && Mathf.Abs(targetX - player.position.x) > 0.06f
                   && Mathf.Sign(targetX - player.position.x) == direction)
            {
                if (body != null) body.linearVelocity = new Vector2(direction * walkSpeed, body.linearVelocity.y);
                else player.position += new Vector3(direction * walkSpeed * Time.deltaTime, 0f, 0f);
                timeout -= Time.deltaTime;
                yield return null;
            }
            if (body != null) body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            yield return new WaitForSeconds(0.4f);

            // Se ficou preso ou chegou por cima de uma caixa, faz um corte curto até o piso da C4.
            // Assim nunca começa a desarmar do outro lado da arena ou no ar.
            float floorY = transform.position.y;
            RaycastHit2D floor = Physics2D.Raycast(new Vector2(targetX, transform.position.y + 0.2f),
                Vector2.down, 3f, LayerMask.GetMask("Ground"));
            if (floor.collider != null) floorY = floor.point.y;
            float currentFeet = sprite != null ? sprite.bounds.min.y : player.position.y;
            if (Mathf.Abs(player.position.x - targetX) > 0.15f || Mathf.Abs(currentFeet - floorY) > 0.3f)
            {
                Canvas cutCanvas = CreateCanvas();
                Image cut = CreateFullScreen(cutCanvas.transform, Color.clear);
                for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
                {
                    cut.color = new Color(0f, 0f, 0f, t);
                    yield return null;
                }
                cut.color = Color.black;
                if (body != null) body.simulated = false;
                player.position = new Vector3(targetX, player.position.y + floorY - currentFeet, player.position.z);
                if (body != null) body.linearVelocity = Vector2.zero;
                for (float t = 0f; t < 1f; t += Time.deltaTime / 0.25f)
                {
                    cut.color = new Color(0f, 0f, 0f, 1f - t);
                    yield return null;
                }
                Destroy(cutCanvas.gameObject);
            }

            // 3) Vira para a C4 e se ajoelha (abaixa mantendo os pés no chão).
            if (sprite != null) sprite.flipX = side > 0f;
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.simulated = false;
            }
            float feet = sprite != null ? sprite.bounds.min.y : player.position.y;
            Vector3 scale = player.localScale;
            for (float t = 0f; t < 1f; t += Time.deltaTime / 0.3f)
            {
                SetHeight(player, sprite, scale, Mathf.Lerp(1f, kneelScaleY, t), feet);
                yield return null;
            }
            SetHeight(player, sprite, scale, kneelScaleY, feet);

            // 4) Áudio da C4 (sem a voz do começo) + barra de desarme + LED nos bips.
            Canvas canvas = CreateCanvas();
            Image barBack;
            Image barFill = CreateBar(canvas.transform, out barBack);
            float duration = Mathf.Max(0.5f, explosionTime - clipStart);
            if (bombClip != null)
            {
                bombSource.volume = bombVolume * GameSettings.EffectsVolume;
                bombSource.time = Mathf.Clamp(clipStart, 0f, bombClip.length - 0.05f);
                bombSource.Play();
            }
            float lastListed = beepTimes.Length > 0 ? beepTimes[beepTimes.Length - 1] : clipStart;
            for (float t = 0f; t < duration; t += Time.deltaTime)
            {
                float clipTime = clipStart + t;
                barFill.fillAmount = Mathf.Clamp01(t / duration) * defuseReached;
                if (led != null) led.enabled = IsBeeping(clipTime, lastListed);
                yield return null;
            }

            // 5) Explosão: clarão branco que escurece até o preto (o som continua).
            barBack.gameObject.SetActive(false);
            if (led != null) led.enabled = false;
            Image screen = CreateFullScreen(canvas.transform, Color.white);
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, flashToBlack))
            {
                float v = 1f - t;
                screen.color = new Color(v, v, v, 1f);
                yield return null;
            }
            screen.color = Color.black;
            yield return new WaitForSeconds(blackHold);

            // 6) A frase.
            Text text = CreateText(canvas.transform, finalText, 72, new Vector2(0.5f, 0.72f), FontStyle.Bold, Color.white);
            yield return FadeText(text, textFadeDuration);

            // 7) O texto de baixo, uma linha de cada vez.
            string[] lines = finalSubtext.Split('\n');
            Color soft = new Color(0.85f, 0.85f, 0.85f, 1f);
            for (int i = 0; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                yield return new WaitForSeconds(subtextLineDelay);
                bool last = i == lines.Length - 1;
                Vector2 anchor = new Vector2(0.5f, 0.56f - i * 0.065f);
                Text line = CreateText(canvas.transform, lines[i].Trim(), 34, anchor,
                    last ? FontStyle.Bold : FontStyle.Normal, last ? Color.white : soft);
                yield return FadeText(line, subtextLineFade);
            }
            yield return new WaitForSecondsRealtime(4f);
            CreditsOverlay.Show(null, () =>
            {
                Time.timeScale = 1f;
                AudioListener.pause = false;
                SceneManager.LoadScene("MenuInicial");
            }, true);
        }

        private static IEnumerator FadeText(Text text, float duration)
        {
            Color c = text.color;
            for (float t = 0f; t < 1f; t += Time.deltaTime / Mathf.Max(0.05f, duration))
            {
                text.color = new Color(c.r, c.g, c.b, t);
                yield return null;
            }
            text.color = new Color(c.r, c.g, c.b, 1f);
        }

        private bool IsBeeping(float clipTime, float lastListed)
        {
            foreach (float beep in beepTimes)
            {
                if (clipTime >= beep && clipTime < beep + 0.1f) return true;
            }
            if (clipTime > lastListed + 0.1f && fastBlinkInterval > 0f)
            {
                return (clipTime - lastListed) % fastBlinkInterval < fastBlinkInterval * 0.5f;
            }
            return false;
        }

        private static void SetHeight(Transform player, SpriteRenderer sprite, Vector3 baseScale, float factor, float feet)
        {
            player.localScale = new Vector3(baseScale.x, baseScale.y * factor, baseScale.z);
            if (sprite != null)
            {
                float offset = sprite.bounds.min.y - feet;
                player.position -= new Vector3(0f, offset, 0f);
            }
        }

        private IEnumerator FadeOtherAudio(float duration)
        {
            AudioSource[] sources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
            float[] start = new float[sources.Length];
            for (int i = 0; i < sources.Length; i++) start[i] = sources[i] != null ? sources[i].volume : 0f;
            for (float t = 0f; t < 1f; t += Time.deltaTime / duration)
            {
                for (int i = 0; i < sources.Length; i++)
                {
                    if (sources[i] != null && sources[i] != bombSource && sources[i] != idleSource)
                    {
                        sources[i].volume = Mathf.Lerp(start[i], 0f, t);
                    }
                }
                yield return null;
            }
            foreach (AudioSource source in sources)
            {
                if (source != null && source != bombSource && source != idleSource) source.volume = 0f;
            }
        }

        private static Canvas CreateCanvas()
        {
            GameObject canvasObj = new GameObject("TelaFinal");
            Canvas canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;
            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            return canvas;
        }

        private static Image CreateFullScreen(Transform parent, Color color)
        {
            GameObject obj = new GameObject("Tela");
            obj.transform.SetParent(parent, false);
            Image image = obj.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            RectTransform rect = image.rectTransform;
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return image;
        }

        private static Image CreateBar(Transform parent, out Image back)
        {
            GameObject backObj = new GameObject("BarraDesarme");
            backObj.transform.SetParent(parent, false);
            back = backObj.AddComponent<Image>();
            back.color = new Color(0f, 0f, 0f, 0.7f);
            RectTransform rect = back.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.2f);
            rect.sizeDelta = new Vector2(520f, 26f);

            GameObject fillObj = new GameObject("Preenchimento");
            fillObj.transform.SetParent(backObj.transform, false);
            Image fill = fillObj.AddComponent<Image>();
            Texture2D white = Texture2D.whiteTexture;
            fill.sprite = Sprite.Create(white, new Rect(0f, 0f, white.width, white.height), new Vector2(0.5f, 0.5f));
            fill.type = Image.Type.Filled;
            fill.fillMethod = Image.FillMethod.Horizontal;
            fill.fillAmount = 0f;
            fill.color = new Color(0.25f, 0.55f, 1f, 1f);
            RectTransform fillRect = fill.rectTransform;
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = new Vector2(4f, 4f);
            fillRect.offsetMax = new Vector2(-4f, -4f);
            return fill;
        }

        private static Text CreateText(Transform parent, string content, int size, Vector2 anchor, FontStyle style, Color color)
        {
            GameObject obj = new GameObject("Frase");
            obj.transform.SetParent(parent, false);
            Text text = obj.AddComponent<Text>();
            text.text = content;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            text.fontSize = size;
            text.fontStyle = style;
            text.alignment = TextAnchor.MiddleCenter;
            text.horizontalOverflow = HorizontalWrapMode.Overflow;
            text.raycastTarget = false;
            text.color = new Color(color.r, color.g, color.b, 0f);
            RectTransform rect = text.rectTransform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.sizeDelta = new Vector2(1700f, 100f);
            return text;
        }
    }
}
