using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private SimpleButton playButton;

    [Header("Stat Window")]
    [SerializeField] private List<SimpleButton> buttonBar;

    [Header("Level Window")]
    [SerializeField] private SimpleButton levelPrefab;
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
    private TypeWriterSettings cmdWriterSettings;

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

        cmdWriterSettings = new TypeWriterSettings
        {
            shouldClearOnNewLine = false,
            clearCurrent = true,
            charactersPerSecond = 50,
            delayBetweenLines = -1,
            playSound = true
        };

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
            {
                "credits", new List<string>()
                {
                    "[Credits]",
                    "ABC by Resben",
                    "CDE by yo mumma",
                    "FGH by aiy",
                    "IJK by oop"
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
        yield return new WaitForSeconds(2f);
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

    private IEnumerator GameStartAnimation()
    {
        yield return new WaitForSeconds(5f);
        GameManager.Instance.CurrentGameState = GameState.Cutscene;
        GameManager.Instance.LoadGame();
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
                StartCoroutine(DelayedAction(() => _allowedStart = true, 2.5f));
                break;
            case MenuScene.Monitor:
                if (!_monitorSceneReady && !_monitorSceneLoading)
                {
                    StartCoroutine(StartupAnimation());
                }
                break;
            case MenuScene.Start:
                StartCoroutine(GameStartAnimation());
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

    public void StatsClicked()
    {
        SetButtonSelected(0);
        StartCoroutine(writer.StartTypeWriterEnumerable(sideWriter[0], dialogue["stats"], writerSettings));
    }

    public void CreditsClicked()
    {
        SetButtonSelected(1);
        StartCoroutine(writer.StartTypeWriterEnumerable(sideWriter[0], dialogue["credits"], writerSettings));
    }

    public void SettingsClicked()
    {
        SetButtonSelected(2);
        StartCoroutine(writer.StartTypeWriterEnumerable(sideWriter[0], dialogue["settings"], writerSettings));
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
