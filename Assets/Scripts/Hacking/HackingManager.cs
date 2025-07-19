using UnityEngine;
using System.Collections;

public class HackingManager : MonoBehaviour
{
    private BeatManager _beatManager;
    private NoteManager _noteManager;
    private Canvas _noteCanvas;
    private LevelManager _levelManager;
    [SerializeField] private GameObject _beatManagerPrefab;
    [SerializeField] private GameObject _noteManagerPrefab;
    [SerializeField] private GameObject _noteCanvasPrefab;
    [SerializeField] private GameObject _levelManagerObj;
    private Hackable _currentHackingItem;

    private bool _isHacking = false;

    private int _successPoints = 0;
    private int _failPoints = 0;
    private int _totalNoteCount = 0;
    private int _passPercentage = 100;
    private float _score = 0.0f;

    // music state index
    private int _mainMusicIndex = 1;
    private float _ambientReturn = 1f;
    private float _combatInit = 2f;
    private float _combatReturn = 3f;
    private float _hackingStart = 4f;
    private float _hackingEnd = 5f;

    void Start()
    {
        if (!_beatManagerPrefab || !_noteManagerPrefab || !_noteCanvasPrefab)
        {
            Debug.LogError("Note or beat or canvas prefabs are not set");
            Destroy(gameObject);
            return;
        }

        if (!_levelManagerObj)
        {
            Debug.LogError("Level Manager obj is not linked");
            Destroy(gameObject);
            return;
        }
        _levelManager = _levelManagerObj.GetComponent<LevelManager>();

        Vector3 pos = new Vector3(0, 0, 0);
        _beatManager = Instantiate(_beatManagerPrefab, pos, Quaternion.identity).GetComponent<BeatManager>();
        _noteManager = Instantiate(_noteManagerPrefab, pos, Quaternion.identity).GetComponent<NoteManager>();
        _noteCanvas = Instantiate(_noteCanvasPrefab).GetComponent<Canvas>();
        _noteCanvas.worldCamera = Camera.main;

        _noteManager.Init(this, _noteCanvas);
        _beatManager.StartMusic(_mainMusicIndex);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !_isHacking) // Left click
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            int hackableLayerMask = LayerMask.GetMask("Hackable");

            if (Physics.Raycast(ray, out RaycastHit hit, 10000f, hackableLayerMask))
            {
                Debug.Log($"Hit name: {hit.collider.gameObject.name}");
                if (hit.collider.TryGetComponent<Hackable>(out var hackable))
                {
                    Debug.Log("CLicked on Hackable");
                    hackable.OnClicked();
                }
            }
        }

        // @DEBUG
        if (Input.GetKeyDown(KeyCode.P))
        {
            DestroyHackingSession();
        }

        // @DEBUG MUSIC
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            _beatManager.PlayMXState(_mainMusicIndex, 0f);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            _beatManager.PlayMXState(_mainMusicIndex, _ambientReturn);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            _beatManager.PlayMXState(_mainMusicIndex, _combatInit);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            _beatManager.PlayMXState(_mainMusicIndex, _combatReturn);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            _beatManager.PlayMXState(_mainMusicIndex, _hackingStart);
        }

        // @DEBUG MUSIC
        if (Input.GetKeyDown(KeyCode.Q))
        {
            _beatManager.PlayHackingBeat(_mainMusicIndex, 0f);
        }
        if (Input.GetKeyDown(KeyCode.W))
        {
            _beatManager.PlayHackingBeat(_mainMusicIndex, 1f);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            _beatManager.PlayHackingBeat(_mainMusicIndex, 2f);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            _beatManager.PlayHackingBeat(_mainMusicIndex, 3f);
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            _beatManager.PlayHackingBeat(_mainMusicIndex, 4f);
        }
    }

    void OnDestroy()
    {

    }

    public bool IsHacking() => _isHacking;
    public void SetIsHacking(bool isHacking) => _isHacking = isHacking;

    public void CreateHackingSession(Hackable hackableItem)
    {
        if (IsHacking() || _currentHackingItem)
        {
            return;
        }
        SetIsHacking(true);

        _levelManager.SlowDown();
        StartCoroutine(StartSessionCoroutine(hackableItem));
    }

    private IEnumerator StartSessionCoroutine(Hackable hackableItem)
    {
        _currentHackingItem = hackableItem;

        var hackingBeat = _beatManager.GetRandomBeat();
        Debug.Log($"Beat# = {hackingBeat.trackNumber} with beatCount = {hackingBeat.beatNotes.Count}");

        _successPoints = 0;
        _failPoints = 0;
        _totalNoteCount = hackingBeat.beatNotes.Count;
        _passPercentage = hackableItem.PassPercentage;

        _beatManager.PlayMXState(_mainMusicIndex, _hackingStart);
        yield return new WaitForSecondsRealtime(1.0f); // sort wait intro music into the Beat

        _beatManager.PlayHackingBeat(_mainMusicIndex, hackingBeat.trackNumber);
        _noteManager.StartSession(hackingBeat.beatNotes);
        _currentHackingItem.StartHack();
        yield return null;
    }

    public void DestroyHackingSession()
    {
        if (!IsHacking() || !_currentHackingItem)
        {
            return;
        }
        SetIsHacking(false);

        Debug.LogWarning("Hacking Stopped");

        _levelManager.SpeedUp();
        DestroySession();
    }

    private void DestroySession()
    {
        _beatManager.PlayHackingBeat(_mainMusicIndex, _hackingEnd);
        _beatManager.PlayMXState(_mainMusicIndex, _combatReturn);
        _noteManager.EndSession();
        _currentHackingItem.EndHack();
        DetermineSuccess(_currentHackingItem);
        _currentHackingItem = null;
    }

    public void AddSuccessPoint()
    {
        if (!IsHacking()) return;

        _successPoints++;

        // @TODO: add to progress meter?
    }

    public void AddFailPoint()
    {
        if (!IsHacking()) return;

        _failPoints++;

        //@TODO: get if hackable item instantly fails
    }

    // @DEBUG
    public void PrintPoints()
    {
        Debug.Log($"Success Points: {_successPoints}, Fail Points: {_failPoints}");
        Debug.Log($"Score Percentage: {GetScorePercentage()}");
    }

    public float GetScorePercentage()
    {
        return _successPoints / _totalNoteCount;
    }

    private void DetermineSuccess(Hackable hackableItem)
    {
        float passRate = _passPercentage / 100.0f;
        //PrintPoints();
        if (_score < passRate)
        {
            hackableItem.OnFailedHack();
        }
        else
        {
            hackableItem.OnSuccessfulHack();
        }
    }
}