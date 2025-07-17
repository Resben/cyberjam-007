using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class SimpleButton : MonoBehaviour
{
    [SerializeField] private string text;
    [SerializeField] private Button button;
    [SerializeField] private TMP_Text textBox;
    [SerializeField] private Color toggledColour;
    private Color _saveNormalColor;
    private Action _callback;
    private bool _forceDisable = false;
    private bool _isSelected = false;

    public void Start()
    {
        textBox.text = string.Empty;
        _saveNormalColor = button.colors.normalColor;
    }

    public void Init(Action callback, string text, bool disabled = false)
    {
        _forceDisable = disabled;
        this.text = text;
        _callback = callback;
        button.onClick.AddListener(() => OnClick());
        button.interactable = false;
    }

    public IEnumerator StartTypeWriteEffect(TypeWriterSettings settings)
    {
        yield return TypeWriterManager.Instance.StartTypeWriterEnumerable(textBox, new List<string>() { text }, settings);
    }

    public void SetInteraction()
    {
        if (!_forceDisable)
            button.interactable = true;
    }

    public bool IsSelected()
    {
        return _isSelected;
    }

    public void SetSelection(bool isSelected)
    {
        _isSelected = isSelected;

        if (_isSelected)
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = toggledColour;
            button.colors = colorBlock;
        }
        else
        {
            ColorBlock colorBlock = button.colors;
            colorBlock.normalColor = _saveNormalColor;
            button.colors = colorBlock;
        }
    }

    public void SetCallback(Action callback)
    {
        _callback = callback;
        Debug.Log(text);
    }

    public void OnClick()
    {
        _callback?.Invoke();
    }
}
