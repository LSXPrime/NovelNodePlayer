using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace NovelNodePlayer.Data
{
    public class SaveData
    {
        public int ID { get; set; }
        public string Scene { get; set; }
        public string Background { get; set; }
        public string Dialogue { get; set; }
        public string Narrator { get; set; }
        public string Timestamp { get; set; }
        public List<PlayerViewCharacterData> Characters { get; set; } = new();
        public List<NodeChoiceData> Choices { get; set; } = new();
        public List<BlackboardData> Blackboards { get; set; } = new();
        [JsonIgnore]
        public string Thumbnail => Path.Combine(AppConfig.DataPath, $"Checkpoints\\checkpoint_{ID}.png");
    }
}