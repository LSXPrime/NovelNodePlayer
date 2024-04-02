using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NovelNodePlayer.Data;
using UnityEngine;

namespace NovelNodePlayer.Helpers
{
    public class NodeJsonConverter : JsonConverter<Node>
    {
        public override Node ReadJson(JsonReader reader, Type objectType, Node existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            JObject jo = JObject.Load(reader);
            string type = (string)jo["Type"];
            Debug.Log($"Node type: {type}");
            return type switch
            {
                "Background" => jo.ToObject<NodeBackground>(serializer),
                "Character" => jo.ToObject<NodeCharacter>(serializer),
                "Checkpoint" => jo.ToObject<NodeCheckpoint>(serializer),
                "Choice" => jo.ToObject<NodeChoice>(serializer),
                "Condition" => jo.ToObject<NodeCondition>(serializer),
                "Dialogue" => jo.ToObject<NodeDialogue>(serializer),
                "Event" => jo.ToObject<NodeEvent>(serializer),
                "Scene" => jo.ToObject<NodeScene>(serializer),
                _ => jo.ToObject<Node>(serializer),
            };
        }

        public override void WriteJson(JsonWriter writer, Node value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}
