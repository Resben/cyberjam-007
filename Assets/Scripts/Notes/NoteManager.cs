using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NoteManager : MonoBehaviour
{
    [SerializeField] private GameObject _notePrefab;
    private HackingManager _hackingManager;
    private Canvas _noteCanvas;
    private EventSystem _eventSystem;
    private GraphicRaycaster _GraphicRaycaster;

    private RectTransform _canvasRectTransform;
    private Transform _cameraTransform;

    private float _windowHeight;
    private float _windowWidth;

    // Properties for the notes, This should have level difficulty in mind
    [SerializeField] private float _acceptanceWindow; // Time window for accepting a note hit
    [SerializeField] private float _leadWindowTime; // Spawn time before note marker happens

    //private float _startTime; // will need this if we decide to use Main Game loop music
    //[SerializeField] private float _gracePeriod; // amount of time before notes can appear

    private List<Vector2> _spawnLocations = new();
    private Vector2 _noteRadius;

    private float _beatDuration = 20.75f; // hard coded length of each beat track

    void Start()
    {
        if (_cameraTransform == null)
        {
            _cameraTransform = Camera.main.transform;
        }

        _noteRadius = _notePrefab.GetComponent<RectTransform>().rect.size;

        _eventSystem = GameObject.FindGameObjectWithTag("EventSystem").GetComponent<EventSystem>();
        if (!_eventSystem)
        {
            Debug.LogError("Cannot link Event System");
            Destroy(gameObject);
            return;
        }
        
        if (!_hackingManager)
        {
            Debug.LogError("Hacking manager is not linked");
            Destroy(gameObject);
            return;
        }

        if (!_noteCanvas)
        {
            Debug.LogError("Canvas is not linked");
            Destroy(gameObject);
            return;
        }

        _GraphicRaycaster = _noteCanvas.GetComponent<GraphicRaycaster>();
        if (!_GraphicRaycaster)
        {
            Debug.LogError("Cannot link GraphicRaycaster");
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        EndSession();
    }

    void Update()
    {
        ClickHandler();
    }

    private void ClickHandler()
    {
        if (_hackingManager.IsHacking())
        {
            if (Input.GetMouseButtonDown(0))
            {
                PointerEventData pointerData = new PointerEventData(_eventSystem)
                {
                    position = Input.mousePosition
                };

                List<RaycastResult> results = new List<RaycastResult>();
                _GraphicRaycaster.Raycast(pointerData, results);

                foreach (var result in results)
                {
                    if (result.gameObject.TryGetComponent<ParticleNote>(out var particleNote))
                    {
                        particleNote.OnNoteClicked();
                    }
                }
            }
        }
    }

    public void Init(HackingManager hackingManager, Canvas canvas)
    {
        _hackingManager = hackingManager;
        _noteCanvas = canvas;
        float paddingFromEdge = 150;
        _windowWidth = _noteCanvas.GetComponent<RectTransform>().rect.width - paddingFromEdge;
        _windowHeight = _noteCanvas.GetComponent<RectTransform>().rect.height - paddingFromEdge;
        _canvasRectTransform = _noteCanvas.GetComponent<RectTransform>();
    }

    public void StartSession(List<BeatNote> beatNotes)
    {
        if (beatNotes.Count < 1)
        {
            Debug.LogError("Music Data is missing");
            Destroy(gameObject);
            return;
        }

        SpawnNotes(beatNotes);

        StartCoroutine(LifetimeCoroutine());
    }

    public void EndSession()
    {
        GameObject[] notes = GameObject.FindGameObjectsWithTag("Note");

        if (notes.Length > 0)
        {
            foreach (GameObject note in notes)
            {
                Destroy(note);
            }
        }

        StopAllCoroutines();
    }

    private IEnumerator LifetimeCoroutine()
    {
        // Destroy itself at the end of the music
        // float realTimeToWait = _musicEvent.length / 1000.0f; // length is in milliseconds
        // yield return new WaitForSecondsRealtime(realTimeToWait); 

        yield return new WaitForSecondsRealtime(_beatDuration); // got lazy, cannot get time from section currently

        _hackingManager.DestroyHackingSession();
        yield return null;
    }
    
    void SpawnNotes(List<BeatNote> Beat)
    {
        Debug.Log($"Spawncount = {Beat.Count}");
        foreach (BeatNote beatNote in Beat)
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

        GameObject noteObj = Instantiate(_notePrefab, _canvasRectTransform, false);
        if (!noteObj)
        {
            Debug.LogError("Failed to instantiate note prefab.");
            yield break;
        }
        RectTransform noteRectTransform = noteObj.GetComponent<RectTransform>();
        noteRectTransform.anchoredPosition = GetRandomPosition();

        ParticleNote note = noteObj.GetComponent<ParticleNote>();
        if (!note)
        {
            Debug.LogError("Note script not found on the note prefab.");
            yield break;
        }

        note.InitializeNote(this, duration, _leadWindowTime, _acceptanceWindow);

        yield return null;
    }

    Vector2 GetRandomPosition()
    {
        // @TODO: Rework this to spawn on a HUD on the screen space
        float x = Random.Range(-_windowWidth * 0.5f, _windowWidth * 0.5f);
        float y = Random.Range(-_windowHeight * 0.5f, _windowHeight * 0.5f);
        Vector2 SpawnLocation = new Vector2(x, y);

        bool overlaps = false;
        foreach (var pos in _spawnLocations)
        {
            if (Vector2.Distance(SpawnLocation, pos) < _noteRadius.x)
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

    public void RemoveLocationSpawn(Vector2 location)
    {
        if (!_spawnLocations.Remove(location))
        {
            Debug.LogWarning("Cannot remove location from list");
        }
    }

    public void AddSuccessPoint()
    {
        _hackingManager.AddSuccessPoint();
    }

    public void AddFailPoint()
    {
        _hackingManager.AddFailPoint();
    }
}
