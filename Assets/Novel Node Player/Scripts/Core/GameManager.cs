using System.Collections.Generic;
using UnityEngine;
using NovelNodePlayer.Data;
using NovelNodePlayer.Enums;
using System.Linq;
using System;
using NovelNodePlayer.Helpers;
using System.IO;
using Cysharp.Threading.Tasks;
using System.Threading.Tasks;
using NetFabric.Hyperlinq;
using Newtonsoft.Json;
using System.Collections.ObjectModel;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using System.Threading;

namespace NovelNodePlayer.Core
{
    public class GameManager : MonoBehaviour
    {
        public SceneData SceneSelected;
        public AssetPlayState State = AssetPlayState.Stopped;

        private string _background = string.Empty;
        public string Background
        {
            get => _background;
            set
            {
                if (_background != value)
                {
                    _background = value;
                    OnBackgroundChange.Invoke(_background);
                }
            }
        }

        private string _dialogue = string.Empty;
        public string Dialogue
        {
            get => _dialogue;
            set
            {
                if (_dialogue != value)
                {
                    _dialogue = value;
                    OnDialogueChange.Invoke(_dialogue);
                }
            }
        }

        private string _narrator = string.Empty;
        public string Narrator
        {
            get => _narrator;
            set
            {
                if (_narrator != value)
                {
                    _narrator = value;
                    OnNarratorChange.Invoke(_narrator);
                }
            }
        }

        public ObservableCollection<PlayerViewCharacterData> Characters = new();
        public ObservableCollection<NodeChoiceData> Choices = new();

        public UnityEvent<string> OnBackgroundChange = new();
        public UnityEvent<string> OnDialogueChange = new();
        public UnityEvent<string> OnNarratorChange = new();
        public UnityEvent<PlayerViewCharacterData> OnCharacterChange = new();

        public List<Node> parsedNodes = new();
        private Dictionary<string, Node> nodesById;
        private IEnumerable<IGrouping<string, NodeConnection>> connectionsBySource;
        private List<BlackboardData> blackboards = new();
        internal NodeChoiceData selectedChoice = null;
        private AudioSource BGSound;
        private AudioSource DialogueSound;
        internal EventTrigger PointerEvent;
        internal bool isAuto;
        internal double fadeTime = 0f;

        private void Awake()
        {
            BGSound = gameObject.AddComponent<AudioSource>();
            DialogueSound = gameObject.AddComponent<AudioSource>();
        }

        public void Play()
        {
            switch (State)
            {
                case AssetPlayState.Stopped:
                    blackboards = ProjectData.Current.Blackboards.Copy();
                    foreach (var blackboard in blackboards)
                        blackboard.PopulateCache();
                    connectionsBySource = SceneSelected.Connections.GroupBy(c => c.SourceID);
                    nodesById = SceneSelected.Nodes.AsValueEnumerable()
                        .Where(node => node.Input != null)
                        .SelectMany(node => node.Input.Select(input => (input.ID, node)))
                        .ToDictionary(pair => pair.ID, pair => pair.node);
                    var startNode = SceneSelected.Nodes.FirstOrDefault(x => x is NodeState node && node.State == NodeSwitch.Enter);
                    if (startNode != null)
                    {
                        nodesById.Add(startNode.Guid, startNode);
                        parsedNodes.Add(startNode);
                        _ = TraverseConnections(startNode);
                    }
                    State = AssetPlayState.Playing;
                    break;
                case AssetPlayState.Playing:
                    Choices.Clear();
                    Characters.Clear();
                    parsedNodes.Clear();
                    blackboards = null;
                    nodesById = null;
                    connectionsBySource = null;
                    Background = Dialogue = Narrator = string.Empty;
                    State = AssetPlayState.Stopped;
                    break;
            }
        }

