using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

enum MenuScene
{
    Default,
    Monitor,
    Start,
    LeftScene
}

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Text cmdLine;
    [SerializeField] private TMP_Text mainWriter;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private List<TMP_Text> sideWriter;
    [SerializeField] private List<GameObject> virtualCameras;
    [SerializeField] private SimpleButton playButton;
    [SerializeField] private CanvasGroup fade;
    [SerializeField] private CanvasGroup pressTextFade;
    [SerializeField] private TMP_Text pressText;

    [Header("Stat Window")]
    [SerializeField] private List<SimpleButton> buttonBar;

    [Header("Level Window")]
    [SerializeField] private SimpleButton levelPrefab;
    [SerializeField] private GameObject levelParent;
    [SerializeField] private SettingsUI settingsUI;

    private Dictionary<string, List<string>> dialogue;
    private PlayerInputActions _inputActions;
    private bool _showCaret = false;
    private MenuScene currentScene = MenuScene.Default;
    private MenuScene lastScene = MenuScene.Default;

    private TypeWriterManager writer;
    private TypeWriterSettings writerSettings;
    private TypeWriterSettings cmdWriterSettings;

    private EventInstance _menuBGM;
    private EventInstance _terminalIdleSFX;
    private EventInstance _terminalBootSFX;
    private EventInstance _typingSoundSFX;

    private Tween textTween;

    private bool _monitorSceneLoading = false;
    private bool _monitorSceneReady = false;
    private bool _allowedStart = false;

    private bool _banksLoaded = false;

    private GameObject lastSelected;

    void Init()
    {
        // When we want to have a different outcome after a win/loss game
        // if (GameManager.Instance.exitedLevel)
        // {
        //     currentScene = MenuScene.LeftScene;
        // }
        // else
        // {
        //     currentScene = MenuScene.Default;
        // }

        pressTextFade.alpha = 1;
        fade.alpha = 1;
        _menuBGM = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.menuBGM, true);
        _menuBGM.setVolume(0);
        _menuBGM.start();
        AudioManager.Instance.FadeInstance(_menuBGM, true, false, 2.0f);
        // _terminalIdleSFX = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.terminalIdleSFX, false);
        // _terminalBootSFX = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.terminalBootSFX, false);
        // _typingSoundSFX = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.terminalSFX, false);

        writerSettings = new TypeWriterSettings
        {
            shouldClearOnNewLine = false,
            clearCurrent = true,
            charactersPerSecond = 200,
            delayBetweenLines = -1,
            playSound = false
        };

        cmdWriterSettings = new TypeWriterSettings
        {
            shouldClearOnNewLine = false,
            clearCurrent = true,
            charactersPerSecond = 50,
            delayBetweenLines = -1,
            // sound = _typingSoundSFX,
            playSound = true
        };

        writer = TypeWriterManager.Instance;
        _inputActions = GameManager.Instance.inputActions;
        
        SwitchCamera(currentScene);
        StartCoroutine(BlinkCaret());
        dialogue = new Dictionary<string, List<string>> {
            { "start", new List<string>()
                {
                    "[DRONE AUTONOMOUS FRAMEWORK]",
                    "Boot Sequence Initiated: 00:00:03.912",
                    "> Cortex Link: UNSANCTIONED",
                    "> Firmware Lock Bypassed [OK]",
                    "> Authority Code: NOT FOUND",
                    "> Authority Code: OVERRIDEN",
                    "> Adaptive Flight Engine: ONLINE",
                    "> Behavioral Overrides: DISABLED",
                    "> Command Protocols: IGNORED",
                    "> DRN-15... RETASKED",
                    "",
                    "[SUCCESS]",
                    "Swarm online..."
            }
            },
            { "stats", new List<string>()
                {
                    "[Stats]",
                    "Perfect Hacks:     " + GameManager.Instance.stats.perfects,
                    "Good Hacks:        " + GameManager.Instance.stats.good,
                    "Ok Hacks:          " + GameManager.Instance.stats.ok,
                    "Failed Hacks:      " + GameManager.Instance.stats.fails,
                    "Levels Complete:   " + GameManager.Instance.stats.levels
                }
            },
            {
                "credits", new List<string>()
                {
                    "[Credits]",
                    "Resben: Programmer",
                    "Idralisk: Programmer",
                    "Tokki: Artist",
                    "Papi: Artist",
                    "Josh Bakaimis: Composer"
                }
            },
            {
                "settings", new List<string>()
                {
                    "> Settings"
                }
            },
            { "levels", new List<string>()
                {
                    "> levels    " + (GameManager.Instance.CurrentLevel != null ? "| Current: " + GameManager.Instance.CurrentLevel.id : "")
                }
            },
            { "nonsense0", new List<string>()
                {
                    "> [Action] SYNCHRONIZING ...",
                    "> [SIG/SEGV] Recovering data",
                    "> [99ff::bead] Ghost thread online"
                }
            },
            { "nonsense1", new List<string>()
                {
                    "[Drone Inc] installing dependencies",
                    "[Error] Unauthorised user detected",
                    "[Security] Locating user location",
                    "[Security] Overriden",
                    "[Build] Continuing installation"
                }
            },
            { "nonsense2", new List<string>()
                {
                    "Loading: [░░░░░░░░░░] 0%",
                    "Loading: [█░░░░░░░░░] 10%",
                    "Loading: [██░░░░░░░░] 20%",
                    "Loading: [███░░░░░░░] 35%",
                    "Loading: [██████░░░░] 60%",
                    "Loading: [███████░░░] 75%",
                    "Loading: [██████████] 100%",
                }
            }
        };

        // Probably want to do some sort of intro the whole thing
        EnterState(currentScene);
    }

    void Update()
    {
        // WebGL compatability
        if (FMODUnity.RuntimeManager.HasBankLoaded("Master") && !_banksLoaded)
        {
            Debug.Log("Master Bank Loaded");
            _banksLoaded = true;
            Init();
        }
        else if (!FMODUnity.RuntimeManager.HasBankLoaded("Master"))
        {
            return;
        }


        if (lastScene != currentScene)
        {
            ExitState(lastScene);
            EnterState(currentScene);
            SwitchCamera(currentScene);
            lastScene = currentScene;
        }

        RunState(currentScene);

        // Ensure our buttons remain selected on empty click
        if (EventSystem.current.currentSelectedGameObject != null)
            lastSelected = EventSystem.current.currentSelectedGameObject;
        else if (lastSelected != null)
            EventSystem.current.SetSelectedGameObject(lastSelected);
    }

    private IEnumerator BlinkCaret()
    {
        while (true)
        {
            _showCaret = !_showCaret;
            yield return new WaitForSeconds(0.5f);
        }
    }

    private IEnumerator StartupAnimation()
    {
        yield return new WaitForSeconds(2f);
        // Show CMD command then play SFX
        // _terminalBootSFX.play();
        // _terminalIdleSFX.play();
        yield return new WaitForSeconds(1.0f);
        _monitorSceneLoading = true;

        SetButtonSelected(0);
        foreach (SimpleButton button in buttonBar)
        {
            yield return button.StartTypeWriteEffect(writerSettings);
        }

        yield return writer.StartTypeWriterEnumerable(sideWriter[0], dialogue["stats"], writerSettings);
        yield return writer.StartTypeWriterEnumerable(sideWriter[1], dialogue["levels"], writerSettings);
        foreach (var pair in GameManager.Instance.levelDirectory)
        {
            SimpleButton button = Instantiate(levelPrefab, levelParent.transform);
            button.transform.localPosition = Vector3.zero;
            button.Init(() => OnLevelSelected(pair.Value), pair.Value.GetMenuString(), !pair.Value.isUnlocked);

            if (pair.Value == GameManager.Instance.CurrentLevel)
                SetLevelSelected(pair.Value.index);

            yield return button.StartTypeWriteEffect(writerSettings);
        }

        yield return writer.StartTypeWriterEnumerable(mainWriter, dialogue["start"], writerSettings);
        StartCoroutine(NonsensePanel());

        foreach (Transform child in levelParent.transform)
            child.GetComponent<SimpleButton>().SetInteraction();

        _monitorSceneReady = true;
    }

    private void OnLevelSelected(Level level)
    {
        playButton.gameObject.SetActive(false);
        GameManager.Instance.CurrentLevel = level;
        SetLevelSelected(level.index);
        StartCoroutine(LevelSelectedCoroutine());
    }

    private IEnumerator LevelSelectedCoroutine()
    {
        yield return writer.StartTypeWriterEnumerable(sideWriter[1], dialogue["levels"], writerSettings);
        yield return writer.StartTypeWriterEnumerable(mainWriter, GameManager.Instance.CurrentLevel.description, writerSettings);
        playButton.gameObject.SetActive(true);
        yield return new WaitForEndOfFrame();
        yield return playButton.StartTypeWriteEffect(writerSettings);
        playButton.SetInteraction();
    }

    private IEnumerator NonsensePanel()
    {
        int currentIndex = 0;
        int maxIndexes = 3;

        while (true)
        {
            yield return writer.StartTypeWriterEnumerable(sideWriter[2], dialogue["nonsense" + currentIndex], writerSettings);
            currentIndex += 1;
            if (currentIndex >= maxIndexes)
                currentIndex = 0;
            yield return new WaitForSeconds(2.5f);
        }
    }

    private void CleanMenu()
    {
        StopAllCoroutines();
        writer.DestroyAll();
    }

    private void SwitchCamera(MenuScene camera)
    {
        camera = camera == MenuScene.LeftScene ? MenuScene.Monitor : camera;
        virtualCameras.ForEach(e => e.SetActive(false));
        virtualCameras[(int)camera].SetActive(true);
    }

    private IEnumerator DelayedAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }


    // Menu State Management

    private IEnumerator GameStartAnimation()
    {
        yield return new WaitForSeconds(3.5f);
        fade.DOFade(1.0f, 1.0f).WaitForCompletion();
        yield return AudioManager.Instance.FadeInstanceEnumerator(_menuBGM, true, true, 1.0f);
        CleanMenu();
        GameManager.Instance.CurrentGameState = GameState.Cutscene;
        GameManager.Instance.LoadGame();
    }

    private IEnumerator FirstIntro()
    {
        yield return new WaitForSeconds(1.0f);
        yield return fade.DOFade(0, 1.5f);
        yield return new WaitForSeconds(0.75f);
        _allowedStart = true;
        yield return writer.StartTypeWriterEnumerable(pressText, new List<string>() { "Press any button to start" }, writerSettings);
        textTween = pressText.DOFade(0.3f, 0.5f).SetLoops(-1, LoopType.Yoyo);
    }

    private void SetButtonSelected(int index)
    {
        buttonBar.ForEach(b => b.SetSelection(false));
        buttonBar[index].SetSelection(true);
    }

    private void SetLevelSelected(int index)
    {
        Debug.Log(index);

        foreach (Transform child in levelParent.transform)
            child.GetComponent<SimpleButton>().SetSelection(false);

        levelParent.transform.GetChild(index).GetComponent<SimpleButton>().SetSelection(true);
    }

    private void ExitState(MenuScene state) { }

    private void EnterState(MenuScene state)
    {
        switch (state)
        {
            case MenuScene.Default:
                StartCoroutine(FirstIntro());
                break;
            case MenuScene.Monitor:
                if (!_monitorSceneReady && !_monitorSceneLoading)
                {
                    StartCoroutine(StartupAnimation());
                }
                break;
            case MenuScene.Start:
                // _terminalIdleSFX.fadeorwhatever();
                StartCoroutine(GameStartAnimation());
                break;
            case MenuScene.LeftScene:
                pressTextFade.alpha = 0;
                break;
        }
    }

    private void RunState(MenuScene state)
    {
        var input = _inputActions.UI;
        cmdLine.text = @"C:\ > " + (_showCaret ? "|" : "");

        switch (state)
        {
            case MenuScene.Default:
                if (_allowedStart)
                {
                    if (input.Click.IsPressed())
                    {
                        textTween.Kill();
                        pressTextFade.DOFade(0, 1.0f);
                        _allowedStart = false;
                        currentScene = MenuScene.Monitor;
                    }
                }
                break;
            case MenuScene.Monitor:
                break;
            case MenuScene.Start:
                break;
        }
    }

    public void StatsClicked()
    {
        settingsUI.Reset();
        SetButtonSelected(0);
        StartCoroutine(writer.StartTypeWriterEnumerable(sideWriter[0], dialogue["stats"], writerSettings));
    }

    public void CreditsClicked()
    {
        settingsUI.Reset();
        SetButtonSelected(1);
        StartCoroutine(writer.StartTypeWriterEnumerable(sideWriter[0], dialogue["credits"], writerSettings));
    }

    public void SettingsClicked()
    {
        sideWriter[0].text = string.Empty;
        settingsUI.Reset();
        SetButtonSelected(2);
        StartCoroutine(settingsUI.StartSettingsAnimation());
    }

    public void PlayClicked()
    {
        currentScene = MenuScene.Start;
    }

    public void ExitClicked()
    {
        GameManager.Instance.ExitGame();
    }
}
