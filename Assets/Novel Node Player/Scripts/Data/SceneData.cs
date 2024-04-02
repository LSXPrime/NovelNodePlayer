using System.Collections.Generic;
using UnityEngine;

namespace NovelNodePlayer.Data
{
    public class SceneData : ScriptableObject
    {
        public string Name = string.Empty;
        public int ID = 0;
        public List<Node> Nodes = new();
        public List<NodeConnection> Connections = new();
        public List<KeyValue> Backgrounds = new();
        public List<AudioData> Sounds = new();
    }
}