        public void LoadCheckpoint(SaveData savedData)
        {
            SceneSelected = ProjectData.Current.Scenes.First(x => x.Name == savedData.Scene);

            connectionsBySource = SceneSelected.Connections.GroupBy(c => c.SourceID);
            nodesById = SceneSelected.Nodes
                .AsValueEnumerable()
                .Where(node => node.Input != null)
                .SelectMany(node => node.Input.Select(input => (input.ID, node)))
                .ToDictionary(pair => pair.ID, pair => pair.node);

            parsedNodes.Clear();
            Background = savedData.Background;
            Dialogue = savedData.Dialogue;
            Narrator = savedData.Narrator;

            var checkpointNode = nodesById?.First(x => x.Value.Type.Equals("Checkpoint") && ((NodeCheckpoint)x.Value).CheckpointID == savedData.ID).Value;
            parsedNodes.Add(checkpointNode);

            var targetNode = nodesById?.FirstOrDefault(node => node.Value.Guid == checkpointNode.Previous).Value;
            parsedNodes.Insert(0, targetNode);

            while (!targetNode.Type.Equals("Base"))
            {
                var node = nodesById?.FirstOrDefault(node => node.Value.Guid == targetNode.Previous).Value;
                targetNode = node;
                parsedNodes.Insert(0, node);
            }

            Characters.Clear();
            foreach (var character in savedData.Characters)
            {
                character.Sprite = character.CharacterData.Sprites.FirstOrDefault(x => x.Key == character.SpriteName).Value.Sprite;
                Characters.Add(character);
            }
            Choices.Clear();
            foreach (var choice in savedData.Choices)
            {
                Choices.Add(choice);
            }
            blackboards.Clear();
            foreach (var blackboard in savedData.Blackboards)
            {
                blackboard.PopulateCache();
                blackboards.Add(blackboard);
            }

            var nextNodeId = connectionsBySource.FirstOrDefault(group => checkpointNode.Output.Any(x => x.ID == group.Key))?.First()?.TargetID;
            var nextNode = nextNodeId != null ? nodesById.GetValueOrDefault(nextNodeId) : null;
            if (nextNode != null)
                _ = TraverseConnections(nextNode, true);
        }

        private async UniTask TraverseConnections(Node nodeId, bool overrideNode = false)
        {
            var targetNode = overrideNode ? nodeId : nodesById?.GetValueOrDefault(connectionsBySource.FirstOrDefault(group => nodeId.Output.Any(x => x.ID == group.Key)).FirstOrDefault().TargetID);
            if (targetNode != null && !parsedNodes.Contains(targetNode))
            {
                parsedNodes.Add(targetNode);
                switch (targetNode)
                {
                    case NodeState node:
                        HandleNodeState(node);
                        return;
                    case NodeBackground node:
                        HandleNodeBackground(node);
                        break;
                    case NodeCharacter node:
                        HandleNodeCharacter(node);
                        break;
                    case NodeDialogue node:
                        await HandleNodeDialogue(node);
                        break;
                    case NodeChoice node:
                        await HandleNodeChoice(node);
                        return;
                    case NodeEvent node:
                        HandleNodeEvent(node);
                        break;
                    case NodeCondition node:
                        HandleNodeCondition(node);
                        return;
                    case NodeScene node:
                        HandleNodeScene(node);
                        return;
                    case NodeCheckpoint node:
                        HandleNodeCheckpoint(node);
                        return;

                }
                _ = TraverseConnections(targetNode);
            }
        }

        private void HandleNodeState(NodeState node)
        {
            if (node.State == NodeSwitch.Exit)
                Play();
            else
                _ = TraverseConnections(node);
        }

        private void HandleNodeBackground(NodeBackground node)
        {
            if (node.AssetType == AssetType.Sprite)
            {
                fadeTime = node.State == NodeSwitch.Enter ? node.FadeTime : -node.FadeTime;
                Background = SceneSelected.Backgrounds.FirstOrDefault(x => x.Key == node.Background).Key ?? string.Empty;
            }
            else if (node.AssetType == AssetType.Audio)
            {
                BGSound.clip = SceneSelected.Sounds.FirstOrDefault(x => x.Name == node.Background).Audio;
                BGSound.loop = node.Loop;
                BGSound.Play();
            }
        }

