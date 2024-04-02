using System.IO;
using UnityEditor;
using UnityEngine;
using NovelNodePlayer.Data;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using NovelNodePlayer.Helpers;
using Cysharp.Threading.Tasks;

namespace NovelNodePlayer.Editor
{
    public class JsonImporter : AssetPostprocessor
    {
        private const string ResourcePath = @"Assets/Novel Node Player/Resources";

        [MenuItem("Tools/Blood Moon/Import Project")]
        public static async UniTask ImportProject()
        {
            string path = EditorUtility.OpenFilePanel("Select Novel Node ProjectData.json file", "", "json");

            if (!string.IsNullOrEmpty(path))
            {
                string directory = Path.GetDirectoryName(path);
                var files = Directory.EnumerateFiles(directory, "*.*", SearchOption.AllDirectories);

                if (Directory.Exists(ResourcePath))
                    Directory.Delete(ResourcePath, true);
                
                Directory.CreateDirectory(ResourcePath);
                foreach (var dirPath in Directory.GetDirectories(directory, "*", SearchOption.AllDirectories))
                    Directory.CreateDirectory(dirPath.Replace(directory, ResourcePath));

                foreach (var file in files)
                {
                    string targetPath = Path.Combine(ResourcePath, file[(directory.Length + 1)..]);
                    using var sourceStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
                    using var destinationStream = new FileStream(targetPath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true);
                    await sourceStream.CopyToAsync(destinationStream);
                }

                AssetDatabase.Refresh();
                Debug.Log("Project files imported successfully.");
            }
            else
            {
                Debug.Log("File selection canceled or invalid.");
            }
        }

        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            if (importedAssets == null || importedAssets.Length == 0)
                return;

            bool isProjectData = false;

            foreach (var assetPath in importedAssets)
            {
                if (!Path.GetExtension(assetPath).Equals(".json", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                try
                {
                    var dataSO = DeserializeJson(assetPath);
                    if (dataSO != null)
                    {
                        if (dataSO is ProjectData)
                            isProjectData = true;
                        CreateScriptableObject(assetPath, dataSO);
                        Debug.Log($"Successfully imported JSON: '{assetPath}' to ScriptableObject.");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error processing JSON file: '{assetPath}'. {e.Message}, Trace: {e.StackTrace}");
                }
            }

            if (isProjectData)
            {
                ProjectData.Instance.Characters = LoadScriptableObjects<CharacterData>($@"{ResourcePath}\Characters");
                ProjectData.Instance.Scenes = LoadScriptableObjects<SceneData>($@"{ResourcePath}\Scenes");
                ProjectData.Instance.Blackboards = LoadScriptableObjects<BlackboardData>($@"{ResourcePath}\Blackboards");

                EditorUtility.SetDirty(ProjectData.Instance);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static ScriptableObject DeserializeJson(string assetPath)
        {
            string json = File.ReadAllText(assetPath);
            switch (assetPath)
            {
                case string dir when dir.Contains("ProjectData"):
                    var projectData = ScriptableObject.CreateInstance<ProjectData>();
                    JsonConvert.PopulateObject(json, projectData);
                    return projectData;
                case string dir when dir.Contains("Characters"):
                    return DeserializeCharacterData(json);
                case string dir when dir.Contains("Scenes"):
                    return DeserializeSceneData(json);
                case string dir when dir.Contains("Blackboards"):
                    var blackboardData = ScriptableObject.CreateInstance<BlackboardData>();
                    JsonConvert.PopulateObject(json, blackboardData);
                    return blackboardData;
                default:
                    Debug.LogError($"Unsupported JSON asset type: '{assetPath}'");
                    return null;
            }
        }

        static ScriptableObject DeserializeCharacterData(string json)
        {
            var data = ScriptableObject.CreateInstance<CharacterData>();
            JsonConvert.PopulateObject(json, data);
            foreach (var sprite in data.Sprites)
            {
                sprite.Value.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ResourcePath}/{sprite.Value.String}");
            }
            foreach (var audio in data.Sounds)
            {
                audio.Audio = AssetDatabase.LoadAssetAtPath<AudioClip>($"{ResourcePath}/{audio.Path}");
            }
            return data;
        }

        static ScriptableObject DeserializeSceneData(string json)
        {
            var data = ScriptableObject.CreateInstance<SceneData>();
            JsonConvert.PopulateObject(json, data);

            foreach (var sprite in data.Backgrounds)
            {
                sprite.Value.Sprite = AssetDatabase.LoadAssetAtPath<Sprite>($"{ResourcePath}/{sprite.Value.String}");
            }
            foreach (var audio in data.Sounds)
            {
                audio.Audio = AssetDatabase.LoadAssetAtPath<AudioClip>($"{ResourcePath}/{audio.Path}");
            }
            return data;
        }

        static void CreateScriptableObject(string assetPath, ScriptableObject dataSO)
        {
            string scriptableObjectPath = Path.ChangeExtension(assetPath, ".asset");

            if (dataSO is SceneData data)
                for (int i = 0; i < data.Nodes.Count; i++)
                {
                    Node node = data.Nodes[i];
                    var nodePath = scriptableObjectPath.Replace("Scene.asset", $"Nodes");
                    Directory.CreateDirectory(nodePath);
                    var nodeSO = ScriptableObject.CreateInstance(node.GetType());
                    nodeSO.CopyFrom(node);
                    AssetDatabase.CreateAsset(nodeSO, $"{nodePath}/{node.Guid}_{node.Type}.asset");
                    EditorUtility.SetDirty(nodeSO);
                    data.Nodes[i] = (Node)nodeSO;
                }

            AssetDatabase.CreateAsset(dataSO, scriptableObjectPath);
            AssetDatabase.DeleteAsset(assetPath);
            EditorUtility.SetDirty(dataSO);
        }

        static List<T> LoadScriptableObjects<T>(string directoryPath) where T : ScriptableObject
        {
            string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { directoryPath });


            List<T> scriptableObjects = new();

            foreach (string guid in guids)
            {
                string assetPath = AssetDatabase.GUIDToAssetPath(guid);
                T scriptableObject = AssetDatabase.LoadAssetAtPath<T>(assetPath);
                if (scriptableObject != null)
                    scriptableObjects.Add(scriptableObject);
            }

            return scriptableObjects;
        }
    }
}
