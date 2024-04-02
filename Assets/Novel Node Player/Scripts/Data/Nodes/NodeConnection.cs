using Newtonsoft.Json;
using NetFabric.Hyperlinq;
using System.Linq;
using System;
using NovelNodePlayer.Core;
using UnityEngine;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeConnection
    {
        public string SourceID;
        private NodeConnector _source;
        [JsonIgnore]
        public NodeConnector Source { get => _source ??= GameManager.Instance.SceneSelected.Nodes.SelectMany(node => node.Output).FirstOrDefault(connector => connector.ID == SourceID);}

        public string TargetID;
        private NodeConnector _target;
        [JsonIgnore]
        public NodeConnector Target { get => _target ??= GameManager.Instance.SceneSelected.Nodes.SelectMany(node => node.Input).FirstOrDefault(connector => connector.ID == TargetID); }

        public NodeConnection(NodeConnector source, NodeConnector target)
        {
            if (source == null || target == null)
                return;

            SourceID = source.ID;
            TargetID = target.ID;
            _source = source;
            _target = target;
            Source.IsConnected = Target.IsConnected = true;
        }
    }
}