        private void HandleNodeCharacter(NodeCharacter node)
        {
            switch (node.State)
            {
                case NodeSwitch.Enter:
                    var characterData = node.CharacterData;
                    var sprite = characterData.Sprites.FirstOrDefault(x => x.Key == node.Sprite);
                    if (sprite != null)
                    {
                        float x = (float)((node.Position - 50) / 50.0 * 2560);
                        float y = (1440 - sprite.Value.Sprite.rect.size.y) / 2;
                        var data = new PlayerViewCharacterData
                        {
                            CharacterName = characterData.Name,
                            SpriteName = sprite.Key,
                            Sprite = sprite.Value.Sprite,
                            CharacterData = characterData,
                            Margin = new Rect(x, Mathf.Abs(y), 0, 0)
                        };

                        Characters.Add(data);
                    }
                    break;
                case NodeSwitch.Exit:
                    var character = Characters.FirstOrDefault(x => x.CharacterData.Name == node.Character);
                    if (character != null)
                        Characters.Remove(character);
                    break;
            }
        }

        private async UniTask HandleNodeChoice(NodeChoice node)
        {
            Choices.AddRange(node.Choices);
            await UniTask.WaitUntil(() => selectedChoice != null);
            var target = node.Output[selectedChoice.Output];
            var connection = connectionsBySource?.FirstOrDefault(x => x.Key == target.ID);
            var choiceOutput = nodesById?.GetValueOrDefault(connection.First().TargetID);
            selectedChoice = null;
            Choices.Clear();
            _ = TraverseConnections(choiceOutput, true);
        }

