using UnityEngine;
using UnityEngine.UI;
using NovelNodePlayer.Data;
using NovelNodePlayer.Core;
using System.Collections.Specialized;
using System.Linq;
using UnityEngine.EventSystems;
using NovelNodePlayer.Enums;
using Cysharp.Threading.Tasks;

namespace NovelNodePlayer.UI
{
    public class GameUI : MonoBehaviour
    {
        [SerializeField] private UIState uiState = UIState.Main;
        public UIState UIState
        {
            get => uiState;
            set
            {
                uiState = value;
                menuUI.SetActive(UIState != UIState.Play);
                gameUI.SetActive(UIState == UIState.Play);
                Time.timeScale = UIState == UIState.Pause ? 0 : 1;
            }
        }

        [Header("Menu")]
        [SerializeField] private GameObject menuUI;
        [SerializeField] private Text gameTitleText;
        [SerializeField] private Text gameAuthorText;
        [SerializeField] private GameObject startScreen;
        [SerializeField] private GameObject loadScreen;
        [SerializeField] private InputField playerNameInput;
        [SerializeField] private GameObject playButton;
        [SerializeField] private GameObject continueButton;
        [SerializeField] private GameObject saveDataPrefab;
        [SerializeField] private Transform saveDataContainer;
        [SerializeField] private Slider textSpeedSlider;
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider voiceVolumeSlider;
        [SerializeField] private Slider soundFXVolumeSlider;
        [SerializeField] private Dropdown windowStateSlider;

        [Header("Gameplay")]
        [SerializeField] private GameObject gameUI;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private Text dialogueText;
        [SerializeField] private Text narratorText;
        [SerializeField] private Transform characterContainer;
        [SerializeField] private Transform choiceContainer;
        [SerializeField] private GameObject choicePrefab;
        [SerializeField] private EventTrigger pointerEvent;
        [SerializeField] private Image autoPlay;
        [SerializeField] private Sprite autoPlayEnabled;
        [SerializeField] private Sprite autoPlayDisabled;

        private void Awake()
        {
            gameTitleText.text = ProjectData.Instance.Name;
            gameAuthorText.text = ProjectData.Instance.Author;
            GameManager.Instance.Characters.CollectionChanged += SetCharacters;
            GameManager.Instance.Choices.CollectionChanged += SetChoices;
            GameManager.Instance.PointerEvent = pointerEvent;
            var saves = GameData.Instance.SaveGames;
            foreach (var save in saves)
                Instantiate(saveDataPrefab, saveDataContainer).GetComponent<SaveDataUI>().Set(save);
            continueButton.SetActive(GameData.Instance.IsSaved);
            var config = AppConfig.Instance;
            playerNameInput.text = config.PlayerName;
            textSpeedSlider.value = config.TextSpeed;
            musicVolumeSlider.value = config.MusicVolume;
            voiceVolumeSlider.value = config.VoiceVolume;
            soundFXVolumeSlider.value = config.SoundFXVolume;
            windowStateSlider.value = (int)config.WindowState;
        }

        #region Menu
        public void Pause()
        {
            UIState = UIState == UIState.Pause ? UIState.Play : UIState.Pause;
            continueButton.SetActive(UIState == UIState.Pause);
            playButton.SetActive(UIState == UIState.Main);
            menuUI.SetActive(true);
            gameUI.SetActive(false);
        }

        public void Play()
        {
            AppConfig.Instance.PlayerName = playerNameInput.text;
            GameData.Instance.Play();
            AppConfig.Instance.Save();
            startScreen.SetActive(false);
            UIState = UIState.Play;
        }

        public void Continue()
        {
            if (UIState == UIState.Main)
                GameData.Instance.Continue();

            UIState = UIState.Play;
        }

        public void Load(SaveDataUI saveData)
        {
            GameData.Instance.Load(saveData.SaveData);
            loadScreen.SetActive(false);
            UIState = UIState.Play;
        }

        public void Exit()
        {
            Application.Quit();
        }

        public void SettingsSave() => AppConfig.Instance.Save();

        public void SettingsReset()
        {
            var config = AppConfig.Instance;
            config.Reset();
            textSpeedSlider.value = config.TextSpeed;
            musicVolumeSlider.value = config.MusicVolume;
            voiceVolumeSlider.value = config.VoiceVolume;
            soundFXVolumeSlider.value = config.SoundFXVolume;
            windowStateSlider.value = (int)config.WindowState;
        }

