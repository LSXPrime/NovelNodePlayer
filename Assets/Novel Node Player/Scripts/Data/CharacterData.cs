using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace NovelNodePlayer.Data
{
    public class CharacterData : ScriptableObject
    {
        public string Name = string.Empty;
        public string Avatar = string.Empty;
        public List<KeyValue> Sprites = new();
        public List<AudioData> Sounds = new();
        public List<string> Tags = new();

        public static CharacterData FromJson(string json) => JsonConvert.DeserializeObject<CharacterData>(json);
        public string ToJson() => JsonConvert.SerializeObject(this, Formatting.Indented);
    }

    public enum AssetPlayState : short
    {
        Stopped = 0,
        Playing = 1,
        Paused = 2
    }
}