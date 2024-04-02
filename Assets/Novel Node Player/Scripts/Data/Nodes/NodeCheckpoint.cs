using System;
using NovelNodePlayer.Enums;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeCheckpoint : Node
    {
        public override string Type { get; } = "Checkpoint";
        public override string Tooltip { get; } = "Node that represents saving progress or resetting to a previous checkpoint.";
        public CheckpointAction Action;
        public int CheckpointID;
        public int TargetCheckpointID;
    }
}
