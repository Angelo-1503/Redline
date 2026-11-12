using Redline.Core;
using Redline.Menu;
using Redline.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Redline.UI
{
    /// <summary>Pausa instalada automaticamente em cada fase, inclusive ao abrir a cena diretamente.</summary>
    public sealed class PauseMenu : MonoBehaviour
    {
        static int resumeFrame = -1;
        public static bool BlocksGameplayInput => Time.timeScale <= 0f || Time.frameCount == resumeFrame;
        PlayerController player;
        Health health;
        MainMenuController menu;
        bool paused;
        float previousScale;
        bool previousAudioPause;
        CursorLockMode previousLock;
        bool previousCursor;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Register()
        {
            resumeFrame = -1;
            SceneManager.sceneLoaded -= SceneLoaded;
            SceneManager.sceneLoaded += SceneLoaded;
        }
        static void SceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (mode != LoadSceneMode.Single || !scene.name.StartsWith("Fase")) return;
            new GameObject("MenuDePausa", typeof(PauseMenu));
        }
        void Update()
        {
            if (paused || BlocksGameplayInput || !Input.GetKeyDown(KeyCode.Escape)) return;
            if (player == null)
            {
                var obj = GameObject.FindGameObjectWithTag("Player");
                if (obj != null) { player = obj.GetComponent<PlayerController>(); health = obj.GetComponent<Health>(); }
            }
            // Aberturas, passagens, vitória e epílogo desabilitam o controle ou protegem o jogador.
            if (player == null || !player.isActiveAndEnabled || health == null || health.IsDead || health.IsInvulnerable) return;
            previousScale = Time.timeScale; previousAudioPause = AudioListener.pause;
            previousLock = Cursor.lockState; previousCursor = Cursor.visible;
            paused = true; Time.timeScale = 0f; AudioListener.pause = true;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            menu = new GameObject("InterfacePausa").AddComponent<MainMenuController>();
            menu.InitializePause(Resume, Leave);
        }
        void Restore()
        {
            Time.timeScale = previousScale; AudioListener.pause = previousAudioPause;
            Cursor.lockState = previousLock; Cursor.visible = previousCursor;
            resumeFrame = Time.frameCount;
        }
        void Resume()
        {
            if (!paused) return;
            menu.DisposePause(); Restore(); paused = false;
        }
        void Leave()
        {
            if (!Application.CanStreamedLevelBeLoaded("MenuInicial")) return;
            menu.DisposePause(); Restore(); paused = false;
            Time.timeScale = 1f; AudioListener.pause = false;
            Cursor.lockState = CursorLockMode.None; Cursor.visible = true;
            SceneManager.LoadScene("MenuInicial");
        }
        void OnDestroy()
        {
            if (!paused) return;
            if (menu != null) menu.DisposePause();
            Restore();
        }
    }
}
