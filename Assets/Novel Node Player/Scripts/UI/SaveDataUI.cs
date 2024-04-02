using System.IO;
using NovelNodePlayer.Data;
using UnityEngine;
using UnityEngine.UI;

namespace NovelNodePlayer
{
    public class SaveDataUI : MonoBehaviour
    {
        public Image Preview;
        public Text Scene;
        public Text Timestamp;
        public SaveData SaveData { get; set; }

        public void Set(SaveData saveData)
        {
            SaveData = saveData;
            Scene.text = saveData.Scene;
            Timestamp.text = saveData.Timestamp;
            var texture = new Texture2D(2, 2);
            texture.LoadImage(File.ReadAllBytes(saveData.Thumbnail));
            Preview.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.one * 0.5f);
            gameObject.SetActive(true);
        }
    }
}
