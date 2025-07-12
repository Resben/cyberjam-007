using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoteManager : MonoBehaviour
{
    [SerializeField] private GameObject _notePrefab;
    [SerializeField] private GameObject _beatManager;

    private Transform _cameraTransform;

    // Terminal window properties
    //private Transform _terminalWindowTransform; // more ideal to have a panel transform passed in
    [SerializeField] private float _windowHeight;
    [SerializeField] private float _windowWidth;

    // Properties for the notes, This should have level difficulty in mind
    [SerializeField] private float _acceptanceWindow; // Time window for accepting a note hit
    [SerializeField] private float _leadWindowTime; // Spawn time before note marker happens

    private float _startTime; // will need this if we decide to use Main Game loop music
    [SerializeField] private float _gracePeriod; // amount of time before notes can appear

    private float _successPoints = 0;
    private float _failPoints = 0;

    private List<Vector3> _spawnLocations = new();
    private float _noteRadius;

    void Start()
    {
        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform;
        }

        if (_beatManager == null)
        {
            _beatManager = GameObject.FindGameObjectWithTag("BeatManager");
            if (_beatManager == null)
            {
                Debug.LogError("Beat Manager not set");
                Destroy(gameObject);
                return;
            }
        }

        _startTime = Time.time;
        _noteRadius = _notePrefab.GetComponent<SphereCollider>().radius;

        SpawnNotes();
    }

    void OnDestroy()
    {
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");

        if (notes.Length == 0) return;

        foreach (GameObject note in notes)
        {
            Destroy(note);
        }
    }

    void SpawnNotes()
    {
        BeatManager beats = _beatManager.GetComponent<BeatManager>();

        foreach (MusicEvent musicEvent in beats.GetMusicData())
        {
            if (musicEvent.data.tags == null || musicEvent.data.tags.Count == 0)
            {
                Debug.LogWarning($"No tags found for music event: {musicEvent.name}");
                continue;
            }

            foreach (BeatNote beatNote in musicEvent.data.tags)
            {
                StartCoroutine(SpawnNoteCoroutine(beatNote.time, beatNote.duration));
            }
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

    public void AddSuccessPoint()
    {
        _successPoints++;
        PrintPoints();
    }

    public void AddFailPoint()
    {
        _failPoints++;
        PrintPoints();
    }

    public void PrintPoints()
    {
        Debug.Log($"Success Points: {_successPoints}, Fail Points: {_failPoints}");
    }

    public float GetScorePercentage()
    {
        float TotalPoints = _successPoints + _failPoints;
        return _successPoints / TotalPoints;
    }
}
