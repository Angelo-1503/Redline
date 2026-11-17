using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Redline.EditorTools
{
    public static class WindowsBuild
    {
        [MenuItem("Redline/Gerar executável Windows (64 bits)")]
        public static void BuildWindows64()
        {
            string[] scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray();
            string[] expected = new[] { "MenuInicial", "Fase01", "Fase02", "Fase03", "Fase04", "Fase05", "Fase06" };
            if (!scenes.Select(Path.GetFileNameWithoutExtension).SequenceEqual(expected) || scenes.Any(scene => !File.Exists(scene)))
                throw new BuildFailedException("Confira a lista de cenas: MenuInicial e Fase01 a Fase06, nessa ordem.");

            string output = Path.GetFullPath("Builds/Windows");
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i + 1 < args.Length; i++)
                if (args[i] == "-redlineBuildPath") output = Path.GetFullPath(args[i + 1]);
            Directory.CreateDirectory(output);
            var target = NamedBuildTarget.Standalone;
            var previousBackend = PlayerSettings.GetScriptingBackend(target);
            try
            {
                PlayerSettings.SetScriptingBackend(target, ScriptingImplementation.Mono2x);
                BuildReport report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
                {
                    scenes = scenes,
                    locationPathName = Path.Combine(output, "Redline.exe"),
                    target = BuildTarget.StandaloneWindows64,
                    options = BuildOptions.None
                });
                if (report.summary.result != BuildResult.Succeeded)
                    throw new BuildFailedException("Build Windows falhou: " + report.summary.result + "; erros: " + report.summary.totalErrors);
                File.Copy("Assets/Audio/Music/MusicLicenses.txt", Path.Combine(output, "MusicLicenses.txt"), true);
                File.WriteAllText(Path.Combine(output, "LEIA-ME.txt"),
                    "REDLINE — Windows 64 bits\n\n" +
                    "Para jogar, abra Redline.exe. Mantenha todos os arquivos e pastas desta distribuição juntos.\n" +
                    "Para compartilhar, compacte a pasta inteira; enviar apenas o .exe não funciona.\n\n" +
                    "Vídeo: use F11, Alt+Enter ou Configurações → Tela cheia para alternar. Em modo janela, você pode redimensionar ou maximizar.\n\n" +
                    "Teclado e mouse: A/D ou setas movem; W/S orientam a mira; Espaço pula; mouse esquerdo ou Ctrl esquerdo atiram; C desce de plataformas; Esc pausa.\n" +
                    "Xbox: analógico esquerdo/direcional movem; direito mira; A pula; RT ou X atiram; B desce; Menu/Start pausa. Nos menus, A confirma e B volta.\n" +
                    "Os atalhos de teclado e os volumes geral, de música e de efeitos ficam em Configurações.\n\n" +
                    "Créditos das músicas e licenças: MusicLicenses.txt e créditos dentro do jogo.\n");
                Debug.Log("[Redline] Build Windows pronta: " + output + "; tamanho: " + report.summary.totalSize + " bytes; avisos: " + report.summary.totalWarnings);
            }
            finally
            {
                PlayerSettings.SetScriptingBackend(target, previousBackend);
            }
        }
    }
}
