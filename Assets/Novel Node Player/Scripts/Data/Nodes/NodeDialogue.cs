using Newtonsoft.Json;
using System.Linq;
using System.Collections.Generic;
using System;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeDialogue : Node
    {
        public override string Type { get; } = "Dialogue";
        public override string Tooltip { get; } = "This node should represent a character's dialogue. \nincluding fields for the character speaking, the actual dialogue text and emotion.";
        public string Character = string.Empty;
        private CharacterData _characterData;
        [JsonIgnore]
        public CharacterData CharacterData { get => _characterData ??= ProjectData.Current.Characters.FirstOrDefault(x => x.Name == Character); }
        public bool IsPlayerDialogue { get; }
        public List<NodeDialogData> Lines = new();

        [Serializable]
        public class NodeDialogData
        {
            public string Text = string.Empty;
            public string CharacterSprite = string.Empty;
            public string CharacterAudio = string.Empty;
        }
    }
}
