using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelButton : MonoBehaviour
{
    [SerializeField] private TMP_Text text;
    private Level _level;
    private int _page;
    private Button _button;
    private Action<Level> _callback;
    public void Init(Level level, int page, Action<Level> callback)
    {
        text.text = string.Empty;
        _level = level;
        _page = page;
        _button = GetComponent<Button>();
        _callback = callback;
        _button.onClick.AddListener(() => OnClick());
        _button.interactable = false;
    }

    public IEnumerator StartTypeWriteEffect(TypeWriterSettings settings)
    {
        yield return TypeWriterManager.Instance.StartTypeWriterEnumerable(text, new List<string>() { _level.GetMenuString() }, settings);
    }

    public void SetInteraction()
    {
        if (_level.isUnlocked)
            _button.interactable = true;
    }

    public void OnClick()
    {
        _callback?.Invoke(_level);
    }
}
