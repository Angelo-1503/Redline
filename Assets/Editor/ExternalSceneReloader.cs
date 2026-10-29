using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Redline.EditorTools
{
    /// <summary>
    /// Recarrega a cena aberta quando o arquivo dela muda FORA do Unity
    /// (por exemplo, quando o mapa é editado direto no arquivo .unity).
    ///
    /// Sem isto o Unity pode continuar com a versão antiga da cena na memória,
    /// e o jogo roda com objetos que já foram apagados do arquivo.
    ///
    /// - Cena sem mudanças não salvas: recarrega sozinho e avisa no Console.
    /// - Cena com mudanças não salvas: pergunta antes.
    /// - Nunca recarrega durante o Play; ao apertar Play, recarrega antes se o arquivo mudou.
    /// Só existe no editor; não entra no jogo compilado.
    /// </summary>
    [InitializeOnLoad]
    public static class ExternalSceneReloader
    {
        private const string KeyPrefix = "Redline.ExternalSceneReloader.v2.";
        private static double nextCheck;

        static ExternalSceneReloader()
        {
            // Só o "salvar" do próprio Unity atualiza o registro. NÃO usar sceneOpened: ao sair do
            // Play o Unity restaura a cena antiga da memória e dispara esse evento, o que fazia o
            // script achar que a cena já estava atualizada.
            EditorSceneManager.sceneSaved += scene => Remember(scene.path);
            EditorApplication.update += Tick;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        /// <summary>
        /// Ao apertar Play, confere a cena antes de começar. Se o arquivo mudou (por exemplo,
        /// enquanto o jogo ainda rodava, quando o Tick não pode recarregar), cancela o Play,
        /// recarrega do disco e aperta Play de novo — senão o jogo roda com a cena antiga.
        /// </summary>
        private static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state != PlayModeStateChange.ExitingEditMode || !HasChangedOnDisk())
            {
                return;
            }

            EditorApplication.isPlaying = false;
            EditorApplication.delayCall += () =>
            {
                if (ReloadIfChanged())
                {
                    EditorApplication.delayCall += () => EditorApplication.isPlaying = true;
                }
            };
        }

        private static string Stamp(string path)
        {
            return File.GetLastWriteTimeUtc(path).Ticks.ToString();
        }

        private static void Remember(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                SessionState.SetString(KeyPrefix + path, Stamp(path));
            }
        }

        private static void Tick()
        {
            if (EditorApplication.timeSinceStartup < nextCheck)
            {
                return;
            }
            nextCheck = EditorApplication.timeSinceStartup + 1.0;

            if (EditorApplication.isPlayingOrWillChangePlaymode || EditorApplication.isCompiling
                || EditorApplication.isUpdating)
            {
                return;
            }

            ReloadIfChanged();
        }

        private static bool HasChangedOnDisk()
        {
            if (SceneManager.sceneCount != 1)
            {
                return false;
            }

            string path = SceneManager.GetActiveScene().path;
            return !string.IsNullOrEmpty(path) && File.Exists(path)
                   && SessionState.GetString(KeyPrefix + path, "") != Stamp(path);
        }

        /// <summary>Recarrega a cena do disco se o arquivo mudou. Devolve true se recarregou.</summary>
        private static bool ReloadIfChanged()
        {
            if (!HasChangedOnDisk())
            {
                return false;
            }

            Scene scene = SceneManager.GetActiveScene();
            string path = scene.path;
            SessionState.SetString(KeyPrefix + path, Stamp(path));

            if (scene.isDirty)
            {
                bool reload = EditorUtility.DisplayDialog(
                    "Redline - cena alterada fora do Unity",
                    "O arquivo " + Path.GetFileName(path) + " mudou no disco, mas a cena aberta tem "
                    + "alterações não salvas.\n\nRecarregar do disco? As alterações não salvas feitas aqui serão perdidas.",
                    "Recarregar do disco", "Manter a cena aberta");
                if (!reload)
                {
                    return false;
                }
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Debug.Log("[Redline] Cena recarregada do disco: " + path);
            return true;
        }
    }
}