        public void OnSetTextSpeed(float value) => AppConfig.Instance.TextSpeed = value;
        public void OnSetMusicVolume(float value) => AppConfig.Instance.MusicVolume = value;
        public void OnSetVoiceVolume(float value) => AppConfig.Instance.VoiceVolume = value;
        public void OnSetSoundFXVolume(float value) => AppConfig.Instance.SoundFXVolume = value;
        public void OnSetWindowState(int value) => AppConfig.Instance.WindowState = (FullScreenMode)value;

        #endregion

        #region Gameplay
        public void SetBackground(string sprite)
        {
            UniTask.Void(async () =>
            {
                backgroundImage.sprite = GameManager.Instance.SceneSelected.Backgrounds.FirstOrDefault(x => x.Key == sprite)?.Value.Sprite;
                var fadeTime = GameManager.Instance.fadeTime;
                var startAlpha = fadeTime < 0 ? 1f : 0f;
                var targetAlpha = 1f - startAlpha;
                var color = new Color(1f, 1f, 1f, startAlpha);
                backgroundImage.color = color;

                var elapsedTime = 0f;
                while (elapsedTime < Mathf.Abs((float)fadeTime))
                {
                    var normalizedTime = elapsedTime / Mathf.Abs((float)fadeTime);
                    color.a = Mathf.Lerp(startAlpha, targetAlpha, (float)normalizedTime);
                    backgroundImage.color = color;
                    elapsedTime += Time.deltaTime;
                    await UniTask.Yield();
                }
            });
        }

        public void SetDialogue(string dialogue)
        {
            // Set dialogue text
            dialogueText.text = dialogue;
        }

        public void SetNarrator(string narrator)
        {
            // Set narrator text
            narratorText.text = narrator;
        }

        public void SetCharacterSprite(PlayerViewCharacterData characterData)
        {
            var character = characterContainer.transform.Find(characterData.CharacterName).GetComponent<Image>();
            character.sprite = characterData.Sprite;
        }

        public void SetCharacters(object sender, NotifyCollectionChangedEventArgs e)
        {
            var characters = GameManager.Instance.Characters;
            // Clear previous characters
            foreach (Transform child in characterContainer.transform)
            {
                Destroy(child.gameObject);
            }

            // Instantiate and set new characters
            foreach (PlayerViewCharacterData characterData in characters)
            {
                var image = new GameObject().AddComponent<Image>();
                image.sprite = characterData.Sprite;
                image.preserveAspect = true;
                var anchor = image.transform as RectTransform;
                anchor.name = characterData.CharacterName;
                anchor.SetParent(characterContainer);
                anchor.localScale = Vector3.one;
                anchor.sizeDelta = new Vector2(image.sprite.rect.size.x, image.sprite.rect.size.y);
                anchor.anchorMin = Vector2.zero;
                anchor.anchorMax = Vector2.one;
                anchor.offsetMin = new Vector2(characterData.Margin.x, 0);
                anchor.offsetMax = new Vector2(0, -characterData.Margin.y);
            }
        }

        public void SetChoices(object sender, NotifyCollectionChangedEventArgs e)
        {
            var choices = GameManager.Instance.Choices;

            // Clear previous choices
            foreach (Transform child in choiceContainer.transform)
            {
                if (child == choicePrefab.transform)
                    continue;
                Destroy(child.gameObject);
            }

            // Instantiate and set new choices
            foreach (NodeChoiceData choiceData in choices)
            {
                GameObject choiceObject = Instantiate(choicePrefab, choiceContainer.transform);
                choiceObject.SetActive(true);
                choiceObject.GetComponentInChildren<Text>().text = choiceData.Text;
                choiceObject.GetComponent<Button>().onClick.AddListener(() => GameManager.Instance.selectedChoice = choiceData);
            }
        }

        public void SetAutoplay()
        {
            GameManager.Instance.isAuto = !GameManager.Instance.isAuto;
            autoPlay.sprite = GameManager.Instance.isAuto ? autoPlayEnabled : autoPlayDisabled;
        }
        #endregion

    }
}
