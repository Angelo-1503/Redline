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
    /// - Nunca recarrega durante o Play.
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
                || EditorApplication.isUpdating || SceneManager.sceneCount != 1)
            {
                return;
            }

            Scene scene = SceneManager.GetActiveScene();
            string path = scene.path;
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
            {
                return;
            }

            string stamp = Stamp(path);
            if (SessionState.GetString(KeyPrefix + path, "") == stamp)
            {
                return;
            }
            SessionState.SetString(KeyPrefix + path, stamp);

            if (scene.isDirty)
            {
                bool reload = EditorUtility.DisplayDialog(
                    "Redline - cena alterada fora do Unity",
                    "O arquivo " + Path.GetFileName(path) + " mudou no disco, mas a cena aberta tem "
                    + "alterações não salvas.\n\nRecarregar do disco? As alterações não salvas feitas aqui serão perdidas.",
                    "Recarregar do disco", "Manter a cena aberta");
                if (!reload)
                {
                    return;
                }
            }

            EditorSceneManager.OpenScene(path, OpenSceneMode.Single);
            Debug.Log("[Redline] Cena recarregada do disco: " + path);
        }
    }
}
