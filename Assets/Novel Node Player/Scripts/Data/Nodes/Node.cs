using Newtonsoft.Json;
using JsonSubTypes;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace NovelNodePlayer.Data
{
    [Serializable]
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(NodeState), "State")]
    [JsonSubtypes.KnownSubType(typeof(NodeBackground), "Background")]
    [JsonSubtypes.KnownSubType(typeof(NodeCharacter), "Character")]
    [JsonSubtypes.KnownSubType(typeof(NodeDialogue), "Dialogue")]
    [JsonSubtypes.KnownSubType(typeof(NodeChoice), "Choice")]
    [JsonSubtypes.KnownSubType(typeof(NodeEvent), "Event")]
    [JsonSubtypes.KnownSubType(typeof(NodeCondition), "Condition")]
    [JsonSubtypes.KnownSubType(typeof(NodeScene), "Scene")]
    [JsonSubtypes.KnownSubType(typeof(NodeCheckpoint), "Checkpoint")]
    [JsonSubtypes.KnownSubType(typeof(NodeCombiner), "Combiner")]
    public class Node : ScriptableObject
    {
        public virtual string Type { get; } = "Base";
        public virtual string Tooltip { get; } = "Basic purpose node, can be used as dummy or for building another nodes.";
        public string Guid = string.Empty;
        public string Previous;
        public string Next;
        public List<NodeConnector> Input;
        public List<NodeConnector> Output;
    }
}
