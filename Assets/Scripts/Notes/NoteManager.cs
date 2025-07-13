using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    [SerializeField] private GameObject _notePrefab;
    private Hackable _hackableItem;
    private MusicEvent _musicEvent;

    private Transform _cameraTransform;

    // Terminal window properties
    private Transform _terminalWindowTransform; // more ideal to have a panel transform passed in
    [SerializeField] private float _windowHeight;
    [SerializeField] private float _windowWidth;

    // Properties for the notes, This should have level difficulty in mind
    [SerializeField] private float _acceptanceWindow; // Time window for accepting a note hit
    [SerializeField] private float _leadWindowTime; // Spawn time before note marker happens

    private float _startTime; // will need this if we decide to use Main Game loop music
    [SerializeField] private float _gracePeriod; // amount of time before notes can appear

    private int _successPoints = 0;
    private int _failPoints = 0;
    private float _totalNoteCount = 0;

    private List<Vector3> _spawnLocations = new();
    private float _noteRadius;

    void Start()
    {
        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_musicEvent.data.tags.Count < 1)
        {
            Debug.LogError("Music Data is missing");
            Destroy(gameObject);
            return;
        }

        if (!_hackableItem)
        {
            Debug.LogError("Hackable item is not linked");
            Destroy(gameObject);
            return;
        }

        _startTime = Time.time;
        _noteRadius = _notePrefab.GetComponent<SphereCollider>().radius;

        _totalNoteCount = _musicEvent.data.tags.Count;

        SpawnNotes();

        StartCoroutine(LifetimeCoroutine());
    }

    void OnDestroy()
    {
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");

        if (notes.Length > 0)
        {
            foreach (GameObject note in notes)
                {
                    Destroy(note);
                }
        }

        PrintPoints();
        _hackableItem.EndHack(GetScorePercentage());
    }

    private IEnumerator LifetimeCoroutine()
    {
        // Destroy itself at the end of the music
        yield return new WaitForSecondsRealtime(_musicEvent.length / 1000.0f); // length is in milliseconds

        Destroy(gameObject);
        yield return null;
    }

    public void Initialise(MusicEvent musicEvent, Hackable hackableItem)
    {
        _musicEvent = musicEvent;
        _hackableItem = hackableItem;
    }
    
    void SpawnNotes()
    {
        foreach (BeatNote beatNote in _musicEvent.data.tags)
        {
            StartCoroutine(SpawnNoteCoroutine(beatNote.time, beatNote.duration));
        }
        
    }

    /* 
    * will need this if we decide to use Main Game loop music
    *
    private bool ShouldSpawn(float beatTime)
    {
        float spawnTime = beatTime - _leadWindowTime;
        if (spawnTime < _startTime + _gracePeriod)
        {
            return false; // passed the timing, don't spawn
        }
        return true;
    }
    */

    private IEnumerator SpawnNoteCoroutine(float beatTime, float duration)
    {
        /* 
        * will need this if we decide to use Main Game loop music
        *
        float spawnTime = beatTime - _leadWindowTime;
        float realTimeToWait = spawnTime - _startTime;
    
        yield return new WaitForSecondsRealtime(realTimeToWait); // Wait for the note's time before spawning
        */

        float realTimeToWait = beatTime - _leadWindowTime;

        // Assuming Music starts the same time NoteManager starts
        yield return new WaitForSecondsRealtime(realTimeToWait); // Wait for the note's time before spawning

        GameObject noteObj = Instantiate(_notePrefab, GetRandomPosition(), Quaternion.identity);
        if (!noteObj)
        {
            Debug.LogError("Failed to instantiate note prefab.");
            yield break;
        }

        Note note = noteObj.GetComponent<Note>();
        if (!note)
        {
            Debug.LogError("Note script not found on the note prefab.");
            yield break;
        }

        note.InitializeNote(this, duration, _leadWindowTime, _acceptanceWindow, _cameraTransform);

        yield return null;
    }

    Vector3 GetRandomPosition()
    {
        float x = Random.Range(-_windowWidth * 0.5f, _windowWidth * 0.5f);
        float y = Random.Range(-_windowHeight * 0.5f, _windowHeight * 0.5f);
        Vector3 SpawnLocation = new Vector3(x, y, -20.0f); // Fixed Z position for testing

        bool overlaps = false;
        foreach (var pos in _spawnLocations)
        {
            if (Vector3.Distance(SpawnLocation, pos) < _noteRadius * 2)
            {
                overlaps = true;
            }
        }

        if (overlaps)
        {
            return GetRandomPosition();
        }
        else
        {
            _spawnLocations.Add(SpawnLocation);
            return SpawnLocation;
        }
    }

    public int getSuccessPoints() => _successPoints;
    public void AddSuccessPoint()
    {
        _successPoints++;

        // @TODO: add to progress meter?
    }

    public int getFailPoints() => _failPoints;
    public void AddFailPoint()
    {
        _failPoints++;
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
}
