using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using Redline.Menu;
using Redline.Audio;

namespace Redline.EditorTools
{
    public static class MainMenuSceneBuilder
    {
        public const string MenuPath = "Assets/Scenes/MenuInicial.unity";
        [MenuItem("Redline/Recriar cena do menu inicial")]
        public static void Build()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            const string artPath = "Assets/Art/Menu/RedlineMenuBackground.png";
            AssetDatabase.ImportAsset(artPath, ImportAssetOptions.ForceSynchronousImport);
            var importer = (TextureImporter)AssetImporter.GetAtPath(artPath);
            importer.textureType = TextureImporterType.Sprite; importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false; importer.alphaIsTransparency = false;
            importer.filterMode = FilterMode.Bilinear; importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Uncompressed; importer.SaveAndReimport();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var cameraObject = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
            cameraObject.tag = "MainCamera"; cameraObject.transform.position = new Vector3(0, 0, -10);
            var camera = cameraObject.GetComponent<Camera>(); camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = Color.black;
            var music = new SerializedObject(cameraObject.AddComponent<MusicManager>());
            music.FindProperty("musicClip").objectReferenceValue = AssetDatabase.LoadAssetAtPath<AudioClip>("Assets/Audio/Music/Decisions.mp3");
            music.FindProperty("volume").floatValue = 0.25f;
            music.ApplyModifiedPropertiesWithoutUndo();
            var menu = new GameObject("RedlineMenu", typeof(MainMenuController));
            var data = new SerializedObject(menu.GetComponent<MainMenuController>());
            data.FindProperty("backgroundArt").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Sprite>(artPath);
            data.FindProperty("menuFont").objectReferenceValue = AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
            data.ApplyModifiedPropertiesWithoutUndo();
            EditorSceneManager.SaveScene(scene, MenuPath);
            var scenes = EditorBuildSettings.scenes.Where(s => s.path != MenuPath).ToList();
            scenes.Insert(0, new EditorBuildSettingsScene(MenuPath, true)); EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            Debug.Log("[Redline] Menu inicial criado em " + MenuPath);
        }
    }
}
