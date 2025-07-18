using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Video;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    [Header("Cutscene Components")]
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private RawImage videoImage;
    [SerializeField] private TMP_Text bootText;
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] private VideoClip winClip;
    [SerializeField] private VideoClip loseClip;
    [SerializeField] private Renderer cameraOverlay;
    [SerializeField] private Renderer hackOverlay;
    [SerializeField] private Renderer hoverOverlay;

    private GameManager gm;
    private Player _player;
    private TypeWriterSettings writerSettings;
    private bool canLeave = false;

    void Start()
    {
        videoImage.color= new Color(
            videoImage.color.r,
            videoImage.color.g,
            videoImage.color.b,
            0f
        );

        writerSettings = new TypeWriterSettings
        {
            shouldClearOnNewLine = false,
            clearCurrent = true,
            charactersPerSecond = 160,
            delayBetweenLines = -1,
            playSound = false
        };
        fade.alpha = 1.0f;
        StartCoroutine(Cutscene());
        var playerObject = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        _player = playerObject != null ? playerObject.GetComponent<Player>() : null;

        gm = GameManager.Instance;
    }

    void Update()
    {
        if (canLeave)
            if (Input.GetKeyDown(KeyCode.Space))
                StartCoroutine(FadeToMenu());
    }

    // Cutscenes / Getters
    public void LoseCondition()
    {
        gm.CurrentGameState = GameState.Cutscene;
        gm.didWin = false;
        gm.exitedLevel = true;
        videoPlayer.Stop();
        videoPlayer.clip = loseClip;
        videoPlayer.Play();
        StartCoroutine(OnLoss());
    }

    public void WinCondition()
    {
        gm.CurrentGameState = GameState.Cutscene;
        gm.didWin = true;
        gm.exitedLevel = true;
        videoPlayer.Stop();
        videoPlayer.clip = winClip;
        videoPlayer.Play();
        StartCoroutine(OnWin());
    }

    IEnumerator FadeToMenu()
    {
        yield return DOTween.Sequence()
            .Join(videoImage.DOFade(0f, 1.0f))
            .Join(cameraOverlay.material.DOFade(0f, 1.0f))
            .Join(hackOverlay.material.DOFade(0f, 1.0f))
            .Join(hoverOverlay.material.DOFade(0f, 1.0f)).WaitForCompletion();

        yield return new WaitForSeconds(1.0f);
        gm.LoadMainMenu();
    }

    IEnumerator Cutscene()
    {
        yield return new WaitForSeconds(2.0f);
        yield return TypeWriterManager.Instance.StartTypeWriterEnumerable(bootText, new List<string>()
        {
            "Heyooooo",
            "lets gooo",
            "aypppp",
            "protect dat guy"
        }, writerSettings);
        yield return bootText.DOFade(0f, 2.0f).WaitForCompletion();
        yield return fade.DOFade(0f, 2.0f).WaitForCompletion();
        gm.CurrentGameState = GameState.Playing;
    }

    IEnumerator OnLoss()
    {
        _player.SetDeath();
        yield return new WaitForSeconds(2.5f);
        yield return fade.DOFade(1.0f, 2.0f).WaitForCompletion();
        yield return videoImage.DOFade(1.0f, 1.0f).WaitForCompletion();
        canLeave = true;
    }

    IEnumerator OnWin()
    {
        _player.SetWin();
        yield return new WaitForSeconds(2.5f);
        yield return fade.DOFade(1.0f, 2.0f).WaitForCompletion();
        yield return videoImage.DOFade(1.0f, 1.0f).WaitForCompletion();
        canLeave = true;
    }
}
