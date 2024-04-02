using System;
using NovelNodePlayer.Enums;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeState : Node
    {
        public override string Type { get; } = "State";
        public override string Tooltip { get; } = "This node should represent a scene's state. \nwhether is the scene's state point is enter or exit.";
        public NodeSwitch State = NodeSwitch.Enter;
    }
}
