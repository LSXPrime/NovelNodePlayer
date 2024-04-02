using System.Collections.Generic;

namespace NovelNodePlayer.Data
{
    public class ProjectData : SingletonScriptableObject<ProjectData>
    {
        public string Name = string.Empty;
        public string Author = string.Empty;
        public string WordsCount = string.Empty;

        public List<CharacterData> Characters = new();
        public List<SceneData> Scenes = new();
        public List<BlackboardData> Blackboards = new();

        #region Statics
        public static ProjectData Current => Instance; // Kept for compatibility
        #endregion
    }
}
