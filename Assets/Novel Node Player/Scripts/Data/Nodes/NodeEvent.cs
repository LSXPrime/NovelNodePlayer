using System;
using NovelNodePlayer.Enums;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeEvent : Node
    {
        public override string Type { get; } = "Event";

        public override string Tooltip { get; } = "Node that represent events or triggers in the story to check using Condition Node. \nThis could include changes in the game state, unlocking new dialogues, or triggering special scenes.";

        public string BlackboardName = string.Empty;
        public string BlackboardKey = string.Empty;

        public Enums.ValueType BlackboardType;

        public EventTask Operator;
        public string NewValue = string.Empty;
    }
}
