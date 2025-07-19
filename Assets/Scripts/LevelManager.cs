using UnityEngine;
using DG.Tweening;
using System.Collections;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Video;
using UnityEngine.UI;
using UnityEngine.Events;
using System;
using Unity.Cinemachine;

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

    [SerializeField] private CinemachineCamera playerCamera;
    [SerializeField] private CinemachineCamera openCutsceneCamera;

    [SerializeField] private Target startTarget;

    public UnityEvent OnTimeSlowed;
    public UnityEvent OnTimeResumed;

    private GameManager gm;
    private Player _player;
    private TypeWriterSettings writerSettings;
    private bool canLeave = false;

    void Start()
    {
        videoImage.color = new Color(
            videoImage.color.r,
            videoImage.color.g,
            videoImage.color.b,
            0f
        );

        hoverOverlay.material.color = new Color(
            hoverOverlay.material.color.r,
            hoverOverlay.material.color.g,
            hoverOverlay.material.color.b,
            0f
        );

        cameraOverlay.material.color = new Color(
            hoverOverlay.material.color.r,
            hoverOverlay.material.color.g,
            hoverOverlay.material.color.b,
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
        var playerObject = GameObject.FindGameObjectsWithTag("Player").FirstOrDefault();
        _player = playerObject != null ? playerObject.GetComponent<Player>() : null;

        gm = GameManager.Instance;
        StartCoroutine(Cutscene());
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
        GameObject.FindGameObjectWithTag("BeatManager").GetComponent<BeatManager>().FadeStopMusic(1); // @TODO: put into audio manager
        gm.LoadMainMenu();
    }

    IEnumerator Cutscene()
    {
        _player.SetAnimationState("isVibing", true);
        yield return new WaitForSeconds(1.0f);
        yield return TypeWriterManager.Instance.StartTypeWriterEnumerable(bootText, new List<string>()
        {
            "[Booting Systems]",
            "> Drone Connection Established",
            "> Firmware v3.14 loaded.",
            "> GPS lock acquired",
            "> Auto pilot engaged",
            "> Tracking friendly"
        }, writerSettings);

        yield return bootText.DOFade(0f, 1.0f).WaitForCompletion();

        yield return DOTween.Sequence()
            .Append(cameraOverlay.material.DOFade(1.0f, 0.3f))
            .Append(cameraOverlay.material.DOFade(0.0f, 0.2f))
            .Append(cameraOverlay.material.DOFade(0.5f, 0.1f))
            .Append(cameraOverlay.material.DOFade(1.0f, 0.3f))
            .Append(cameraOverlay.material.DOFade(0.4f, 0.2f))
            .Append(cameraOverlay.material.DOFade(1.0f, 0.3f)).WaitForCompletion();

        yield return new WaitForSeconds(0.3f);
        playerCamera.enabled = true;
        openCutsceneCamera.enabled = false;
        yield return fade.DOFade(0f, 1.0f).WaitForCompletion();
        gm.CurrentGameState = GameState.Playing;
        // Have the character vibing
        yield return new WaitForSeconds(1.0f);
        hoverOverlay.material.DOFade(1.0f, 0.5f);
        yield return new WaitForSeconds(2.0f);
        _player.agent.state = AgentState.Tracking;
        _player.SetAnimationState("isVibing", false);
        startTarget.SetUnlock(true);
    }

    IEnumerator OnLoss()
    {
        _player.SetAnimationState("isDead", true);
        yield return new WaitForSeconds(2.5f);
        yield return fade.DOFade(1.0f, 2.0f).WaitForCompletion();
        yield return videoImage.DOFade(1.0f, 1.0f).WaitForCompletion();
        canLeave = true;
    }

    IEnumerator OnWin()
    {
        // play ambient return music
        GameObject.FindGameObjectWithTag("BeatManager").GetComponent<BeatManager>().PlayMXState(1, 1f); // @TODO: put into audio manager

        _player.SetAnimationState("isBoating", true);
        yield return new WaitForSeconds(2.5f);
        yield return fade.DOFade(1.0f, 2.0f).WaitForCompletion();
        yield return videoImage.DOFade(1.0f, 1.0f).WaitForCompletion();
        canLeave = true;
    }

    public void SlowDown(Action callback = null)
    {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 0.01f, 1.0f).OnComplete(() =>
        {
            OnTimeSlowed?.Invoke();
            callback?.Invoke();
        });
    }

    public void SpeedUp(Action callback = null)
    {
        DOTween.To(() => Time.timeScale, x => Time.timeScale = x, 1.0f, 1.0f).OnComplete(() =>
        {
            OnTimeResumed?.Invoke();
            callback?.Invoke();
        });
    }
}
