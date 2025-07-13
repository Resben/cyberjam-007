using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;
using System.Collections;

public struct TypeWriterSettings
{
    public bool shouldClearOnNewLine;
    public bool clearCurrent;
    public int charactersPerSecond;
    public float delayBetweenLines; // -1 for same speed as charactersPerSecond
    public bool playSound;
}


public class TypeWriterManager : MonoBehaviour
{
    private readonly Dictionary<TMP_Text, TypeWriter> activeWriters = new();
    private TypeWriterSettings defaultSettings;
    private static TypeWriterManager _instance;

    public static TypeWriterManager Instance
    {
        get
        {
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    private void Awake()
    {
        if (Instance != null)
            return;

        Instance = this;
    }

    void Start()
    {
        defaultSettings = new TypeWriterSettings
        {
            shouldClearOnNewLine = false,
            clearCurrent = true,
            charactersPerSecond = 60,
            delayBetweenLines = 1.0f,
            playSound = false
        };
    }

    public void StartTypeWriter(TMP_Text text, List<string> texts, TypeWriterSettings? settings = null, Action onFinshed = null)
    {
        settings ??= defaultSettings;
        TypeWriter writer = GetTypeWriter(text, settings.Value);
        writer.StartTypeWriter(texts, settings.Value, onFinshed);
    }

    public IEnumerator StartTypeWriterEnumerable(TMP_Text text, List<string> texts, TypeWriterSettings? settings = null, Action onFinshed = null)
    {
        settings ??= defaultSettings;
        TypeWriter writer = GetTypeWriter(text, settings.Value);
        return writer.StartTypeWriterEnumerable(texts, settings.Value, onFinshed);
    }

    private TypeWriter GetTypeWriter(TMP_Text text, TypeWriterSettings settings)
    {
        if (activeWriters.TryGetValue(text, out TypeWriter writer))
        {
            if (settings.clearCurrent)
            {
                writer.Clear();
            }
        }
        else
        {
            GameObject go = new("TypeWriter_" + text.name);
            go.transform.SetParent(transform);

            writer = go.AddComponent<TypeWriter>();
            writer.Init(text);
            activeWriters.Add(text, writer);
        }

        return writer;
    }

    public void DestoryWriter(TMP_Text text)
    {
        if (activeWriters.TryGetValue(text, out TypeWriter writer))
        {
            writer.Clear();
            Destroy(writer.gameObject);
            activeWriters.Remove(text);
        }
    }

    public void DestroyAll()
    {
        foreach (var pair in activeWriters)
        {
            pair.Value.Clear();
            Destroy(pair.Value.gameObject);
        }

        activeWriters.Clear();
    }

    public void SetDefaultSettings(TypeWriterSettings settings)
    {
        defaultSettings = settings;
    }
}
