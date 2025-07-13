using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using TMPro;
using UnityEngine;

enum MenuScene
{
    Default,
    Monitor,
    Start,
}

public class Menu : MonoBehaviour
{
    [SerializeField] private TMP_Text cmdLine;
    [SerializeField] private TypeWriter mainWriter;
    [SerializeField] private float fadeDuration = 0.5f;
    [SerializeField] private List<TypeWriter> sideWriter;
    [SerializeField] private List<GameObject> virtualCameras;

    private Dictionary<string, Action<TypeWriter>> cmds;
    private EventInstance _typingSound;
    private EventInstance _menuBGM;
    private PlayerInputActions _inputActions;
    private bool _showCaret = false;
    private MenuScene currentScene = MenuScene.Default;
    private MenuScene lastScene = MenuScene.Default;

    private bool _monitorSceneLoading = false;
    private bool _monitorSceneReady = false;
    private bool _allowedStart = false;

    private int charactersPerSecond = 200;

    void Start()
    {
        _inputActions = GameManager.Instance.inputActions;
        SwitchCamera(currentScene);
        StartCoroutine(BlinkCaret());
        GameManager.Instance.CurrentLevel = GameManager.Instance.levelDirectory["Level 1"];
        // menuBGM = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.menuBGM, true);
        // menuBGM.start();
        // typingSound = AudioManager.Instance.CreateEventInstance(FMODEvents.Instance.typingSound, false);
        cmds = new Dictionary<string, Action<TypeWriter>> {
            { "start", writer => writer.StartTypeWriter(new List<string>()
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
            }, false, true, charactersPerSecond)
            },
            { "stats", writer => writer.StartTypeWriter(new List<string>()
                {
                    "[Stats]",
                    "Perfect Hacks:     " + GameManager.Instance.stats.perfects,
                    "Good Hacks:        " + GameManager.Instance.stats.good,
                    "Ok Hacks:          " + GameManager.Instance.stats.ok,
                    "Failed Hacks:      " + GameManager.Instance.stats.fails,
                    "Levels Complete:   " + GameManager.Instance.stats.levels
                }, false, true, charactersPerSecond)
            },
            { "levels", writer => writer.StartTypeWriter(new List<string>()
                {
                    "> levels",
                    "Test"
                }, false, true, charactersPerSecond)
            },
            { "nonsense", writer =>
                {
                    StartCoroutine(NonsensePanel(writer));
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

        cmds["stats"].Invoke(sideWriter[0]);
        yield return new WaitUntil(() => sideWriter[0].IsTyping() == true);
        cmds["levels"].Invoke(sideWriter[1]);
        yield return new WaitUntil(() => sideWriter[1].IsTyping() == true);
        cmds["nonsense"].Invoke(sideWriter[2]);
        cmds["start"].Invoke(mainWriter);
        yield return new WaitUntil(() => mainWriter.IsTyping() == true);

        _monitorSceneReady = true;
    }

    private IEnumerator NonsensePanel(TypeWriter writer)
    {
        writer.StartTypeWriter(new List<string>()
        {
            "> stats",
            "Test"
        }, false, true, charactersPerSecond);

        yield return new WaitForSeconds(2.5f);
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
                cmdLine.text = @"C:\ > " + (_showCaret ? "|" : "");
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

        switch (state)
        {
            case MenuScene.Default:
                if (_allowedStart)
                {
                    Debug.Log("Here");
                    if (input.Click.IsPressed())
                    {
                        Debug.Log("here");
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
