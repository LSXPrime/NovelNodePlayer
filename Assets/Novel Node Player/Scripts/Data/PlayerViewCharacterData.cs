using System;
using System.Linq;
using Newtonsoft.Json;
using UnityEngine;

namespace NovelNodePlayer.Data
{
    [Serializable]
    public class PlayerViewCharacterData
    {
        public string CharacterName { get; set; }
        public string SpriteName { get; set; }
        [JsonIgnore]
        public Sprite Sprite { get; set; }
        private CharacterData _characterData { get; set; }
        [JsonIgnore]
        public CharacterData CharacterData
        {
            get => _characterData = _characterData != null ? _characterData : ProjectData.Current.Characters.First(x => x.Name == CharacterName);
            set => _characterData = value;
        }
        public SerializableRect Margin { get; set; } // TODO: Margin isn't imported on JSON deserialize or loading saved game
    }

    [Serializable]
    public class SerializableRect
    {
        public float x;
        public float y;
        public float width;
        public float height;


        public SerializableRect(Rect MyRect)
        {
            x = MyRect.x;
            y = MyRect.y;
            width = MyRect.width;
            height = MyRect.height;

        }
        public override string ToString()
        {
            return String.Format("[{0}, {1}, {2}, {3}]", x, y, width, height);
        }


        /// Automatic conversion from SerializableRect to Rect
        public static implicit operator Rect(SerializableRect vRect)
        {
            return new Rect(vRect.x, vRect.y, vRect.width, vRect.height);
        }


        /// Automatic conversion from Rect to SerializableRect
        public static implicit operator SerializableRect(Rect vRect)
        {
            return new SerializableRect(vRect);
        }
    }
}