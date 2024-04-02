using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NetFabric.Hyperlinq;
using Newtonsoft.Json;
using NovelNodePlayer.Core;
using NovelNodePlayer.Enums;

namespace NovelNodePlayer.Data
{
    public class GameData
    {
        public double LoadingProgress = 0;

        public List<SaveData> SaveGames = new();
        public bool IsSaved => SaveGames.Count > 0;

        #region Methods
        public GameData()
        {
            AppConfig.LoadSelf();
            var savesPath = $"{AppConfig.DataPath}\\Checkpoints";
            if (!Directory.Exists(savesPath))
                Directory.CreateDirectory(savesPath);

            var saves = Directory.EnumerateFiles(savesPath)
                .AsValueEnumerable()
                .Where(x => x.EndsWith(".json"));

            foreach (var savePath in saves)
            {
                var save = File.ReadAllText(savePath);
                var saveData = JsonConvert.DeserializeObject<SaveData>(save);
                SaveGames.Add(saveData);
            }
        }

        public void Play()
        {
            GameManager.Instance.SceneSelected = ProjectData.Current.Scenes.First(x => x.ID == 0);
            GameManager.Instance.Play();
        }

        public void Continue()
        {
            SaveData latestSave = null;
            DateTime latestTimestamp = DateTime.MinValue;
            foreach (var save in SaveGames)
            {
                DateTime timestamp = DateTime.Parse(save.Timestamp);
                if (timestamp > latestTimestamp)
                {
                    latestTimestamp = timestamp;
                    latestSave = save;
                }
            }

            GameManager.Instance.LoadCheckpoint(latestSave);
        }

        public void Load(SaveData saveData)
        {
            GameManager.Instance.LoadCheckpoint(saveData);
        }

        #endregion

        #region Statics
        private static GameData instance;
        public static GameData Instance => instance ??= new GameData();
        #endregion
    }
}
