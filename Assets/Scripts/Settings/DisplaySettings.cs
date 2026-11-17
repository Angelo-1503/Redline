using UnityEngine;

namespace Redline.Settings
{
    /// <summary>Modo de vídeo persistido, independente das preferências antigas do player Unity.</summary>
    public sealed class DisplaySettings : MonoBehaviour
    {
        const string FullscreenKey = "Redline.Display.Fullscreen";
        public static bool IsFullscreen => PlayerPrefs.GetInt(FullscreenKey, 1) != 0;
        static int windowWidth = 1280, windowHeight = 720;
        FullScreenMode reportedMode = (FullScreenMode)(-1);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void Initialize()
        {
            if (Application.isEditor) return;
            var root = new GameObject("ConfiguracaoDeVideo", typeof(DisplaySettings));
            DontDestroyOnLoad(root);
            Apply(IsFullscreen);
        }

        void Update()
        {
            if (Screen.fullScreenMode != reportedMode)
            {
                reportedMode = Screen.fullScreenMode;
                Debug.Log("[Redline] Vídeo: " + reportedMode + " " + Screen.width + "x" + Screen.height);
            }
            if (Input.GetKeyDown(KeyCode.F11)
                || (Input.GetKeyDown(KeyCode.Return) && (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))))
                ToggleFullscreen();
        }

        public static void ToggleFullscreen() { SetFullscreen(!IsFullscreen); }
        public static void SetFullscreen(bool fullscreen)
        {
            PlayerPrefs.SetInt(FullscreenKey, fullscreen ? 1 : 0);
            PlayerPrefs.Save();
            Apply(fullscreen);
        }

        static void Apply(bool fullscreen)
        {
            if (Application.isEditor) return;
            if (fullscreen && !Screen.fullScreen)
            {
                windowWidth = Screen.width;
                windowHeight = Screen.height;
            }
            Resolution desktop = Screen.currentResolution;
            int width = fullscreen ? desktop.width : Mathf.Clamp(windowWidth, 640, Mathf.Max(640, desktop.width - 80));
            int height = fullscreen ? desktop.height : Mathf.Clamp(windowHeight, 360, Mathf.Max(360, desktop.height - 80));
            Screen.SetResolution(width, height, fullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed);
        }
    }
}
