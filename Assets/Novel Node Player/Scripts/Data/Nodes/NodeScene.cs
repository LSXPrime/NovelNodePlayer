using System;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeScene : Node
    {
        public override string Type { get; } = "Scene";
        public override string Tooltip { get; } = "This node represents switching scenes in the visual novel.";
        public string SceneName = string.Empty;
        public double FadeTime = 1;
    }
}
