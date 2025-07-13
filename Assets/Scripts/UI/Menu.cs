using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using FMOD.Studio;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

enum MenuScene
{
    Default,
    Monitor,
    Start,
}

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Text cmdLine;
    [SerializeField] private TMP_Text mainWriter;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private List<TMP_Text> sideWriter;
    [SerializeField] private List<GameObject> virtualCameras;
    [SerializeField] private Button playButton;

    [Header("Level Window")]
    [SerializeField] private LevelButton levelPrefab;
    [SerializeField] private GameObject levelParent;

    private Dictionary<string, List<string>> dialogue;
    private EventInstance _typingSound;
    private EventInstance _menuBGM;
    private PlayerInputActions _inputActions;
    private bool _showCaret = false;
    private MenuScene currentScene = MenuScene.Default;
    private MenuScene lastScene = MenuScene.Default;

    private TypeWriterManager writer;
    private TypeWriterSettings writerSettings;

    private bool _monitorSceneLoading = false;
    private bool _monitorSceneReady = false;
    private bool _allowedStart = false;

    private GameObject lastSelected;

    void Start()
    {
        writerSettings = new TypeWriterSettings
        {
            shouldClearOnNewLine = false,
            clearCurrent = true,
            charactersPerSecond = 200,
            delayBetweenLines = -1,
            playSound = false
        };

        playButton.onClick.AddListener(() => currentScene = MenuScene.Start);

        writer = TypeWriterManager.Instance;
        _inputActions = GameManager.Instance.inputActions;
        SwitchCamera(currentScene);
        StartCoroutine(BlinkCaret());
        // menuBGM = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.menuBGM, true);
        // menuBGM.start();
        // typingSound = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.typingSound, false);
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
            { "levels", new List<string>()
                {
                    "> levels    " + (GameManager.Instance.CurrentLevel != null ? "| Current: " + GameManager.Instance.CurrentLevel.id : "")
                }
            },
            { "nonsense0", new List<string>()
                {
                    "Just some nonsense for 0",
                    "asdasdasd",
                    "asdasdasdasd"
                }
            },
            { "nonsense1", new List<string>()
                {
                    "Just some nonsense for 1",
                    "asdasdasd",
                    "asdasdasdasd"
                }
            },
            { "nonsense2", new List<string>()
                {
                    "Just some nonsense for 2",
                    "asdasdasd",
                    "asdasdasdasd"
                }
            }
        };

        // Probably want to do some sort of intro the whole thing
        EnterState(MenuScene.Default);
    }

    void Update()
    {
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
        _monitorSceneLoading = true;
        yield return writer.StartTypeWriterEnumerable(sideWriter[0], dialogue["stats"], writerSettings);
        yield return writer.StartTypeWriterEnumerable(sideWriter[1], dialogue["levels"], writerSettings);
        foreach (var pair in GameManager.Instance.levelDirectory)
        {
            LevelButton button = Instantiate(levelPrefab, levelParent.transform);
            button.transform.localPosition = Vector3.zero;
            button.Init(pair.Value, 1, OnLevelSelected);

            if (pair.Value == GameManager.Instance.CurrentLevel)
                EventSystem.current.SetSelectedGameObject(button.gameObject);

            yield return button.StartTypeWriteEffect(writerSettings);
        }

        yield return writer.StartTypeWriterEnumerable(mainWriter, dialogue["start"], writerSettings);
        StartCoroutine(NonsensePanel());

        foreach (Transform child in levelParent.transform)
            child.GetComponent<LevelButton>().SetInteraction();

        _monitorSceneReady = true;
    }

    private void OnLevelSelected(Level level)
    {
        playButton.gameObject.SetActive(false);
        GameManager.Instance.CurrentLevel = level;
        StartCoroutine(LevelSelectedCoroutine());
    }

    private IEnumerator LevelSelectedCoroutine()
    {
        yield return writer.StartTypeWriterEnumerable(sideWriter[1], dialogue["levels"], writerSettings);
        yield return writer.StartTypeWriterEnumerable(mainWriter, GameManager.Instance.CurrentLevel.description, writerSettings);
        playButton.gameObject.SetActive(true);
        playButton.interactable = false;
        // Yo awesome code
        yield return writer.StartTypeWriterEnumerable(playButton.gameObject.GetComponentInChildren<TMP_Text>(), new List<string>() { "Play" }, writerSettings);
        playButton.interactable = true;
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

    private void ExitMenu()
    {
        StopAllCoroutines();
        writer.DestroyAll();
    }

    private void SwitchCamera(MenuScene camera)
    {
        virtualCameras.ForEach(e => e.SetActive(false));
        virtualCameras[(int)camera].SetActive(true);
    }

    private IEnumerator DelayedAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action?.Invoke();
    }


    // Menu State Management

    private void ExitState(MenuScene state) {}

    private void EnterState(MenuScene state)
    {
        switch (state)
        {
            case MenuScene.Default:
                StartCoroutine(DelayedAction(() => _allowedStart = true, 2.5f));
                break;
            case MenuScene.Monitor:
                if (!_monitorSceneReady && !_monitorSceneLoading)
                {
                    StartCoroutine(StartupAnimation());
                }
                break;
            case MenuScene.Start:
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
}
