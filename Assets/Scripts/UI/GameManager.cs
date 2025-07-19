using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public enum GameState
{
    Menu,
    Playing,
    Paused,
    Cutscene,
}

public class Level
{
    public Level(string id, string requiredId, bool isUnlocked, bool isComplete, int difficulty, int buildId, bool underDevelopment, int index, List<string> description)
    {
        this.id = id;
        this.requiredId = requiredId;
        this.isUnlocked = isUnlocked;
        this.isComplete = isComplete;
        this.difficulty = difficulty;
        this.description = description;
        this.buildId = buildId;
        this.index = index;
        this.underDevelopment = underDevelopment;
    }

    public string id;
    public string requiredId;
    public bool isUnlocked;
    public bool isComplete;
    public int difficulty;
    public List<string> description;
    public int buildId;
    public bool underDevelopment;
    public int index;

    public string GetMenuString()
    {
        return id + "   Drone Status: " + (underDevelopment ? "ERROR" : (isUnlocked ? "Active" : "InActive"));
    }
}

public struct Stats
{
    public int perfects;
    public int good;
    public int ok;
    public int fails;
    public int levels;
}

public class GameManager : MonoBehaviour
{
    // ------------------ SCENE / GAMESTATE MANAGEMENT ------------------ //

    public UnityEvent GameResumed;
    public UnityEvent GamePaused;
    public bool exitedLevel = false;
    public bool didWin = false;
    public GameState CurrentGameState = GameState.Menu;
    public Stats stats;
    public Level CurrentLevel;
    public bool isDebugMode = false;
    public PlayerInputActions inputActions;
    private float volume = 0.3f;
    private float sfxVolume = 0.3f;
    private static GameManager _instance;
    private GameState _lastState;

    public LevelManager currentLevelManager;

    public Dictionary<string, Level> levelDirectory = new()
    {
        { "L1", new Level("L1", null, true, false, 1, 1, false, 0, new List<string>() { // Note use 2x empty lines so the play button can be easily inserted
            "Hey so this is level 1",
            "cool",
            "",
            ""
        })},
        { "L2", new Level("L2", "L1", false, false, 2, 2, true, 1, null)},
        { "L3", new Level("L3", "L2", false, false, 3, 3, true, 2, null)},
        { "L4", new Level("L4", "L3", false, false, 4, 4, true, 3, null)},
    };

    public static GameManager Instance
    {
        get
        {
            return _instance;
        }
        private set
        {
            _instance = value;
        }
    }

    private void Awake()
    {
        if (Instance != null)
        {
            return;
        }

        stats = new Stats
        {
            perfects = 0,
            good = 0,
            ok = 0,
            fails = 0,
            levels = 0,
        };

        inputActions = new PlayerInputActions();
        inputActions.Enable();

        DontDestroyOnLoad(gameObject);
        Instance = this;
    }

    private void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Update()
    {
        if (_lastState != CurrentGameState)
        {
            if (CurrentGameState == GameState.Playing && _lastState != GameState.Playing)
                GameResumed?.Invoke();
            if (CurrentGameState != GameState.Playing && _lastState == GameState.Playing)
                GamePaused?.Invoke();
        }

        _lastState = CurrentGameState;
    }

    public void OnLevelWon()
    {
        foreach (var lvl in levelDirectory.Values)
        {
            if (lvl.requiredId == CurrentLevel.id)
            {
                lvl.isUnlocked = true;
            }

            CurrentLevel.isComplete = true;
        }
    }

    public void LoadGame()
    {
        SceneManager.LoadScene(CurrentLevel.buildId);
        CurrentGameState = GameState.Cutscene;
    }

    public void LoadMainMenu()
    {
        SceneManager.LoadScene(0);
        CurrentGameState = GameState.Menu;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode) { }

    public bool IsGameInPlay()
    {
        if (CurrentGameState == GameState.Playing)
        {
            return true;
        }
        return false;
    }

    public bool IsCutscene()
    {
        return CurrentGameState == GameState.Cutscene;
    }

    public float GetVolume()
    {
        return volume;
    }

    public void SetVolume(float value)
    {
        volume = value;
        AudioManager.Instance.OnVolumeChanged(volume);
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = value;
        AudioManager.Instance.OnSFXVolumeChanged(sfxVolume);
    }

    void OnDestroy()
    {
        // inputActions?.Dispose();
    }
}
