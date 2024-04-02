using System;
using NovelNodePlayer.Enums;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class NodeConnector
    {
        public string ID = string.Empty;
        public string Title = string.Empty;
        public bool IsConnected;
        public NodeConnectorFlow Flow = NodeConnectorFlow.Input;
    }
}
