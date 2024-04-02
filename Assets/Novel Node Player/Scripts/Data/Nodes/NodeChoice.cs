using System;
using System.Collections.Generic;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeChoice : Node
    {
        public override string Type { get; } = "Choice";
        public override string Tooltip { get; } = "Node that represent choices the player can make, for branching dialogues. \nInclude fields for the text of the choice and the next dialogue node linked to each choice output.";
        public List<NodeChoiceData> Choices = new();
    }
}
