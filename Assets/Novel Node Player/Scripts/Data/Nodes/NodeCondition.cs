using System;
using NovelNodePlayer.Enums;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeCondition : Node
    {
        public override string Type { get; } = "Condition";
        public override string Tooltip { get; } = "This node represents a condition based on Blackboard data.";
        public string BlackboardName = string.Empty;
        public string BlackboardKey = string.Empty;

        public Enums.ValueType BlackboardType;
        public ComparisonOperator Operator;
        public string CompareValue = string.Empty;
    }
}
