using System;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeChoiceData
    {
        public string Text = string.Empty;
        public Guid Next;
        public int Output;
    }
}
