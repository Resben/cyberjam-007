using System.Collections.Generic;
using UnityEngine;
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
    public Level(string id, string requiredId, bool isUnlocked, bool isComplete, int difficulty, List<string> description, int buildId, bool underDevelopment)
    {
        this.id = id;
        this.requiredId = requiredId;
        this.isUnlocked = isUnlocked;
        this.isComplete = isComplete;
        this.difficulty = difficulty;
        this.description = description;
        this.buildId = buildId;
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

    public string GetMenuString()
    {
        return id + "      Status: " + (underDevelopment ? "In Development      " : (isUnlocked ? "Active              " : "Inactive            ")) + "| Level Difficulty: " + difficulty;
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

    public bool exitedLevel = false;
    public GameState CurrentGameState = GameState.Menu;
    public Stats stats;
    public Level CurrentLevel;
    public bool isDebugMode = false;
    public PlayerInputActions inputActions;
    private float volume = 1f;
    private float sfxVolume = 1f;
    private static GameManager _instance;

    public Dictionary<string, Level> levelDirectory = new Dictionary<string, Level>
    {
        { "Level 1", new Level("Level 1", null, true, false, 1, null, 1, false)},
        { "Level 2", new Level("Level 2", "Level 1", false, false, 2, null, 2, true)},
        { "Level 3", new Level("Level 3", "Level 2", false, false, 3, null, 3, true)},
        { "Level 4", new Level("Level 4", "Level 3", false, false, 4, null, 4, true)},
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
            Destroy(gameObject);
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
        volume = Mathf.Clamp01(value);
        AudioManager.Instance.OnVolumeChanged(volume);
    }

    public float GetSFXVolume()
    {
        return sfxVolume;
    }

    public void SetSFXVolume(float value)
    {
        sfxVolume = Mathf.Clamp01(value);
        AudioManager.Instance.OnSFXVolumeChanged(sfxVolume);
    }

    void OnDestroy()
    {
        inputActions.Dispose();
    }
}
