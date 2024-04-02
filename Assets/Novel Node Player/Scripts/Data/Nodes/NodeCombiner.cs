using System;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeCombiner : Node
    {
        public override string Type { get; } = "Combiner";
        public override string Tooltip { get; } = "This node should represent a nodes combiner. \nwhere you can combine multiply paths nodes into one.";
    }
}
