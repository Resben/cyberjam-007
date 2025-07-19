using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

// Simply just handles the animation / settings so we don't clutter Menu
public class SettingsUI : MonoBehaviour
{
    [SerializeField] private TMP_Text volumeText;
    [SerializeField] private TMP_Text sfxText;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private Slider sfxSlider;

    private TypeWriterManager _tm;
    private TypeWriterSettings _writerSettings;
    private CanvasGroup _volumeSliderGroup;
    private CanvasGroup _sfxSliderGroup;

    void Start()
    {
        _writerSettings = new TypeWriterSettings
        {
            shouldClearOnNewLine = false,
            clearCurrent = true,
            charactersPerSecond = 200,
            delayBetweenLines = -1,
            playSound = false
        };

        _volumeSliderGroup = volumeSlider.gameObject.GetComponent<CanvasGroup>();
        _sfxSliderGroup = sfxSlider.gameObject.GetComponent<CanvasGroup>();

        _tm = TypeWriterManager.Instance;

        volumeSlider.value = GameManager.Instance.GetVolume();
        sfxSlider.value = GameManager.Instance.GetSFXVolume();

        volumeSlider.onValueChanged.AddListener(value => GameManager.Instance.SetVolume(value));
        sfxSlider.onValueChanged.AddListener(value => GameManager.Instance.SetSFXVolume(value));
    }

    public IEnumerator StartSettingsAnimation()
    {
        _volumeSliderGroup.DOFade(1.0f, 0.25f);
        yield return _tm.StartTypeWriterEnumerable(volumeText, new List<string>() { "Volume" }, _writerSettings);
        _sfxSliderGroup.DOFade(1.0f, 0.25f);
        yield return _tm.StartTypeWriterEnumerable(sfxText, new List<string>() { "SFX" }, _writerSettings);
    }

    public void Reset()
    {
        _volumeSliderGroup.alpha = 0;
        _sfxSliderGroup.alpha = 0;
        volumeText.text = string.Empty;
        sfxText.text = string.Empty;
    }
}
