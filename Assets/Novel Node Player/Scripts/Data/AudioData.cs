using System;
using Newtonsoft.Json;
using UnityEngine;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class AudioData
    {
        public string Name = string.Empty;
        public string Path = string.Empty;
        [JsonIgnore]
        public AudioClip Audio = null;
        [JsonIgnore]
        public AssetPlayState State = AssetPlayState.Stopped;
    }
}