        private async UniTask HandleNodeDialogue(NodeDialogue node)
        {
            Narrator = node.IsPlayerDialogue ? AppConfig.Instance.PlayerName : node.Character;
            var characterData = Characters.FirstOrDefault(x => x.CharacterData.Name == node.Character);
            foreach (var line in node.Lines)
            {
                if (characterData != null)
                {
                    characterData.SpriteName = line.CharacterSprite;
                    characterData.Sprite = characterData.CharacterData.Sprites.FirstOrDefault(x => x.Key == line.CharacterSprite).Value.Sprite;
                    OnCharacterChange.Invoke(characterData);
                    var dialogueAudio = characterData.CharacterData.Sounds.FirstOrDefault(x => x.Name == line.CharacterAudio);
                    if (dialogueAudio != null && dialogueAudio.Audio != null)
                    {
                        DialogueSound.clip = dialogueAudio.Audio;
                        DialogueSound.Play();
                    }
                    
                    Dialogue = string.Empty;
                    var dialogueLine = line.Text;
                    if (!node.IsPlayerDialogue)
                        dialogueLine = dialogueLine.Replace("{Player}", $"{AppConfig.Instance.PlayerName}");
                    var dialogueChars = dialogueLine.ToCharArray();

                    var cts = new CancellationTokenSource();

                    EventTrigger.Entry entry = new() { eventID = EventTriggerType.PointerDown };
                    entry.callback.AddListener((eventData) => cts.Cancel());
                    PointerEvent.triggers.Add(entry);


                    var textSpeed = (int)(125f / AppConfig.Instance.TextSpeed);

                    try
                    {
                        foreach (var character in dialogueChars)
                        {
                            Dialogue += character;
                            await UniTask.Delay(textSpeed, cancellationToken: cts.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        Dialogue = dialogueLine;
                        cts = new();
                    }

                    await UniTask.WaitUntil(() => cts.IsCancellationRequested || isAuto == true);
                    PointerEvent.triggers.Clear();
                    DialogueSound.Stop();
                }
            }
        }

        private void HandleNodeEvent(NodeEvent node)
        {
            var board = blackboards.AsValueEnumerable().First(x => x.Name == node.BlackboardName)?.GetValueFromCacheOrSearch(node.BlackboardType, node.BlackboardKey);
            if (board != null)
            {
                switch (node.BlackboardType)
                {
                    case Enums.ValueType.Float:
                        if (node.Operator == EventTask.Set)
                            board.Value.Float = float.Parse(node.NewValue);
                        else
                            board.Value.Float += node.Operator == EventTask.Increase ? float.Parse(node.NewValue) : -float.Parse(node.NewValue);
                        break;
                    case Enums.ValueType.String:
                        board.Value.String = node.NewValue;
                        break;
                    case Enums.ValueType.Boolean:
                        board.Value.Boolean = bool.Parse(node.NewValue);
                        break;
                }
            }
        }

        private void HandleNodeCondition(NodeCondition node)
        {
            var targetBoard = blackboards.AsValueEnumerable().First(x => x.Name == node.BlackboardName)?.GetValueFromCacheOrSearch(node.BlackboardType, node.BlackboardKey);
            if (targetBoard != null)
            {
                var requirementsMet = false;

                switch (node.BlackboardType)
                {
                    case Enums.ValueType.Float:
                        var floatValue = float.Parse(node.CompareValue);
                        requirementsMet = node.Operator switch
                        {
                            ComparisonOperator.EqualTo => targetBoard.Value.Float == floatValue,
                            ComparisonOperator.NotEqualTo => targetBoard.Value.Float != floatValue,
                            ComparisonOperator.GreaterThan => targetBoard.Value.Float > floatValue,
                            ComparisonOperator.LessThan => targetBoard.Value.Float < floatValue,
                            ComparisonOperator.GreaterThanOrEqualTo => targetBoard.Value.Float >= floatValue,
                            ComparisonOperator.LessThanOrEqualTo => targetBoard.Value.Float <= floatValue,
                            _ => false
                        };
                        break;
                    case Enums.ValueType.String:
                        requirementsMet = node.Operator switch
                        {
                            ComparisonOperator.EqualTo => targetBoard.Value.String.Equals(node.CompareValue),
                            _ => !targetBoard.Value.String.Equals(node.CompareValue),
                        };
                        break;
                    case Enums.ValueType.Boolean:
                        requirementsMet = node.Operator switch
                        {
                            ComparisonOperator.EqualTo => targetBoard.Value.Boolean == bool.Parse(node.CompareValue),
                            _ => targetBoard.Value.Boolean != bool.Parse(node.CompareValue),
                        };
                        break;
                }

                var reqNode = node.Output[requirementsMet ? 0 : 1];
                var reqConnection = connectionsBySource?.FirstOrDefault(x => x.Key == reqNode.ID);
                var reqTargetNode = nodesById?.GetValueOrDefault(reqConnection.First().TargetID);
                _ = TraverseConnections(reqTargetNode, true);
            }
        }

        private void HandleNodeScene(NodeScene node)
        {
            SceneSelected = ProjectData.Current.Scenes.First(x => x.Name == node.SceneName);
            connectionsBySource = SceneSelected.Connections.GroupBy(c => c.SourceID);
            nodesById = SceneSelected.Nodes.ToDictionary(n => n.Input.First().ID);
            var startNode = nodesById.Values.First();
            if (startNode != null)
            {
                parsedNodes.Add(startNode);
                _ = TraverseConnections(startNode);
            }
            State = AssetPlayState.Playing;
        }

        public void HandleNodeCheckpoint(NodeCheckpoint node)
        {
            switch (node.Action)
            {
                case CheckpointAction.Save:
                    var saveData = new SaveData
                    {
                        ID = node.CheckpointID,
                        Scene = SceneSelected.Name,
                        Background = Background,
                        Dialogue = Dialogue,
                        Narrator = Narrator,
                        Timestamp = DateTime.Now.ToString(),
                        Characters = new List<PlayerViewCharacterData>(Characters),
                        Choices = new List<NodeChoiceData>(Choices),
                        Blackboards = new List<BlackboardData>(blackboards)
                    };

                    var serializedData = JsonConvert.SerializeObject(saveData, Formatting.Indented);
                    File.WriteAllText($"{AppConfig.DataPath}/Checkpoints/checkpoint_{node.CheckpointID}.json", serializedData);
                    ScreenCapture.CaptureScreenshot($"{AppConfig.DataPath}/Checkpoints/checkpoint_{node.CheckpointID}.png");
                    break;
                case CheckpointAction.Rollback:
                    var savedData = GameData.Instance.SaveGames.FirstOrDefault(x => x.ID == node.TargetCheckpointID);
                    if (savedData != null)
                        LoadCheckpoint(savedData);
                    else
                        Debug.LogWarning($"No saved data found for checkpoint id {node.TargetCheckpointID}");
                    break;
            }
        }

        #region Statics
        private static GameManager instance;
        public static GameManager Instance => instance ??= FindObjectOfType<GameManager>();
        #endregion
    }
}
