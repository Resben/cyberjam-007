using UnityEngine;

public class HackingManager : MonoBehaviour
{
    private BeatManager _beatManager;
    private NoteManager _noteManager;
    [SerializeField] private GameObject _beatManagerPrefab;
    [SerializeField] private GameObject _noteManagerPrefab;

    private Hackable _currentHackingItem;

    private bool _isHacking = false;

    private int _successPoints = 0;
    private int _failPoints = 0;
    private int _totalNoteCount = 0;
    private int _passPercentage = 100;
    private float _score = 0.0f;

    void Start()
    {
        if (!_beatManagerPrefab)
        {
            Debug.LogError("Note or beat prefabs are not set");
            Destroy(gameObject);
            return;
        }

        Vector3 pos = new Vector3(0, 0, 0);
        _beatManager = Instantiate(_beatManagerPrefab, pos, Quaternion.identity).GetComponent<BeatManager>();
        _noteManager = Instantiate(_noteManagerPrefab, pos, Quaternion.identity).GetComponent<NoteManager>();
    }

    void Update()
    {
        // @DEBUG
        if (Input.GetButtonDown("Jump"))
        {
            DestroyHackingSession();
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

        _currentHackingItem = hackableItem;

        MusicEvent musicEvent = _beatManager.GetMusicData(_currentHackingItem.GetTrackNumber());
        
        if (musicEvent.data.tags.Count < 1)
        {
            Debug.LogError("Music Data is missing");
            return;
        }
        
        _successPoints = 0;
        _failPoints = 0;
        _totalNoteCount = musicEvent.data.tags.Count;
        _passPercentage = hackableItem.PassPercentage;

        _beatManager.StartMusic(_currentHackingItem.GetTrackNumber());
        _noteManager.StartSession(this, musicEvent);
        _currentHackingItem.StartHack();
    }

    public void DestroyHackingSession()
    {
        if (!IsHacking() || !_currentHackingItem)
        {
            return;
        }
        SetIsHacking(false);

        _beatManager.ImmediatelyStopMusic(_currentHackingItem.GetTrackNumber());
        _noteManager.EndSession();
        Debug.Log($"Total score = {_score}");
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
        Debug.Log($"Score: {_score}, PassRate: {passRate}");
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