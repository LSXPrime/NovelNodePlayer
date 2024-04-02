using Newtonsoft.Json;
using System.Linq;
using NovelNodePlayer.Enums;
using System;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeCharacter : Node
    {
        public override string Type { get; } = "Character";
        public override string Tooltip { get; } = "This node should represent a character on-screen. \nincluding fields for the state (enter or exit), the current sprite, position on screen view by percent and fade time.";
        public string Character = string.Empty;
        private CharacterData _characterData;
        [JsonIgnore]
        public CharacterData CharacterData { get => _characterData = _characterData != null ? _characterData : ProjectData.Current.Characters.FirstOrDefault(x => x.Name == Character); }

        public string Sprite = string.Empty;
        public int Position = 50;
        public double FadeTime = 0.25;
        public NodeSwitch State = NodeSwitch.Enter;
    }
}
