using System;
using NovelNodePlayer.Enums;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeBackground : Node
    {
        public override string Type { get; } = "Background";
        public override string Tooltip { get; } = "This node should represent a scene's background. \nincluding fields for the state (enter or exit), the fade color and fade time.";
        public AssetType AssetType = AssetType.Sprite;
        public string Background = string.Empty;
        public double FadeTime = 1;
        public bool Loop = false;
        public NodeSwitch State = NodeSwitch.Enter;
    }
}
