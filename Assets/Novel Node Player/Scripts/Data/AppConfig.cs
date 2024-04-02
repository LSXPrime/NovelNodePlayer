using System;
using System.IO;
using UnityEngine;
using Newtonsoft.Json;

namespace NovelNodePlayer.Data
{
    public class AppConfig
    {
        #region Game
        public float TextSpeed = 1f;
        public float MusicVolume = 1f;
        public float SoundFXVolume = 1f;
        public float VoiceVolume = 1f;

        public FullScreenMode WindowState = FullScreenMode.FullScreenWindow;

        public string PlayerName = "Player";

        #endregion
        #region Paths
        public static string DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Blood Moon", "Novel Node Player");
        #endregion

        // Load settings from a JSON file
        public static void LoadSelf()
        {
            if (File.Exists($"{DataPath}\\Config.json"))
            {
                var json = File.ReadAllText($"{DataPath}\\Config.json");
                AppConfig self = JsonConvert.DeserializeObject<AppConfig>(json);
                instance = self;
            }
        }

        private void CheckPaths()
        {
            Directory.CreateDirectory(DataPath);
        }

        // Save settings to a JSON file
        public void Save()
        {
            var json = JsonConvert.SerializeObject(this, Formatting.Indented);
            File.WriteAllText($"{DataPath}\\Config.json", json);
            CheckPaths();
            ConfigSavedEvent?.Invoke();
        }

        public void Reset()
        {
            if (File.Exists($"{DataPath}\\Config.json"))
                File.Delete($"{DataPath}\\Config.json");

            DataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Blood Moon", "Novel Node Player");
            TextSpeed = MusicVolume = SoundFXVolume = VoiceVolume = 1f;
            WindowState = FullScreenMode.FullScreenWindow;
            Save();
        }

        public delegate void ConfigSavedDelegate();
        public event ConfigSavedDelegate ConfigSavedEvent;

        private static AppConfig instance;
        public static AppConfig Instance => instance ??= new AppConfig();
    }
}
