using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TypeWriter : MonoBehaviour
{
    public event Action OnTypeWriterCleared;
    public event Action<int> OnLineFinished;
    public event Action OnTypeWriterFinished;

    private int currentStringIndex = 0;
    private int currentLineIndex = 0;
    private string currentString = "";
    private bool skipLine = false;
    private Queue<TextSet> textQueue = new();
    private TextSet currentSets = new();
    private TMP_Text textBox;

    private bool isTyping = false;

    struct TextSet
    {
        public List<string> s_texts;
        public Action s_onFinshed;
        public TypeWriterSettings s_settings;
    }

    public void Init(TMP_Text text)
    {
        textBox = text;
    }

    public void StartTypeWriter(List<string> texts, TypeWriterSettings settings, Action onFinshed = null)
    {
        textQueue.Enqueue(new TextSet { s_texts = texts, s_onFinshed = onFinshed, s_settings = settings });

        if (isTyping)
        {
            LoadNext();
        }
    }

    // Warning this does just clear everything
    public IEnumerator StartTypeWriterEnumerable(List<string> texts, TypeWriterSettings settings, Action onFinshed = null)
    {
        ClearTypeWriter();
        currentSets = new TextSet { s_texts = texts, s_onFinshed = onFinshed, s_settings = settings };
        yield return TypeWriteCoroutine();
    }

    private void LoadNext()
    {
        if (textQueue.TryDequeue(out currentSets))
        {
            var num = TypeWriteCoroutine();
            StartCoroutine(num);
        }
    }

    private IEnumerator TypeWriteCoroutine()
    {
        WaitForSeconds delay = new(1f / currentSets.s_settings.charactersPerSecond);
        WaitForSeconds delayBetweenLines = new(currentSets.s_settings.delayBetweenLines);

        if (currentSets.s_settings.delayBetweenLines < 0)
            delayBetweenLines = delay;

        isTyping = true;
        textBox.text = "";
        foreach (string line in currentSets.s_texts)
        {
            currentString = line;

            foreach (char c in line)
            {
                textBox.text += c;
                currentStringIndex++;
                if (skipLine)
                {
                    textBox.text += currentString[currentStringIndex..];
                    break;
                }
                // Skip the space if the next character is also a space
                if (!(c == ' ' && currentStringIndex + 1 < currentString.Length && currentString[currentStringIndex + 1] == ' '))
                {
                    yield return delay;
                }
            }

            currentStringIndex = 0;

            yield return delayBetweenLines;

            if (currentSets.s_settings.shouldClearOnNewLine)
                textBox.text = string.Empty;
            else
                textBox.text += "\n";

            currentLineIndex++;
            OnLineFinished?.Invoke(currentLineIndex);

        }

        isTyping = false;
        OnTypeWriterFinished?.Invoke();
        currentSets.s_onFinshed?.Invoke();

        LoadNext();
    }

    public bool IsTyping()
    {
        return isTyping;
    }

    public void SkipAll()
    {
        if (!isTyping)
            return;

        textBox.text = string.Empty;

        StopAllCoroutines();
        foreach (string line in currentSets.s_texts)
        {
            textBox.text += line + "\n";
        }
    }

    private void ClearTypeWriter()
    {
        StopAllCoroutines();
        textBox.text = string.Empty;
        currentSets.s_texts?.Clear();
        currentSets.s_onFinshed = null;
        currentString = string.Empty;
        currentStringIndex = 0;
        skipLine = false;
        OnTypeWriterCleared?.Invoke();
        currentLineIndex = 0;
    }

    public void Clear()
    {
        ClearTypeWriter();
    }
}