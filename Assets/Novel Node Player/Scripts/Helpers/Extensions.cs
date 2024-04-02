using System.Collections;
using NetFabric.Hyperlinq;
using System;
using NovelNodePlayer.Data;
using NovelNodePlayer.Enums;
using UnityEngine;
using System.Collections.Generic;
using System.Reflection;
using System.Collections.ObjectModel;
using UnityEngine.UI;

namespace NovelNodePlayer.Helpers
{
    public static class Extensions
    {
        #region Events
        public delegate void SaveDataEvent();
        public static SaveDataEvent onSaveData;

        public static void SaveData()
        {
            if (onSaveData != null)
                onSaveData();
        }

        public delegate void LoadDataEvent();
        public static LoadDataEvent onLoadData;

        public static void LoadData()
        {
            if (onLoadData != null)
                onLoadData();
        }

        public delegate void ProjectLoadEvent();
        public static ProjectLoadEvent onProjectLoad;

        public static void ProjectLoad()
        {
            if (onProjectLoad != null)
                onProjectLoad();
        }

        #endregion

        #region DataHandlingMethods
        public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                collection.Add(item);
            }
        }

        public static string RemoveSpaces(this string text)
        {
            return text.Replace(" ", "");
        }

        public static double BytesToMB(this long bytes)
        {
            const double megabyte = 1024 * 1024;

            double megabytes = (double)bytes / megabyte;
            return megabytes;
        }

        public static double BytesToGB(this long bytes)
        {
            const double gigabyte = 1024 * 1024 * 1024;

            double gigabytes = (double)bytes / gigabyte;
            return gigabytes;
        }

        public static void CopyFrom(this ScriptableObject target, ScriptableObject source)
        {
            Type type = source.GetType();
            FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

            foreach (FieldInfo field in fields)
            {
                field.SetValue(target, field.GetValue(source));
            }
        }

        public static string GetString(this byte[] data) => System.Text.Encoding.UTF8.GetString(data);

        #endregion

        #region EnumsValues
        public static IEnumerable NodeSwitch => Enum.GetValues(typeof(NodeSwitch));
        public static IEnumerable NodeConnectorFlow => Enum.GetValues(typeof(NodeConnectorFlow));
        public static IEnumerable ValueType => Enum.GetValues(typeof(Enums.ValueType));
        public static IEnumerable EventTask => Enum.GetValues(typeof(EventTask));
        public static IEnumerable ComparisonOperator => Enum.GetValues(typeof(ComparisonOperator));
        public static IEnumerable CheckpointAction => Enum.GetValues(typeof(CheckpointAction));
        public static IEnumerable WindowState => Enum.GetValues(typeof(FullScreenMode));
        public static IEnumerable AssetType => Enum.GetValues(typeof(AssetType));

        #endregion

        #region UIHandlingMethods

        public static Vector2 RectToAnchoredPosition(this Rect rect, RectTransform rectTransform)
        {
            var rectCenter = rect.center;
            var anchorCenter = new Vector2(
                (rectCenter.x - rectTransform.rect.min.x) / rectTransform.rect.width,
                (rectCenter.y - rectTransform.rect.min.y) / rectTransform.rect.height
            );

            var anchoredPosition = new Vector2(
                Mathf.Lerp(rectTransform.anchorMin.x, rectTransform.anchorMax.x, anchorCenter.x),
                Mathf.Lerp(rectTransform.anchorMin.y, rectTransform.anchorMax.y, anchorCenter.y)
            );

            anchoredPosition.x *= rectTransform.rect.width;
            anchoredPosition.y *= rectTransform.rect.height;

            var pivotOffset = new Vector2(
                (0.5f - rectTransform.pivot.x) * rectTransform.rect.width,
                (0.5f - rectTransform.pivot.y) * rectTransform.rect.height
            );

            return anchoredPosition - pivotOffset;
        }

        public static void CrossFadeAlphaFixed(this Graphic graphic, float alpha, float duration, bool ignoreTimeScale)
        {
            Color fixedColor = graphic.color;
            fixedColor.a = 1;
            graphic.color = fixedColor;
            graphic.CrossFadeAlpha(0f, 0f, true);
            graphic.CrossFadeAlpha(alpha, duration, ignoreTimeScale);
        }

        #endregion

        #region PlayerViewMethods

        public static List<BlackboardData> Copy(this List<BlackboardData> source)
        {
            return new List<BlackboardData>(
                source.AsValueEnumerable().Select(item => new BlackboardData
                {
                    Name = item.Name,
                    ID = item.ID,
                    Strings = new List<KeyValue>(item.Strings.AsValueEnumerable().Select(keyValue => new KeyValue
                    {
                        Key = keyValue.Key,
                        Type = keyValue.Type,
                        Value = new KeyValue.ValueData
                        {
                            String = keyValue.Value.String,
                            Float = keyValue.Value.Float,
                            Boolean = keyValue.Value.Boolean
                        }
                    })),
                    Floats = new List<KeyValue>(item.Floats.AsValueEnumerable().Select(keyValue => new KeyValue
                    {
                        Key = keyValue.Key,
                        Type = keyValue.Type,
                        Value = new KeyValue.ValueData
                        {
                            String = keyValue.Value.String,
                            Float = keyValue.Value.Float,
                            Boolean = keyValue.Value.Boolean
                        }
                    })),
                    Booleans = new List<KeyValue>(item.Booleans.AsValueEnumerable().Select(keyValue => new KeyValue
                    {
                        Key = keyValue.Key,
                        Type = keyValue.Type,
                        Value = new KeyValue.ValueData
                        {
                            String = keyValue.Value.String,
                            Float = keyValue.Value.Float,
                            Boolean = keyValue.Value.Boolean
                        }
                    }))
                }));
        }

        #endregion
    }
}